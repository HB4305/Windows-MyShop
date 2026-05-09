-- Apply this in Supabase after the schema update.
-- Old function no longer used by the app:
--   public.get_product_sales_by_day(timestamptz, timestamptz, text, text)
--
-- New function used by ReportRepository for the Product Sales line chart:
--   public.get_filtered_product_sales_by_day(timestamptz, timestamptz, text, text)

drop function if exists public.get_product_sales_by_day(timestamptz, timestamptz, text, text);

create or replace function public.get_filtered_product_sales_by_day(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_keyword text default null
)
returns table (
  day timestamp,
  quantity_sold int,
  gross_revenue numeric(12, 2)
)
language sql
stable
as $$
  with params as (
    select
      nullif(btrim(p_category_name), '') as category_filter,
      nullif(btrim(p_keyword), '') as keyword_filter
  ),
  filtered_sales as (
    select
      date_trunc('day', co.created_at)::timestamp as day,
      od.quantity,
      (od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue
    from customerorders co
    join orderdetails od on co.id = od.order_id
    join sportitems si on si.id = od.item_id
    left join categories c on c.id = si.category_id
    cross join params p
    where co.status in ('Completed', 'Delivered')
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (p.category_filter is null or c.name = p.category_filter)
      and (
        p.keyword_filter is null
        or si.name ilike ('%' || p.keyword_filter || '%')
        or coalesce(c.name, '') ilike ('%' || p.keyword_filter || '%')
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
