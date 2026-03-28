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
    where co.status = 'Completed'
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (
        (p_product_name is not null and si.name = p_product_name)
        or
        (p_product_name is null and p_category_name is not null and c.name = p_category_name)
        or
        (p_product_name is null and p_category_name is null)
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

create or replace function public.get_product_sales_by_week(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null
)
returns table (
  start_date timestamp,
  end_date timestamp,
  quantity_sold int,
  gross_revenue numeric(12, 2)
)
language sql
as $$
  with filtered_sales as (
    select
      date_trunc('week', co.created_at)::timestamp as start_date,
      (date_trunc('week', co.created_at) + interval '1 week')::timestamp as end_date,
      c.name as category_name,
      si.name as product_name,
      od.quantity,
      (od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue
    from customerorders co
    join orderdetails od on co.id = od.order_id
    join sportitems si on si.id = od.item_id
    left join categories c on c.id = si.category_id
    where co.status = 'Completed'
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (
        (p_product_name is not null and si.name = p_product_name)
        or
        (p_product_name is null and p_category_name is not null and c.name = p_category_name)
        or
        (p_product_name is null and p_category_name is null)
      )
  )
  select
    start_date,
    end_date,
    sum(quantity)::int as quantity_sold,
    sum(gross_revenue)::numeric(12, 2) as gross_revenue
  from filtered_sales
  group by start_date, end_date
  order by start_date;
$$;

create or replace function public.get_product_sales_by_month(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null
)
returns table (
  year int,
  month int,
  quantity_sold int,
  gross_revenue numeric(12, 2)
)
language sql
as $$
  with filtered_sales as (
    select
      extract(year from co.created_at)::int as year,
      extract(month from co.created_at)::int as month,
      c.name as category_name,
      si.name as product_name,
      od.quantity,
      (od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue
    from customerorders co
    join orderdetails od on co.id = od.order_id
    join sportitems si on si.id = od.item_id
    left join categories c on c.id = si.category_id
    where co.status = 'Completed'
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (
        (p_product_name is not null and si.name = p_product_name)
        or
        (p_product_name is null and p_category_name is not null and c.name = p_category_name)
        or
        (p_product_name is null and p_category_name is null)
      )
  )
  select
    year,
    month,
    sum(quantity)::int as quantity_sold,
    sum(gross_revenue)::numeric(12, 2) as gross_revenue
  from filtered_sales
  group by year, month
  order by year, month;
$$;

create or replace function public.get_product_sales_by_year(
  p_start_date timestamptz,
  p_end_date timestamptz,
  p_category_name text default null,
  p_product_name text default null
)
returns table (
  year int,
  quantity_sold int,
  gross_revenue numeric(12, 2)
)
language sql
as $$
  with filtered_sales as (
    select
      extract(year from co.created_at)::int as year,
      c.name as category_name,
      si.name as product_name,
      od.quantity,
      (od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue
    from customerorders co
    join orderdetails od on co.id = od.order_id
    join sportitems si on si.id = od.item_id
    left join categories c on c.id = si.category_id
    where co.status = 'Completed'
      and co.created_at >= p_start_date
      and co.created_at < p_end_date
      and (
        (p_product_name is not null and si.name = p_product_name)
        or
        (p_product_name is null and p_category_name is not null and c.name = p_category_name)
        or
        (p_product_name is null and p_category_name is null)
      )
  )
  select
    year,
    sum(quantity)::int as quantity_sold,
    sum(gross_revenue)::numeric(12, 2) as gross_revenue
  from filtered_sales
  group by year
  order by year;
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
  image_url text,
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
    coalesce(si.image_url, '') as image_url,
    sum(od.quantity)::int as total_quantity_sold,
    sum(od.quantity * od.unit_price)::numeric(12, 2) as gross_revenue,
    sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0)))::numeric(12, 2) as profit
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status = 'Completed'
    and co.created_at >= p_start_date
    and co.created_at < p_end_date
    and (
      (p_product_name is not null and si.name = p_product_name)
      or
      (p_product_name is null and p_category_name is not null and c.name = p_category_name)
      or
      (p_product_name is null and p_category_name is null)
    )
  group by si.id, si.name, c.name, si.image_url
  order by total_quantity_sold desc, gross_revenue desc, profit desc, si.name asc
  limit p_limit;
$$;
