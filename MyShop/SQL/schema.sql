-- WARNING: This schema is for context only and is not meant to be run.
-- Table order and constraints may not be valid for execution.

CREATE TABLE public._backup_sportitems_20260404 (
  id integer,
  category_id integer,
  name text,
  sku text,
  size text,
  color text,
  cost_price numeric,
  selling_price numeric,
  stock_quantity integer,
  low_stock_threshold integer,
  image_urls ARRAY
);
CREATE TABLE public.categories (
  id integer NOT NULL DEFAULT nextval('categories_id_seq'::regclass),
  name text NOT NULL,
  description text,
  CONSTRAINT categories_pkey PRIMARY KEY (id)
);
CREATE TABLE public.customerorders (
  id integer NOT NULL DEFAULT nextval('customerorders_id_seq'::regclass),
  created_at timestamp with time zone DEFAULT now(),
  customer_name text NOT NULL,
  customer_phone text NOT NULL,
  shipping_address text,
  order_type text CHECK (order_type = ANY (ARRAY['AtStore'::text, 'Delivery'::text])),
  status text DEFAULT 'Pending'::text,
  payment_status text DEFAULT 'Unpaid'::text,
  total_amount numeric DEFAULT 0,
  notes text,
  seller_id integer,
  seller_name character varying NOT NULL DEFAULT ''::character varying,
  customer_id integer,
  payment_method text CHECK (payment_method = ANY (ARRAY['Cash'::text, 'BankTransfer'::text, 'COD'::text])),
  received_amount numeric DEFAULT 0,
  shift_id integer,
  CONSTRAINT customerorders_pkey PRIMARY KEY (id),
  CONSTRAINT customerorders_seller_id_fkey FOREIGN KEY (seller_id) REFERENCES public.users(id),
  CONSTRAINT customerorders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id)
);
CREATE TABLE public.customers (
  id integer NOT NULL DEFAULT nextval('customers_id_seq'::regclass),
  name character varying NOT NULL,
  phone character varying NOT NULL UNIQUE,
  address text,
  created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT customers_pkey PRIMARY KEY (id)
);
CREATE TABLE public.orderdetails (
  id integer NOT NULL DEFAULT nextval('orderdetails_id_seq'::regclass),
  order_id integer,
  item_id integer,
  quantity integer NOT NULL,
  unit_price numeric NOT NULL,
  item_name text,
  variant_id bigint,
  size text,
  color text,
  CONSTRAINT orderdetails_pkey PRIMARY KEY (id),
  CONSTRAINT orderdetails_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.customerorders(id),
  CONSTRAINT orderdetails_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.sportitems(id)
);
CREATE TABLE public.shifts (
  id integer GENERATED ALWAYS AS IDENTITY NOT NULL,
  user_id integer NOT NULL,
  start_time timestamp with time zone DEFAULT now(),
  end_time timestamp with time zone,
  starting_cash numeric DEFAULT 0,
  actual_cash_total numeric DEFAULT 0,
  notes text,
  status text DEFAULT 'Open'::text,
  CONSTRAINT shifts_pkey PRIMARY KEY (id),
  CONSTRAINT shifts_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id)
);
CREATE TABLE public.sportitem_variants (
  id bigint NOT NULL DEFAULT nextval('sportitem_variants_id_seq'::regclass),
  sportitem_id integer NOT NULL,
  size text,
  color text,
  stock_quantity integer NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
  sku text,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT sportitem_variants_pkey PRIMARY KEY (id),
  CONSTRAINT sportitem_variants_sportitem_id_fkey FOREIGN KEY (sportitem_id) REFERENCES public.sportitems(id),
  CONSTRAINT fk_sportitem_variants_sportitem FOREIGN KEY (sportitem_id) REFERENCES public.sportitems(id)
);
CREATE TABLE public.sportitems (
  id integer NOT NULL DEFAULT nextval('sportitems_id_seq'::regclass),
  category_id integer,
  name text NOT NULL,
  cost_price numeric DEFAULT 0,
  selling_price numeric DEFAULT 0,
  stock_quantity integer DEFAULT 0,
  low_stock_threshold integer DEFAULT 5,
  image_urls ARRAY DEFAULT '{}'::text[],
  description text,
  CONSTRAINT sportitems_pkey PRIMARY KEY (id),
  CONSTRAINT sportitems_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(id)
);
CREATE TABLE public.suppliers (
  id integer NOT NULL DEFAULT nextval('suppliers_id_seq'::regclass),
  name text NOT NULL,
  contact_phone text,
  supplier_type text,
  CONSTRAINT suppliers_pkey PRIMARY KEY (id)
);
CREATE TABLE public.supplydetails (
  id integer NOT NULL DEFAULT nextval('supplydetails_id_seq'::regclass),
  supply_id integer,
  item_id integer,
  quantity integer NOT NULL,
  import_price numeric NOT NULL,
  variant_id integer,
  CONSTRAINT supplydetails_pkey PRIMARY KEY (id),
  CONSTRAINT supplydetails_supply_id_fkey FOREIGN KEY (supply_id) REFERENCES public.supplyorders(id),
  CONSTRAINT supplydetails_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.sportitems(id),
  CONSTRAINT fk_supplydetails_variant FOREIGN KEY (variant_id) REFERENCES public.sportitem_variants(id)
);
CREATE TABLE public.supplyorders (
  id integer NOT NULL DEFAULT nextval('supplyorders_id_seq'::regclass),
  supplier_id integer,
  import_date timestamp with time zone DEFAULT now(),
  total_cost numeric DEFAULT 0,
  CONSTRAINT supplyorders_pkey PRIMARY KEY (id),
  CONSTRAINT supplyorders_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id)
);
CREATE TABLE public.users (
  id integer NOT NULL DEFAULT nextval('users_id_seq'::regclass),
  email character varying NOT NULL UNIQUE,
  password character varying NOT NULL,
  role character varying NOT NULL DEFAULT 'sale'::character varying,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT users_pkey PRIMARY KEY (id)
);