# How to run the seed data in Supabase

Run the files **in this exact order** in the Supabase SQL Editor (paste each file content and click Run):

1. `seed.sql` — users, categories, suppliers, products (sportitems), variants
2. `seed_part2_customers_shifts.sql` — 60 customers + ~86 shifts (Jun 2024 – May 2025)
3. `seed_part3_supply.sql` — 20 supply orders + supply details (import history)
4. `seed_part4_orders.sql` — 402 customer orders across 12 months
5. `seed_part5_orderdetails.sql` — order line items (links orders → products)

## Data summary
- 20 products (Nike, Adidas, Puma, New Balance) across 7 categories
- 60 customers with Vietnamese addresses
- 4 staff users (1 admin + 3 sales)
- 402 orders with realistic seasonal patterns:
  - Jun–Sep 2024: ~24–36 orders/month (normal)
  - Oct–Nov 2024: ~44–48 orders/month (surge + Black Friday)
  - Dec 2024: 52 orders (Christmas + year-end peak)
  - Jan 2025: 40 orders (Tết peak)
  - Feb–May 2025: 26–30 orders/month (steady)
- Real product images from Nike/Adidas/Puma/New Balance CDNs
