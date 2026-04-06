-- Enforce sportitems.stock_quantity = SUM(sportitem_variants.stock_quantity)
-- PostgreSQL idempotent migration script.

begin;

-- 1) Ensure relationship exists: sportitem_variants.sportitem_id -> sportitems.id
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint c
        JOIN pg_class t ON t.oid = c.conrelid
        JOIN pg_namespace n ON n.oid = t.relnamespace
        WHERE c.contype = 'f'
          AND c.conname = 'fk_sportitem_variants_sportitem'
          AND n.nspname = 'public'
          AND t.relname = 'sportitem_variants'
    ) THEN
        ALTER TABLE public.sportitem_variants
        ADD CONSTRAINT fk_sportitem_variants_sportitem
        FOREIGN KEY (sportitem_id)
        REFERENCES public.sportitems(id)
        ON DELETE CASCADE;
    END IF;
END $$;

-- 2) Keep variant stock non-negative.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint c
        JOIN pg_class t ON t.oid = c.conrelid
        JOIN pg_namespace n ON n.oid = t.relnamespace
        WHERE c.contype = 'c'
          AND c.conname = 'sportitem_variants_stock_non_negative'
          AND n.nspname = 'public'
          AND t.relname = 'sportitem_variants'
    ) THEN
        ALTER TABLE public.sportitem_variants
        ADD CONSTRAINT sportitem_variants_stock_non_negative
        CHECK (stock_quantity >= 0);
    END IF;
END $$;

-- 3) Function to sync one parent row from all of its variants.
create or replace function public.refresh_sportitem_stock_from_variants(p_item_id int)
returns void
language plpgsql
as $$
declare
    v_total int;
begin
    select coalesce(sum(greatest(coalesce(v.stock_quantity, 0), 0)), 0)::int
      into v_total
      from public.sportitem_variants v
     where v.sportitem_id = p_item_id;

    update public.sportitems s
       set stock_quantity = v_total
     where s.id = p_item_id;
end;
$$;

-- 4) Trigger on variant changes -> refresh affected parent(s).
create or replace function public.trg_refresh_parent_stock_from_variants()
returns trigger
language plpgsql
as $$
begin
    if tg_op = 'DELETE' then
        perform public.refresh_sportitem_stock_from_variants(old.sportitem_id);
        return old;
    end if;

    perform public.refresh_sportitem_stock_from_variants(new.sportitem_id);

    if tg_op = 'UPDATE' and old.sportitem_id is distinct from new.sportitem_id then
        perform public.refresh_sportitem_stock_from_variants(old.sportitem_id);
    end if;

    return new;
end;
$$;

drop trigger if exists sportitem_variants_refresh_parent_stock on public.sportitem_variants;

create trigger sportitem_variants_refresh_parent_stock
after insert or update or delete on public.sportitem_variants
for each row execute function public.trg_refresh_parent_stock_from_variants();

-- 5) Guard parent table so stock_quantity cannot drift from variant sum.
create or replace function public.trg_force_parent_stock_from_variants()
returns trigger
language plpgsql
as $$
begin
    -- New parent rows start at zero; variant inserts will drive real value.
    if tg_op = 'INSERT' then
        new.stock_quantity := 0;
        return new;
    end if;

    -- For updates, recompute from variants and ignore manual edits.
    select coalesce(sum(greatest(coalesce(v.stock_quantity, 0), 0)), 0)::int
      into new.stock_quantity
      from public.sportitem_variants v
     where v.sportitem_id = new.id;

    return new;
end;
$$;

drop trigger if exists sportitems_force_stock_from_variants on public.sportitems;

create trigger sportitems_force_stock_from_variants
before insert or update of stock_quantity on public.sportitems
for each row execute function public.trg_force_parent_stock_from_variants();

-- 6) One-time full recalculation (backfill/fix existing mismatches).
update public.sportitems s
   set stock_quantity = coalesce(v.total_stock, 0)
  from (
      select sportitem_id, coalesce(sum(greatest(coalesce(stock_quantity, 0), 0)), 0)::int as total_stock
      from public.sportitem_variants
      group by sportitem_id
  ) v
 where s.id = v.sportitem_id;

-- Set items with no variants to zero to satisfy "always sum of variants".
update public.sportitems s
   set stock_quantity = 0
 where not exists (
    select 1
      from public.sportitem_variants v
     where v.sportitem_id = s.id
 );

commit;

-- Optional verification:
-- select s.id, s.name, s.stock_quantity,
--        coalesce(v.total_stock, 0) as variant_sum,
--        (s.stock_quantity = coalesce(v.total_stock, 0)) as is_synced
-- from public.sportitems s
-- left join (
--     select sportitem_id, sum(greatest(coalesce(stock_quantity, 0), 0))::int as total_stock
--     from public.sportitem_variants
--     group by sportitem_id
-- ) v on v.sportitem_id = s.id
-- order by s.id;
