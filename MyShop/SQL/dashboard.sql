create or replace function public.get_top_selling_products(
  p_start timestamptz,
  p_end timestamptz,
  p_prev_start timestamptz,
  p_prev_end timestamptz,
  p_limit int default 5
)
returns table (
  item_id int,
  name text,
  category_name text,
  selling_price numeric,
  image_url text,
  quantity_sold int,
  curr_period_revenue numeric,
  prev_period_revenue numeric
)
language sql
as $$
  with current_period as (
    select
      od.item_id,
      sum(od.quantity)::int as quantity_sold,
      sum(od.quantity * od.unit_price)::numeric as curr_period_revenue
    from orderdetails od
    join customerorders co on co.id = od.order_id
    where co.status = 'Completed'
      and co.created_at >= p_start
      and co.created_at < p_end
    group by od.item_id
  ),
  prev_period as (
    select
      od.item_id,
      sum(od.quantity * od.unit_price)::numeric as prev_period_revenue
    from orderdetails od
    join customerorders co on co.id = od.order_id
    where co.status = 'Completed'
      and co.created_at >= p_prev_start
      and co.created_at < p_prev_end
    group by od.item_id
  )
  select
    si.id as item_id,
    si.name,
    c.name as category_name,
    si.selling_price,
    si.image_url,
    cp.quantity_sold,
    cp.curr_period_revenue,
    coalesce(pp.prev_period_revenue, 0) as prev_period_revenue
  from current_period cp
  join sportitems si on si.id = cp.item_id
  left join categories c on c.id = si.category_id
  left join prev_period pp on pp.item_id = cp.item_id
  order by
    cp.quantity_sold desc,
    cp.curr_period_revenue desc,
    si.name asc
  limit p_limit;
$$;
