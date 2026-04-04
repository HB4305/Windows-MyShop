-- End-to-end migration:
-- 1) Create sportitem_variants table (if missing) and backfill from legacy columns.
-- 2) Dedupe sportitems into parent products.
-- 3) Remap FK references.
-- 4) Drop legacy parent columns sku/size/color.
-- Also remaps referencing tables: orderdetails.item_id, supplydetails.item_id.

begin;

-- 0) Create variant table and baseline indexes/backfill.
create table if not exists public.sportitem_variants (
    id bigserial primary key,
    sportitem_id int not null references public.sportitems(id) on delete cascade,
    size text,
    color text,
    stock_quantity int not null default 0,
    sku text,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint sportitem_variants_stock_non_negative check (stock_quantity >= 0)
);

create unique index if not exists ux_sportitem_variants_combo
    on public.sportitem_variants (sportitem_id, coalesce(size, ''), coalesce(color, ''));

create index if not exists ix_sportitem_variants_item_id
    on public.sportitem_variants (sportitem_id);

-- Backfill only when legacy columns still exist.
do $$
begin
  if exists (
    select 1
    from information_schema.columns
    where table_schema = 'public'
      and table_name = 'sportitems'
      and column_name = 'sku'
  ) then
    insert into public.sportitem_variants (sportitem_id, size, color, stock_quantity, sku)
    select
      s.id,
      s.size,
      s.color,
      greatest(coalesce(s.stock_quantity, 0), 0),
      s.sku
    from public.sportitems s
    where not exists (
      select 1 from public.sportitem_variants v where v.sportitem_id = s.id
    );
  end if;
end $$;

-- 1) Build duplicate mapping: keep smallest id in each (category_id, normalized_name) group.
create temp table tmp_sportitem_dedupe_map on commit drop as
with grouped as (
  select
    category_id,
    lower(trim(name)) as normalized_name,
    min(id) as canonical_id
  from public.sportitems
  group by category_id, lower(trim(name))
  having count(*) > 1
)
select
  s.id as old_id,
  g.canonical_id
from public.sportitems s
join grouped g
  on s.category_id is not distinct from g.category_id
 and lower(trim(s.name)) = g.normalized_name
where s.id <> g.canonical_id;

-- 2) Remap foreign keys referencing sportitems.id.
update public.orderdetails od
set item_id = m.canonical_id
from tmp_sportitem_dedupe_map m
where od.item_id = m.old_id;

update public.supplydetails sd
set item_id = m.canonical_id
from tmp_sportitem_dedupe_map m
where sd.item_id = m.old_id;

-- 3) Merge variants under canonical item ids.
create temp table tmp_merged_variants on commit drop as
select
  coalesce(m.canonical_id, v.sportitem_id) as sportitem_id,
  nullif(trim(v.size), '') as size,
  nullif(trim(v.color), '') as color,
  sum(greatest(coalesce(v.stock_quantity, 0), 0))::int as stock_quantity,
  (array_agg(v.sku order by v.id) filter (where v.sku is not null and trim(v.sku) <> ''))[1] as sku
from public.sportitem_variants v
left join tmp_sportitem_dedupe_map m
  on m.old_id = v.sportitem_id
group by
  coalesce(m.canonical_id, v.sportitem_id),
  nullif(trim(v.size), ''),
  nullif(trim(v.color), '');

-- Replace variants for affected canonical ids.
delete from public.sportitem_variants v
where v.sportitem_id in (
  select old_id from tmp_sportitem_dedupe_map
  union
  select canonical_id from tmp_sportitem_dedupe_map
);

insert into public.sportitem_variants (sportitem_id, size, color, stock_quantity, sku)
select
  mv.sportitem_id,
  mv.size,
  mv.color,
  mv.stock_quantity,
  mv.sku
from tmp_merged_variants mv
where mv.sportitem_id in (
  select distinct canonical_id from tmp_sportitem_dedupe_map
);

-- 4) Delete duplicate parent rows.
delete from public.sportitems s
using tmp_sportitem_dedupe_map m
where s.id = m.old_id;

-- 5) Ensure variant-level SKU uniqueness is enforced.
create unique index if not exists ux_sportitem_variants_sku
on public.sportitem_variants (sku)
where sku is not null and trim(sku) <> '';

-- 6) Replace aggregate function to sync stock only (parent has no sku/size/color columns anymore).
create or replace function public.refresh_sportitem_aggregate(p_item_id int)
returns void
language plpgsql
as $$
declare
    v_total_stock int;
begin
    select coalesce(sum(v.stock_quantity), 0)
    into v_total_stock
    from public.sportitem_variants v
    where v.sportitem_id = p_item_id;

    update public.sportitems
    set stock_quantity = v_total_stock
    where id = p_item_id;
end;
$$;

create or replace function public.trg_refresh_sportitem_aggregate()
returns trigger
language plpgsql
as $$
begin
  if tg_op = 'DELETE' then
    perform public.refresh_sportitem_aggregate(old.sportitem_id);
    return old;
  end if;

  perform public.refresh_sportitem_aggregate(new.sportitem_id);
  if tg_op = 'UPDATE' and old.sportitem_id <> new.sportitem_id then
    perform public.refresh_sportitem_aggregate(old.sportitem_id);
  end if;
  return new;
end;
$$;

drop trigger if exists sportitem_variants_refresh_aggregate
on public.sportitem_variants;

create trigger sportitem_variants_refresh_aggregate
after insert or update or delete on public.sportitem_variants
for each row execute function public.trg_refresh_sportitem_aggregate();

-- Initial refresh for all parent rows.
update public.sportitems s
set stock_quantity = agg.total_stock
from (
  select v.sportitem_id, coalesce(sum(v.stock_quantity), 0) as total_stock
  from public.sportitem_variants v
  group by v.sportitem_id
) agg
where s.id = agg.sportitem_id;

-- 7) Drop old parent columns now owned by variants.
drop view if exists public.view_low_stock_alert;

alter table public.sportitems drop column if exists sku;
alter table public.sportitems drop column if exists size;
alter table public.sportitems drop column if exists color;

-- Recreate low stock view from variant-level inventory.
create view public.view_low_stock_alert as
select
  s.name,
  coalesce(sum(v.stock_quantity), 0)::int as stock_quantity
from public.sportitems s
left join public.sportitem_variants v
  on v.sportitem_id = s.id
group by s.id, s.name, s.low_stock_threshold
having coalesce(sum(v.stock_quantity), 0) <= coalesce(s.low_stock_threshold, 5);

commit;

-- Suggested verification queries:
-- select count(*) as sportitems_count from public.sportitems;
-- select count(*) as variants_count from public.sportitem_variants;
-- select count(*) as dup_groups
-- from (
--   select category_id, lower(trim(name)) as normalized_name
--   from public.sportitems
--   group by category_id, lower(trim(name))
--   having count(*) > 1
-- ) x;
-- select column_name from information_schema.columns
-- where table_schema='public' and table_name='sportitems'
--   and column_name in ('sku','size','color');
