create or replace function public.get_product_sales_by_day(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null
)
returns table (
  day timestamp,
  quantity_sold int,
  gross_revenue numeric(12, 2)
)
language sql
as $$
  with filtered_sales as (
    select
      date_trunc('day', co.created_at)::timestamp as day,
      c.name as category_name,
      si.name as product_name,
      od.quantity,
      (od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue
    from customerorders co
    join orderdetails od on co.id = od.order_id
    join sportitems si on si.id = od.item_id
    left join categories c on c.id = si.category_id
    where co.status = 'Delivered'
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (
        (
          nullif(btrim(p_product_name), '') is not null
          and to_tsvector('simple', coalesce(si.name, ''))
            @@ plainto_tsquery('simple', nullif(btrim(p_product_name), ''))
        )
        or
        (nullif(btrim(p_product_name), '') is null and p_category_name is not null and c.name = p_category_name)
        or
        (nullif(btrim(p_product_name), '') is null and p_category_name is null)
      )
  )
  select
    day,
    sum(quantity)::int as quantity_sold,
    sum(gross_revenue)::numeric(12, 2) as gross_revenue
  from filtered_sales
  group by day
  order by day;
$$;

create or replace function public.get_top_performing_products(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null,
  p_limit int default 5
)
returns table (
  id int,
  product_name text,
  category_name text,
  image_urls text[],
  total_quantity_sold int,
  gross_revenue numeric(12, 2),
  profit numeric(12, 2)
)
language sql
as $$
  select
    si.id,
    si.name as product_name,
    coalesce(c.name, '') as category_name,
    coalesce(si.image_urls, '{}'::text[]) as image_urls,
    sum(od.quantity)::int as total_quantity_sold,
    sum(od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue,
    sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0)))::numeric(12, 2) as profit
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status = 'Delivered'
    and co.created_at >= p_start_date
    and co.created_at < p_end_date
    and (
      (
        nullif(btrim(p_product_name), '') is not null
        and to_tsvector('simple', coalesce(si.name, ''))
          @@ plainto_tsquery('simple', nullif(btrim(p_product_name), ''))
      )
      or
      (nullif(btrim(p_product_name), '') is null and p_category_name is not null and c.name = p_category_name)
      or
      (nullif(btrim(p_product_name), '') is null and p_category_name is null)
    )
  group by si.id, si.name, c.name, si.image_urls
  order by profit desc, gross_revenue desc, total_quantity_sold desc, si.name asc
  limit p_limit;
$$;

create or replace function public.get_report_overview(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null
)
returns table (
  total_revenue numeric(12, 2),
  total_quantity_sold int,
  total_profit numeric(12, 2),
  total_customers int
)
language sql
as $$
  select
    coalesce(sum(od.quantity * od.unit_price), 0)::numeric(12, 2) as total_revenue,
    coalesce(sum(od.quantity), 0)::int as total_quantity_sold,
    coalesce(sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0))), 0)::numeric(12, 2) as total_profit,
    coalesce(count(distinct nullif(co.customer_phone, '')), 0)::int as total_customers
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status = 'Delivered'
    and co.created_at >= p_start_date
    and co.created_at < p_end_date
    and (
      (
        nullif(btrim(p_product_name), '') is not null
        and to_tsvector('simple', coalesce(si.name, ''))
          @@ plainto_tsquery('simple', nullif(btrim(p_product_name), ''))
      )
      or
      (nullif(btrim(p_product_name), '') is null and p_category_name is not null and c.name = p_category_name)
      or
      (nullif(btrim(p_product_name), '') is null and p_category_name is null)
    );
$$;

create or replace function public.get_category_profit(
  p_start_date timestamptz,
  p_end_date timestamptz
)
returns table (
  category_name text,
  profit numeric(12, 2)
)
language sql
as $$
  select
    coalesce(c.name, 'Uncategorized') as category_name,
    coalesce(sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0))), 0)::numeric(12, 2) as profit
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status = 'Delivered'
    and co.created_at >= p_start_date
    and co.created_at < p_end_date
  group by coalesce(c.name, 'Uncategorized')
  order by profit desc, category_name asc;
$$;
