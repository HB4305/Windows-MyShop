--
-- PostgreSQL database dump
--

\restrict gB2BdTU6TlQEaeZ5sbVgYYG9Yctbzt7MQ8r3iKAlCRphoKYxCsWY4yMU8C5spPw

-- Dumped from database version 17.6
-- Dumped by pg_dump version 17.9

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: auth; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA auth;


--
-- Name: extensions; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA extensions;


--
-- Name: graphql; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA graphql;


--
-- Name: graphql_public; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA graphql_public;


--
-- Name: pgbouncer; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA pgbouncer;


--
-- Name: realtime; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA realtime;


--
-- Name: storage; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA storage;


--
-- Name: vault; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA vault;


--
-- Name: pg_stat_statements; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_stat_statements WITH SCHEMA extensions;


--
-- Name: EXTENSION pg_stat_statements; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION pg_stat_statements IS 'track planning and execution statistics of all SQL statements executed';


--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA extensions;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- Name: supabase_vault; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS supabase_vault WITH SCHEMA vault;


--
-- Name: EXTENSION supabase_vault; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION supabase_vault IS 'Supabase Vault Extension';


--
-- Name: unaccent; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;


--
-- Name: EXTENSION unaccent; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION unaccent IS 'text search dictionary that removes accents';


--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA extensions;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


--
-- Name: aal_level; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.aal_level AS ENUM (
    'aal1',
    'aal2',
    'aal3'
);


--
-- Name: code_challenge_method; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.code_challenge_method AS ENUM (
    's256',
    'plain'
);


--
-- Name: factor_status; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.factor_status AS ENUM (
    'unverified',
    'verified'
);


--
-- Name: factor_type; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.factor_type AS ENUM (
    'totp',
    'webauthn',
    'phone'
);


--
-- Name: oauth_authorization_status; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.oauth_authorization_status AS ENUM (
    'pending',
    'approved',
    'denied',
    'expired'
);


--
-- Name: oauth_client_type; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.oauth_client_type AS ENUM (
    'public',
    'confidential'
);


--
-- Name: oauth_registration_type; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.oauth_registration_type AS ENUM (
    'dynamic',
    'manual'
);


--
-- Name: oauth_response_type; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.oauth_response_type AS ENUM (
    'code'
);


--
-- Name: one_time_token_type; Type: TYPE; Schema: auth; Owner: -
--

CREATE TYPE auth.one_time_token_type AS ENUM (
    'confirmation_token',
    'reauthentication_token',
    'recovery_token',
    'email_change_token_new',
    'email_change_token_current',
    'phone_change_token'
);


--
-- Name: action; Type: TYPE; Schema: realtime; Owner: -
--

CREATE TYPE realtime.action AS ENUM (
    'INSERT',
    'UPDATE',
    'DELETE',
    'TRUNCATE',
    'ERROR'
);


--
-- Name: equality_op; Type: TYPE; Schema: realtime; Owner: -
--

CREATE TYPE realtime.equality_op AS ENUM (
    'eq',
    'neq',
    'lt',
    'lte',
    'gt',
    'gte',
    'in'
);


--
-- Name: user_defined_filter; Type: TYPE; Schema: realtime; Owner: -
--

CREATE TYPE realtime.user_defined_filter AS (
	column_name text,
	op realtime.equality_op,
	value text
);


--
-- Name: wal_column; Type: TYPE; Schema: realtime; Owner: -
--

CREATE TYPE realtime.wal_column AS (
	name text,
	type_name text,
	type_oid oid,
	value jsonb,
	is_pkey boolean,
	is_selectable boolean
);


--
-- Name: wal_rls; Type: TYPE; Schema: realtime; Owner: -
--

CREATE TYPE realtime.wal_rls AS (
	wal jsonb,
	is_rls_enabled boolean,
	subscription_ids uuid[],
	errors text[]
);


--
-- Name: buckettype; Type: TYPE; Schema: storage; Owner: -
--

CREATE TYPE storage.buckettype AS ENUM (
    'STANDARD',
    'ANALYTICS',
    'VECTOR'
);


--
-- Name: email(); Type: FUNCTION; Schema: auth; Owner: -
--

CREATE FUNCTION auth.email() RETURNS text
    LANGUAGE sql STABLE
    AS $$
  select 
  coalesce(
    nullif(current_setting('request.jwt.claim.email', true), ''),
    (nullif(current_setting('request.jwt.claims', true), '')::jsonb ->> 'email')
  )::text
$$;


--
-- Name: FUNCTION email(); Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON FUNCTION auth.email() IS 'Deprecated. Use auth.jwt() -> ''email'' instead.';


--
-- Name: jwt(); Type: FUNCTION; Schema: auth; Owner: -
--

CREATE FUNCTION auth.jwt() RETURNS jsonb
    LANGUAGE sql STABLE
    AS $$
  select 
    coalesce(
        nullif(current_setting('request.jwt.claim', true), ''),
        nullif(current_setting('request.jwt.claims', true), '')
    )::jsonb
$$;


--
-- Name: role(); Type: FUNCTION; Schema: auth; Owner: -
--

CREATE FUNCTION auth.role() RETURNS text
    LANGUAGE sql STABLE
    AS $$
  select 
  coalesce(
    nullif(current_setting('request.jwt.claim.role', true), ''),
    (nullif(current_setting('request.jwt.claims', true), '')::jsonb ->> 'role')
  )::text
$$;


--
-- Name: FUNCTION role(); Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON FUNCTION auth.role() IS 'Deprecated. Use auth.jwt() -> ''role'' instead.';


--
-- Name: uid(); Type: FUNCTION; Schema: auth; Owner: -
--

CREATE FUNCTION auth.uid() RETURNS uuid
    LANGUAGE sql STABLE
    AS $$
  select 
  coalesce(
    nullif(current_setting('request.jwt.claim.sub', true), ''),
    (nullif(current_setting('request.jwt.claims', true), '')::jsonb ->> 'sub')
  )::uuid
$$;


--
-- Name: FUNCTION uid(); Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON FUNCTION auth.uid() IS 'Deprecated. Use auth.jwt() -> ''sub'' instead.';


--
-- Name: grant_pg_cron_access(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.grant_pg_cron_access() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  IF EXISTS (
    SELECT
    FROM pg_event_trigger_ddl_commands() AS ev
    JOIN pg_extension AS ext
    ON ev.objid = ext.oid
    WHERE ext.extname = 'pg_cron'
  )
  THEN
    grant usage on schema cron to postgres with grant option;

    alter default privileges in schema cron grant all on tables to postgres with grant option;
    alter default privileges in schema cron grant all on functions to postgres with grant option;
    alter default privileges in schema cron grant all on sequences to postgres with grant option;

    alter default privileges for user supabase_admin in schema cron grant all
        on sequences to postgres with grant option;
    alter default privileges for user supabase_admin in schema cron grant all
        on tables to postgres with grant option;
    alter default privileges for user supabase_admin in schema cron grant all
        on functions to postgres with grant option;

    grant all privileges on all tables in schema cron to postgres with grant option;
    revoke all on table cron.job from postgres;
    grant select on table cron.job to postgres with grant option;
  END IF;
END;
$$;


--
-- Name: FUNCTION grant_pg_cron_access(); Type: COMMENT; Schema: extensions; Owner: -
--

COMMENT ON FUNCTION extensions.grant_pg_cron_access() IS 'Grants access to pg_cron';


--
-- Name: grant_pg_graphql_access(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.grant_pg_graphql_access() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $_$
DECLARE
    func_is_graphql_resolve bool;
BEGIN
    func_is_graphql_resolve = (
        SELECT n.proname = 'resolve'
        FROM pg_event_trigger_ddl_commands() AS ev
        LEFT JOIN pg_catalog.pg_proc AS n
        ON ev.objid = n.oid
    );

    IF func_is_graphql_resolve
    THEN
        -- Update public wrapper to pass all arguments through to the pg_graphql resolve func
        DROP FUNCTION IF EXISTS graphql_public.graphql;
        create or replace function graphql_public.graphql(
            "operationName" text default null,
            query text default null,
            variables jsonb default null,
            extensions jsonb default null
        )
            returns jsonb
            language sql
        as $$
            select graphql.resolve(
                query := query,
                variables := coalesce(variables, '{}'),
                "operationName" := "operationName",
                extensions := extensions
            );
        $$;

        -- This hook executes when `graphql.resolve` is created. That is not necessarily the last
        -- function in the extension so we need to grant permissions on existing entities AND
        -- update default permissions to any others that are created after `graphql.resolve`
        grant usage on schema graphql to postgres, anon, authenticated, service_role;
        grant select on all tables in schema graphql to postgres, anon, authenticated, service_role;
        grant execute on all functions in schema graphql to postgres, anon, authenticated, service_role;
        grant all on all sequences in schema graphql to postgres, anon, authenticated, service_role;
        alter default privileges in schema graphql grant all on tables to postgres, anon, authenticated, service_role;
        alter default privileges in schema graphql grant all on functions to postgres, anon, authenticated, service_role;
        alter default privileges in schema graphql grant all on sequences to postgres, anon, authenticated, service_role;

        -- Allow postgres role to allow granting usage on graphql and graphql_public schemas to custom roles
        grant usage on schema graphql_public to postgres with grant option;
        grant usage on schema graphql to postgres with grant option;
    END IF;

END;
$_$;


--
-- Name: FUNCTION grant_pg_graphql_access(); Type: COMMENT; Schema: extensions; Owner: -
--

COMMENT ON FUNCTION extensions.grant_pg_graphql_access() IS 'Grants access to pg_graphql';


--
-- Name: grant_pg_net_access(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.grant_pg_net_access() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  IF EXISTS (
    SELECT 1
    FROM pg_event_trigger_ddl_commands() AS ev
    JOIN pg_extension AS ext
    ON ev.objid = ext.oid
    WHERE ext.extname = 'pg_net'
  )
  THEN
    IF NOT EXISTS (
      SELECT 1
      FROM pg_roles
      WHERE rolname = 'supabase_functions_admin'
    )
    THEN
      CREATE USER supabase_functions_admin NOINHERIT CREATEROLE LOGIN NOREPLICATION;
    END IF;

    GRANT USAGE ON SCHEMA net TO supabase_functions_admin, postgres, anon, authenticated, service_role;

    IF EXISTS (
      SELECT FROM pg_extension
      WHERE extname = 'pg_net'
      -- all versions in use on existing projects as of 2025-02-20
      -- version 0.12.0 onwards don't need these applied
      AND extversion IN ('0.2', '0.6', '0.7', '0.7.1', '0.8', '0.10.0', '0.11.0')
    ) THEN
      ALTER function net.http_get(url text, params jsonb, headers jsonb, timeout_milliseconds integer) SECURITY DEFINER;
      ALTER function net.http_post(url text, body jsonb, params jsonb, headers jsonb, timeout_milliseconds integer) SECURITY DEFINER;

      ALTER function net.http_get(url text, params jsonb, headers jsonb, timeout_milliseconds integer) SET search_path = net;
      ALTER function net.http_post(url text, body jsonb, params jsonb, headers jsonb, timeout_milliseconds integer) SET search_path = net;

      REVOKE ALL ON FUNCTION net.http_get(url text, params jsonb, headers jsonb, timeout_milliseconds integer) FROM PUBLIC;
      REVOKE ALL ON FUNCTION net.http_post(url text, body jsonb, params jsonb, headers jsonb, timeout_milliseconds integer) FROM PUBLIC;

      GRANT EXECUTE ON FUNCTION net.http_get(url text, params jsonb, headers jsonb, timeout_milliseconds integer) TO supabase_functions_admin, postgres, anon, authenticated, service_role;
      GRANT EXECUTE ON FUNCTION net.http_post(url text, body jsonb, params jsonb, headers jsonb, timeout_milliseconds integer) TO supabase_functions_admin, postgres, anon, authenticated, service_role;
    END IF;
  END IF;
END;
$$;


--
-- Name: FUNCTION grant_pg_net_access(); Type: COMMENT; Schema: extensions; Owner: -
--

COMMENT ON FUNCTION extensions.grant_pg_net_access() IS 'Grants access to pg_net';


--
-- Name: pgrst_ddl_watch(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.pgrst_ddl_watch() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
  cmd record;
BEGIN
  FOR cmd IN SELECT * FROM pg_event_trigger_ddl_commands()
  LOOP
    IF cmd.command_tag IN (
      'CREATE SCHEMA', 'ALTER SCHEMA'
    , 'CREATE TABLE', 'CREATE TABLE AS', 'SELECT INTO', 'ALTER TABLE'
    , 'CREATE FOREIGN TABLE', 'ALTER FOREIGN TABLE'
    , 'CREATE VIEW', 'ALTER VIEW'
    , 'CREATE MATERIALIZED VIEW', 'ALTER MATERIALIZED VIEW'
    , 'CREATE FUNCTION', 'ALTER FUNCTION'
    , 'CREATE TRIGGER'
    , 'CREATE TYPE', 'ALTER TYPE'
    , 'CREATE RULE'
    , 'COMMENT'
    )
    -- don't notify in case of CREATE TEMP table or other objects created on pg_temp
    AND cmd.schema_name is distinct from 'pg_temp'
    THEN
      NOTIFY pgrst, 'reload schema';
    END IF;
  END LOOP;
END; $$;


--
-- Name: pgrst_drop_watch(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.pgrst_drop_watch() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
  obj record;
BEGIN
  FOR obj IN SELECT * FROM pg_event_trigger_dropped_objects()
  LOOP
    IF obj.object_type IN (
      'schema'
    , 'table'
    , 'foreign table'
    , 'view'
    , 'materialized view'
    , 'function'
    , 'trigger'
    , 'type'
    , 'rule'
    )
    AND obj.is_temporary IS false -- no pg_temp objects
    THEN
      NOTIFY pgrst, 'reload schema';
    END IF;
  END LOOP;
END; $$;


--
-- Name: set_graphql_placeholder(); Type: FUNCTION; Schema: extensions; Owner: -
--

CREATE FUNCTION extensions.set_graphql_placeholder() RETURNS event_trigger
    LANGUAGE plpgsql
    AS $_$
    DECLARE
    graphql_is_dropped bool;
    BEGIN
    graphql_is_dropped = (
        SELECT ev.schema_name = 'graphql_public'
        FROM pg_event_trigger_dropped_objects() AS ev
        WHERE ev.schema_name = 'graphql_public'
    );

    IF graphql_is_dropped
    THEN
        create or replace function graphql_public.graphql(
            "operationName" text default null,
            query text default null,
            variables jsonb default null,
            extensions jsonb default null
        )
            returns jsonb
            language plpgsql
        as $$
            DECLARE
                server_version float;
            BEGIN
                server_version = (SELECT (SPLIT_PART((select version()), ' ', 2))::float);

                IF server_version >= 14 THEN
                    RETURN jsonb_build_object(
                        'errors', jsonb_build_array(
                            jsonb_build_object(
                                'message', 'pg_graphql extension is not enabled.'
                            )
                        )
                    );
                ELSE
                    RETURN jsonb_build_object(
                        'errors', jsonb_build_array(
                            jsonb_build_object(
                                'message', 'pg_graphql is only available on projects running Postgres 14 onwards.'
                            )
                        )
                    );
                END IF;
            END;
        $$;
    END IF;

    END;
$_$;


--
-- Name: FUNCTION set_graphql_placeholder(); Type: COMMENT; Schema: extensions; Owner: -
--

COMMENT ON FUNCTION extensions.set_graphql_placeholder() IS 'Reintroduces placeholder function for graphql_public.graphql';


--
-- Name: graphql(text, text, jsonb, jsonb); Type: FUNCTION; Schema: graphql_public; Owner: -
--

CREATE FUNCTION graphql_public.graphql("operationName" text DEFAULT NULL::text, query text DEFAULT NULL::text, variables jsonb DEFAULT NULL::jsonb, extensions jsonb DEFAULT NULL::jsonb) RETURNS jsonb
    LANGUAGE plpgsql
    AS $$
            DECLARE
                server_version float;
            BEGIN
                server_version = (SELECT (SPLIT_PART((select version()), ' ', 2))::float);

                IF server_version >= 14 THEN
                    RETURN jsonb_build_object(
                        'errors', jsonb_build_array(
                            jsonb_build_object(
                                'message', 'pg_graphql extension is not enabled.'
                            )
                        )
                    );
                ELSE
                    RETURN jsonb_build_object(
                        'errors', jsonb_build_array(
                            jsonb_build_object(
                                'message', 'pg_graphql is only available on projects running Postgres 14 onwards.'
                            )
                        )
                    );
                END IF;
            END;
        $$;


--
-- Name: get_auth(text); Type: FUNCTION; Schema: pgbouncer; Owner: -
--

CREATE FUNCTION pgbouncer.get_auth(p_usename text) RETURNS TABLE(username text, password text)
    LANGUAGE plpgsql SECURITY DEFINER
    SET search_path TO ''
    AS $_$
  BEGIN
      RAISE DEBUG 'PgBouncer auth request: %', p_usename;

      RETURN QUERY
      SELECT
          rolname::text,
          CASE WHEN rolvaliduntil < now()
              THEN null
              ELSE rolpassword::text
          END
      FROM pg_authid
      WHERE rolname=$1 and rolcanlogin;
  END;
  $_$;


--
-- Name: get_category_profit(timestamp with time zone, timestamp with time zone); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.get_category_profit(p_start_date timestamp with time zone, p_end_date timestamp with time zone) RETURNS TABLE(category_name text, profit numeric)
    LANGUAGE sql
    AS $$select
    coalesce(c.name, 'Uncategorized') as category_name,
    coalesce(sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0))), 0)::numeric(12, 2) as profit
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status IN ('Delivered', 'Completed')
    and co.created_at >= p_start_date
    and co.created_at < p_end_date
  group by coalesce(c.name, 'Uncategorized')
  order by profit desc, category_name asc;$$;


--
-- Name: get_filtered_product_sales_by_day(timestamp with time zone, timestamp with time zone, text, text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.get_filtered_product_sales_by_day(p_start_date timestamp with time zone, p_end_date timestamp with time zone, p_category_name text DEFAULT NULL::text, p_keyword text DEFAULT NULL::text) RETURNS TABLE(day timestamp without time zone, quantity_sold integer, gross_revenue numeric)
    LANGUAGE sql STABLE
    AS $$
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


--
-- Name: get_report_overview(timestamp with time zone, timestamp with time zone, text, text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.get_report_overview(p_start_date timestamp with time zone, p_end_date timestamp with time zone, p_category_name text DEFAULT NULL::text, p_product_name text DEFAULT NULL::text) RETURNS TABLE(total_revenue numeric, total_quantity_sold integer, total_profit numeric, total_customers integer)
    LANGUAGE sql
    AS $$select
    coalesce(sum(od.quantity * od.unit_price), 0)::numeric(12, 2) as total_revenue,
    coalesce(sum(od.quantity), 0)::int as total_quantity_sold,
    coalesce(sum(od.quantity * (od.unit_price - coalesce(si.cost_price, 0))), 0)::numeric(12, 2) as total_profit,
    coalesce(count(distinct nullif(co.customer_phone, '')), 0)::int as total_customers
  from customerorders co
  join orderdetails od on co.id = od.order_id
  join sportitems si on si.id = od.item_id
  left join categories c on c.id = si.category_id
  where co.status IN ('Delivered', 'Completed')
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
    );$$;


--
-- Name: get_top_performing_products(timestamp with time zone, timestamp with time zone, text, text, integer); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.get_top_performing_products(p_start_date timestamp with time zone, p_end_date timestamp with time zone, p_category_name text DEFAULT NULL::text, p_product_name text DEFAULT NULL::text, p_limit integer DEFAULT 5) RETURNS TABLE(id integer, product_name text, category_name text, image_urls text[], total_quantity_sold integer, gross_revenue numeric, profit numeric)
    LANGUAGE sql
    AS $$select
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
  where co.status IN ('Delivered', 'Completed')
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
  limit p_limit;$$;


--
-- Name: get_top_selling_products(timestamp with time zone, timestamp with time zone, timestamp with time zone, timestamp with time zone, integer); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.get_top_selling_products(p_start timestamp with time zone, p_end timestamp with time zone, p_prev_start timestamp with time zone, p_prev_end timestamp with time zone, p_limit integer DEFAULT 5) RETURNS TABLE(item_id integer, name text, category_name text, selling_price numeric, image_urls text[], quantity_sold integer, curr_period_revenue numeric, prev_period_revenue numeric)
    LANGUAGE sql
    AS $$with current_period as (
    select
      od.item_id,
      sum(od.quantity)::int as quantity_sold,
      sum(od.quantity * od.unit_price)::numeric as curr_period_revenue
    from orderdetails od
    join customerorders co on co.id = od.order_id
    where co.status IN ('Delivered', 'Completed')
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
    where co.status IN ('Delivered', 'Completed')
      and co.created_at >= p_prev_start
      and co.created_at < p_prev_end
    group by od.item_id
  )
  select
    si.id as item_id,
    si.name,
    c.name as category_name,
    si.selling_price,
    coalesce(si.image_urls, '{}'::text[]) as image_urls,
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
  limit p_limit;$$;


--
-- Name: handle_new_user(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.handle_new_user() RETURNS trigger
    LANGUAGE plpgsql SECURITY DEFINER
    AS $$
BEGIN
    INSERT INTO public.profiles (id, full_name)
    VALUES (NEW.id, NEW.raw_user_meta_data->>'full_name');
    RETURN NEW;
END;
$$;


--
-- Name: handle_stock_on_sale(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.handle_stock_on_sale() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE SportItems
    SET stock_quantity = stock_quantity - NEW.quantity
    WHERE id = NEW.item_id;
    RETURN NEW;
END;
$$;


--
-- Name: handle_stock_on_supply(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.handle_stock_on_supply() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE SportItems
    SET stock_quantity = stock_quantity + NEW.quantity
    WHERE id = NEW.item_id;
    RETURN NEW;
END;
$$;


--
-- Name: refresh_sportitem_aggregate(integer); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.refresh_sportitem_aggregate(p_item_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
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


--
-- Name: refresh_sportitem_stock_from_variants(integer); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.refresh_sportitem_stock_from_variants(p_item_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
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


--
-- Name: trg_force_parent_stock_from_variants(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.trg_force_parent_stock_from_variants() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
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


--
-- Name: trg_refresh_parent_stock_from_variants(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.trg_refresh_parent_stock_from_variants() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
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


--
-- Name: trg_refresh_sportitem_aggregate(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.trg_refresh_sportitem_aggregate() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
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


--
-- Name: apply_rls(jsonb, integer); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.apply_rls(wal jsonb, max_record_bytes integer DEFAULT (1024 * 1024)) RETURNS SETOF realtime.wal_rls
    LANGUAGE plpgsql
    AS $$
declare
-- Regclass of the table e.g. public.notes
entity_ regclass = (quote_ident(wal ->> 'schema') || '.' || quote_ident(wal ->> 'table'))::regclass;

-- I, U, D, T: insert, update ...
action realtime.action = (
    case wal ->> 'action'
        when 'I' then 'INSERT'
        when 'U' then 'UPDATE'
        when 'D' then 'DELETE'
        else 'ERROR'
    end
);

-- Is row level security enabled for the table
is_rls_enabled bool = relrowsecurity from pg_class where oid = entity_;

subscriptions realtime.subscription[] = array_agg(subs)
    from
        realtime.subscription subs
    where
        subs.entity = entity_
        -- Filter by action early - only get subscriptions interested in this action
        -- action_filter column can be: '*' (all), 'INSERT', 'UPDATE', or 'DELETE'
        and (subs.action_filter = '*' or subs.action_filter = action::text);

-- Subscription vars
roles regrole[] = array_agg(distinct us.claims_role::text)
    from
        unnest(subscriptions) us;

working_role regrole;
claimed_role regrole;
claims jsonb;

subscription_id uuid;
subscription_has_access bool;
visible_to_subscription_ids uuid[] = '{}';

-- structured info for wal's columns
columns realtime.wal_column[];
-- previous identity values for update/delete
old_columns realtime.wal_column[];

error_record_exceeds_max_size boolean = octet_length(wal::text) > max_record_bytes;

-- Primary jsonb output for record
output jsonb;

begin
perform set_config('role', null, true);

columns =
    array_agg(
        (
            x->>'name',
            x->>'type',
            x->>'typeoid',
            realtime.cast(
                (x->'value') #>> '{}',
                coalesce(
                    (x->>'typeoid')::regtype, -- null when wal2json version <= 2.4
                    (x->>'type')::regtype
                )
            ),
            (pks ->> 'name') is not null,
            true
        )::realtime.wal_column
    )
    from
        jsonb_array_elements(wal -> 'columns') x
        left join jsonb_array_elements(wal -> 'pk') pks
            on (x ->> 'name') = (pks ->> 'name');

old_columns =
    array_agg(
        (
            x->>'name',
            x->>'type',
            x->>'typeoid',
            realtime.cast(
                (x->'value') #>> '{}',
                coalesce(
                    (x->>'typeoid')::regtype, -- null when wal2json version <= 2.4
                    (x->>'type')::regtype
                )
            ),
            (pks ->> 'name') is not null,
            true
        )::realtime.wal_column
    )
    from
        jsonb_array_elements(wal -> 'identity') x
        left join jsonb_array_elements(wal -> 'pk') pks
            on (x ->> 'name') = (pks ->> 'name');

for working_role in select * from unnest(roles) loop

    -- Update `is_selectable` for columns and old_columns
    columns =
        array_agg(
            (
                c.name,
                c.type_name,
                c.type_oid,
                c.value,
                c.is_pkey,
                pg_catalog.has_column_privilege(working_role, entity_, c.name, 'SELECT')
            )::realtime.wal_column
        )
        from
            unnest(columns) c;

    old_columns =
            array_agg(
                (
                    c.name,
                    c.type_name,
                    c.type_oid,
                    c.value,
                    c.is_pkey,
                    pg_catalog.has_column_privilege(working_role, entity_, c.name, 'SELECT')
                )::realtime.wal_column
            )
            from
                unnest(old_columns) c;

    if action <> 'DELETE' and count(1) = 0 from unnest(columns) c where c.is_pkey then
        return next (
            jsonb_build_object(
                'schema', wal ->> 'schema',
                'table', wal ->> 'table',
                'type', action
            ),
            is_rls_enabled,
            -- subscriptions is already filtered by entity
            (select array_agg(s.subscription_id) from unnest(subscriptions) as s where claims_role = working_role),
            array['Error 400: Bad Request, no primary key']
        )::realtime.wal_rls;

    -- The claims role does not have SELECT permission to the primary key of entity
    elsif action <> 'DELETE' and sum(c.is_selectable::int) <> count(1) from unnest(columns) c where c.is_pkey then
        return next (
            jsonb_build_object(
                'schema', wal ->> 'schema',
                'table', wal ->> 'table',
                'type', action
            ),
            is_rls_enabled,
            (select array_agg(s.subscription_id) from unnest(subscriptions) as s where claims_role = working_role),
            array['Error 401: Unauthorized']
        )::realtime.wal_rls;

    else
        output = jsonb_build_object(
            'schema', wal ->> 'schema',
            'table', wal ->> 'table',
            'type', action,
            'commit_timestamp', to_char(
                ((wal ->> 'timestamp')::timestamptz at time zone 'utc'),
                'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'
            ),
            'columns', (
                select
                    jsonb_agg(
                        jsonb_build_object(
                            'name', pa.attname,
                            'type', pt.typname
                        )
                        order by pa.attnum asc
                    )
                from
                    pg_attribute pa
                    join pg_type pt
                        on pa.atttypid = pt.oid
                where
                    attrelid = entity_
                    and attnum > 0
                    and pg_catalog.has_column_privilege(working_role, entity_, pa.attname, 'SELECT')
            )
        )
        -- Add "record" key for insert and update
        || case
            when action in ('INSERT', 'UPDATE') then
                jsonb_build_object(
                    'record',
                    (
                        select
                            jsonb_object_agg(
                                -- if unchanged toast, get column name and value from old record
                                coalesce((c).name, (oc).name),
                                case
                                    when (c).name is null then (oc).value
                                    else (c).value
                                end
                            )
                        from
                            unnest(columns) c
                            full outer join unnest(old_columns) oc
                                on (c).name = (oc).name
                        where
                            coalesce((c).is_selectable, (oc).is_selectable)
                            and ( not error_record_exceeds_max_size or (octet_length((c).value::text) <= 64))
                    )
                )
            else '{}'::jsonb
        end
        -- Add "old_record" key for update and delete
        || case
            when action = 'UPDATE' then
                jsonb_build_object(
                        'old_record',
                        (
                            select jsonb_object_agg((c).name, (c).value)
                            from unnest(old_columns) c
                            where
                                (c).is_selectable
                                and ( not error_record_exceeds_max_size or (octet_length((c).value::text) <= 64))
                        )
                    )
            when action = 'DELETE' then
                jsonb_build_object(
                    'old_record',
                    (
                        select jsonb_object_agg((c).name, (c).value)
                        from unnest(old_columns) c
                        where
                            (c).is_selectable
                            and ( not error_record_exceeds_max_size or (octet_length((c).value::text) <= 64))
                            and ( not is_rls_enabled or (c).is_pkey ) -- if RLS enabled, we can't secure deletes so filter to pkey
                    )
                )
            else '{}'::jsonb
        end;

        -- Create the prepared statement
        if is_rls_enabled and action <> 'DELETE' then
            if (select 1 from pg_prepared_statements where name = 'walrus_rls_stmt' limit 1) > 0 then
                deallocate walrus_rls_stmt;
            end if;
            execute realtime.build_prepared_statement_sql('walrus_rls_stmt', entity_, columns);
        end if;

        visible_to_subscription_ids = '{}';

        for subscription_id, claims in (
                select
                    subs.subscription_id,
                    subs.claims
                from
                    unnest(subscriptions) subs
                where
                    subs.entity = entity_
                    and subs.claims_role = working_role
                    and (
                        realtime.is_visible_through_filters(columns, subs.filters)
                        or (
                          action = 'DELETE'
                          and realtime.is_visible_through_filters(old_columns, subs.filters)
                        )
                    )
        ) loop

            if not is_rls_enabled or action = 'DELETE' then
                visible_to_subscription_ids = visible_to_subscription_ids || subscription_id;
            else
                -- Check if RLS allows the role to see the record
                perform
                    -- Trim leading and trailing quotes from working_role because set_config
                    -- doesn't recognize the role as valid if they are included
                    set_config('role', trim(both '"' from working_role::text), true),
                    set_config('request.jwt.claims', claims::text, true);

                execute 'execute walrus_rls_stmt' into subscription_has_access;

                if subscription_has_access then
                    visible_to_subscription_ids = visible_to_subscription_ids || subscription_id;
                end if;
            end if;
        end loop;

        perform set_config('role', null, true);

        return next (
            output,
            is_rls_enabled,
            visible_to_subscription_ids,
            case
                when error_record_exceeds_max_size then array['Error 413: Payload Too Large']
                else '{}'
            end
        )::realtime.wal_rls;

    end if;
end loop;

perform set_config('role', null, true);
end;
$$;


--
-- Name: broadcast_changes(text, text, text, text, text, record, record, text); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.broadcast_changes(topic_name text, event_name text, operation text, table_name text, table_schema text, new record, old record, level text DEFAULT 'ROW'::text) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    -- Declare a variable to hold the JSONB representation of the row
    row_data jsonb := '{}'::jsonb;
BEGIN
    IF level = 'STATEMENT' THEN
        RAISE EXCEPTION 'function can only be triggered for each row, not for each statement';
    END IF;
    -- Check the operation type and handle accordingly
    IF operation = 'INSERT' OR operation = 'UPDATE' OR operation = 'DELETE' THEN
        row_data := jsonb_build_object('old_record', OLD, 'record', NEW, 'operation', operation, 'table', table_name, 'schema', table_schema);
        PERFORM realtime.send (row_data, event_name, topic_name);
    ELSE
        RAISE EXCEPTION 'Unexpected operation type: %', operation;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Failed to process the row: %', SQLERRM;
END;

$$;


--
-- Name: build_prepared_statement_sql(text, regclass, realtime.wal_column[]); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.build_prepared_statement_sql(prepared_statement_name text, entity regclass, columns realtime.wal_column[]) RETURNS text
    LANGUAGE sql
    AS $$
      /*
      Builds a sql string that, if executed, creates a prepared statement to
      tests retrive a row from *entity* by its primary key columns.
      Example
          select realtime.build_prepared_statement_sql('public.notes', '{"id"}'::text[], '{"bigint"}'::text[])
      */
          select
      'prepare ' || prepared_statement_name || ' as
          select
              exists(
                  select
                      1
                  from
                      ' || entity || '
                  where
                      ' || string_agg(quote_ident(pkc.name) || '=' || quote_nullable(pkc.value #>> '{}') , ' and ') || '
              )'
          from
              unnest(columns) pkc
          where
              pkc.is_pkey
          group by
              entity
      $$;


--
-- Name: cast(text, regtype); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime."cast"(val text, type_ regtype) RETURNS jsonb
    LANGUAGE plpgsql IMMUTABLE
    AS $$
declare
  res jsonb;
begin
  if type_::text = 'bytea' then
    return to_jsonb(val);
  end if;
  execute format('select to_jsonb(%L::'|| type_::text || ')', val) into res;
  return res;
end
$$;


--
-- Name: check_equality_op(realtime.equality_op, regtype, text, text); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.check_equality_op(op realtime.equality_op, type_ regtype, val_1 text, val_2 text) RETURNS boolean
    LANGUAGE plpgsql IMMUTABLE
    AS $$
      /*
      Casts *val_1* and *val_2* as type *type_* and check the *op* condition for truthiness
      */
      declare
          op_symbol text = (
              case
                  when op = 'eq' then '='
                  when op = 'neq' then '!='
                  when op = 'lt' then '<'
                  when op = 'lte' then '<='
                  when op = 'gt' then '>'
                  when op = 'gte' then '>='
                  when op = 'in' then '= any'
                  else 'UNKNOWN OP'
              end
          );
          res boolean;
      begin
          execute format(
              'select %L::'|| type_::text || ' ' || op_symbol
              || ' ( %L::'
              || (
                  case
                      when op = 'in' then type_::text || '[]'
                      else type_::text end
              )
              || ')', val_1, val_2) into res;
          return res;
      end;
      $$;


--
-- Name: is_visible_through_filters(realtime.wal_column[], realtime.user_defined_filter[]); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.is_visible_through_filters(columns realtime.wal_column[], filters realtime.user_defined_filter[]) RETURNS boolean
    LANGUAGE sql IMMUTABLE
    AS $_$
    /*
    Should the record be visible (true) or filtered out (false) after *filters* are applied
    */
        select
            -- Default to allowed when no filters present
            $2 is null -- no filters. this should not happen because subscriptions has a default
            or array_length($2, 1) is null -- array length of an empty array is null
            or bool_and(
                coalesce(
                    realtime.check_equality_op(
                        op:=f.op,
                        type_:=coalesce(
                            col.type_oid::regtype, -- null when wal2json version <= 2.4
                            col.type_name::regtype
                        ),
                        -- cast jsonb to text
                        val_1:=col.value #>> '{}',
                        val_2:=f.value
                    ),
                    false -- if null, filter does not match
                )
            )
        from
            unnest(filters) f
            join unnest(columns) col
                on f.column_name = col.name;
    $_$;


--
-- Name: list_changes(name, name, integer, integer); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.list_changes(publication name, slot_name name, max_changes integer, max_record_bytes integer) RETURNS TABLE(wal jsonb, is_rls_enabled boolean, subscription_ids uuid[], errors text[], slot_changes_count bigint)
    LANGUAGE sql
    SET log_min_messages TO 'fatal'
    AS $$
  WITH pub AS (
    SELECT
      concat_ws(
        ',',
        CASE WHEN bool_or(pubinsert) THEN 'insert' ELSE NULL END,
        CASE WHEN bool_or(pubupdate) THEN 'update' ELSE NULL END,
        CASE WHEN bool_or(pubdelete) THEN 'delete' ELSE NULL END
      ) AS w2j_actions,
      coalesce(
        string_agg(
          realtime.quote_wal2json(format('%I.%I', schemaname, tablename)::regclass),
          ','
        ) filter (WHERE ppt.tablename IS NOT NULL AND ppt.tablename NOT LIKE '% %'),
        ''
      ) AS w2j_add_tables
    FROM pg_publication pp
    LEFT JOIN pg_publication_tables ppt ON pp.pubname = ppt.pubname
    WHERE pp.pubname = publication
    GROUP BY pp.pubname
    LIMIT 1
  ),
  -- MATERIALIZED ensures pg_logical_slot_get_changes is called exactly once
  w2j AS MATERIALIZED (
    SELECT x.*, pub.w2j_add_tables
    FROM pub,
         pg_logical_slot_get_changes(
           slot_name, null, max_changes,
           'include-pk', 'true',
           'include-transaction', 'false',
           'include-timestamp', 'true',
           'include-type-oids', 'true',
           'format-version', '2',
           'actions', pub.w2j_actions,
           'add-tables', pub.w2j_add_tables
         ) x
  ),
  -- Count raw slot entries before apply_rls/subscription filter
  slot_count AS (
    SELECT count(*)::bigint AS cnt
    FROM w2j
    WHERE w2j.w2j_add_tables <> ''
  ),
  -- Apply RLS and filter as before
  rls_filtered AS (
    SELECT xyz.wal, xyz.is_rls_enabled, xyz.subscription_ids, xyz.errors
    FROM w2j,
         realtime.apply_rls(
           wal := w2j.data::jsonb,
           max_record_bytes := max_record_bytes
         ) xyz(wal, is_rls_enabled, subscription_ids, errors)
    WHERE w2j.w2j_add_tables <> ''
      AND xyz.subscription_ids[1] IS NOT NULL
  )
  -- Real rows with slot count attached
  SELECT rf.wal, rf.is_rls_enabled, rf.subscription_ids, rf.errors, sc.cnt
  FROM rls_filtered rf, slot_count sc

  UNION ALL

  -- Sentinel row: always returned when no real rows exist so Elixir can
  -- always read slot_changes_count. Identified by wal IS NULL.
  SELECT null, null, null, null, sc.cnt
  FROM slot_count sc
  WHERE NOT EXISTS (SELECT 1 FROM rls_filtered)
$$;


--
-- Name: quote_wal2json(regclass); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.quote_wal2json(entity regclass) RETURNS text
    LANGUAGE sql IMMUTABLE STRICT
    AS $$
      select
        (
          select string_agg('' || ch,'')
          from unnest(string_to_array(nsp.nspname::text, null)) with ordinality x(ch, idx)
          where
            not (x.idx = 1 and x.ch = '"')
            and not (
              x.idx = array_length(string_to_array(nsp.nspname::text, null), 1)
              and x.ch = '"'
            )
        )
        || '.'
        || (
          select string_agg('' || ch,'')
          from unnest(string_to_array(pc.relname::text, null)) with ordinality x(ch, idx)
          where
            not (x.idx = 1 and x.ch = '"')
            and not (
              x.idx = array_length(string_to_array(nsp.nspname::text, null), 1)
              and x.ch = '"'
            )
          )
      from
        pg_class pc
        join pg_namespace nsp
          on pc.relnamespace = nsp.oid
      where
        pc.oid = entity
    $$;


--
-- Name: send(jsonb, text, text, boolean); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.send(payload jsonb, event text, topic text, private boolean DEFAULT true) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
  generated_id uuid;
  final_payload jsonb;
BEGIN
  BEGIN
    -- Generate a new UUID for the id
    generated_id := gen_random_uuid();

    -- Check if payload has an 'id' key, if not, add the generated UUID
    IF payload ? 'id' THEN
      final_payload := payload;
    ELSE
      final_payload := jsonb_set(payload, '{id}', to_jsonb(generated_id));
    END IF;

    -- Set the topic configuration
    EXECUTE format('SET LOCAL realtime.topic TO %L', topic);

    -- Attempt to insert the message
    INSERT INTO realtime.messages (id, payload, event, topic, private, extension)
    VALUES (generated_id, final_payload, event, topic, private, 'broadcast');
  EXCEPTION
    WHEN OTHERS THEN
      -- Capture and notify the error
      RAISE WARNING 'ErrorSendingBroadcastMessage: %', SQLERRM;
  END;
END;
$$;


--
-- Name: subscription_check_filters(); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.subscription_check_filters() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    /*
    Validates that the user defined filters for a subscription:
    - refer to valid columns that the claimed role may access
    - values are coercable to the correct column type
    */
    declare
        col_names text[] = coalesce(
                array_agg(c.column_name order by c.ordinal_position),
                '{}'::text[]
            )
            from
                information_schema.columns c
            where
                format('%I.%I', c.table_schema, c.table_name)::regclass = new.entity
                and pg_catalog.has_column_privilege(
                    (new.claims ->> 'role'),
                    format('%I.%I', c.table_schema, c.table_name)::regclass,
                    c.column_name,
                    'SELECT'
                );
        filter realtime.user_defined_filter;
        col_type regtype;

        in_val jsonb;
    begin
        for filter in select * from unnest(new.filters) loop
            -- Filtered column is valid
            if not filter.column_name = any(col_names) then
                raise exception 'invalid column for filter %', filter.column_name;
            end if;

            -- Type is sanitized and safe for string interpolation
            col_type = (
                select atttypid::regtype
                from pg_catalog.pg_attribute
                where attrelid = new.entity
                      and attname = filter.column_name
            );
            if col_type is null then
                raise exception 'failed to lookup type for column %', filter.column_name;
            end if;

            -- Set maximum number of entries for in filter
            if filter.op = 'in'::realtime.equality_op then
                in_val = realtime.cast(filter.value, (col_type::text || '[]')::regtype);
                if coalesce(jsonb_array_length(in_val), 0) > 100 then
                    raise exception 'too many values for `in` filter. Maximum 100';
                end if;
            else
                -- raises an exception if value is not coercable to type
                perform realtime.cast(filter.value, col_type);
            end if;

        end loop;

        -- Apply consistent order to filters so the unique constraint on
        -- (subscription_id, entity, filters) can't be tricked by a different filter order
        new.filters = coalesce(
            array_agg(f order by f.column_name, f.op, f.value),
            '{}'
        ) from unnest(new.filters) f;

        return new;
    end;
    $$;


--
-- Name: to_regrole(text); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.to_regrole(role_name text) RETURNS regrole
    LANGUAGE sql IMMUTABLE
    AS $$ select role_name::regrole $$;


--
-- Name: topic(); Type: FUNCTION; Schema: realtime; Owner: -
--

CREATE FUNCTION realtime.topic() RETURNS text
    LANGUAGE sql STABLE
    AS $$
select nullif(current_setting('realtime.topic', true), '')::text;
$$;


--
-- Name: allow_any_operation(text[]); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.allow_any_operation(expected_operations text[]) RETURNS boolean
    LANGUAGE sql STABLE
    AS $$
  WITH current_operation AS (
    SELECT storage.operation() AS raw_operation
  ),
  normalized AS (
    SELECT CASE
      WHEN raw_operation LIKE 'storage.%' THEN substr(raw_operation, 9)
      ELSE raw_operation
    END AS current_operation
    FROM current_operation
  )
  SELECT EXISTS (
    SELECT 1
    FROM normalized n
    CROSS JOIN LATERAL unnest(expected_operations) AS expected_operation
    WHERE expected_operation IS NOT NULL
      AND expected_operation <> ''
      AND n.current_operation = CASE
        WHEN expected_operation LIKE 'storage.%' THEN substr(expected_operation, 9)
        ELSE expected_operation
      END
  );
$$;


--
-- Name: allow_only_operation(text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.allow_only_operation(expected_operation text) RETURNS boolean
    LANGUAGE sql STABLE
    AS $$
  WITH current_operation AS (
    SELECT storage.operation() AS raw_operation
  ),
  normalized AS (
    SELECT
      CASE
        WHEN raw_operation LIKE 'storage.%' THEN substr(raw_operation, 9)
        ELSE raw_operation
      END AS current_operation,
      CASE
        WHEN expected_operation LIKE 'storage.%' THEN substr(expected_operation, 9)
        ELSE expected_operation
      END AS requested_operation
    FROM current_operation
  )
  SELECT CASE
    WHEN requested_operation IS NULL OR requested_operation = '' THEN FALSE
    ELSE COALESCE(current_operation = requested_operation, FALSE)
  END
  FROM normalized;
$$;


--
-- Name: can_insert_object(text, text, uuid, jsonb); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.can_insert_object(bucketid text, name text, owner uuid, metadata jsonb) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
  INSERT INTO "storage"."objects" ("bucket_id", "name", "owner", "metadata") VALUES (bucketid, name, owner, metadata);
  -- hack to rollback the successful insert
  RAISE sqlstate 'PT200' using
  message = 'ROLLBACK',
  detail = 'rollback successful insert';
END
$$;


--
-- Name: enforce_bucket_name_length(); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.enforce_bucket_name_length() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    if length(new.name) > 100 then
        raise exception 'bucket name "%" is too long (% characters). Max is 100.', new.name, length(new.name);
    end if;
    return new;
end;
$$;


--
-- Name: extension(text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.extension(name text) RETURNS text
    LANGUAGE plpgsql IMMUTABLE
    AS $$
DECLARE
    _parts text[];
    _filename text;
BEGIN
    -- Split on "/" to get path segments
    SELECT string_to_array(name, '/') INTO _parts;
    -- Get the last path segment (the actual filename)
    SELECT _parts[array_length(_parts, 1)] INTO _filename;
    -- Extract extension: reverse, split on '.', then reverse again
    RETURN reverse(split_part(reverse(_filename), '.', 1));
END
$$;


--
-- Name: filename(text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.filename(name text) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
_parts text[];
BEGIN
	select string_to_array(name, '/') into _parts;
	return _parts[array_length(_parts,1)];
END
$$;


--
-- Name: foldername(text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.foldername(name text) RETURNS text[]
    LANGUAGE plpgsql IMMUTABLE
    AS $$
DECLARE
    _parts text[];
BEGIN
    -- Split on "/" to get path segments
    SELECT string_to_array(name, '/') INTO _parts;
    -- Return everything except the last segment
    RETURN _parts[1 : array_length(_parts,1) - 1];
END
$$;


--
-- Name: get_common_prefix(text, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.get_common_prefix(p_key text, p_prefix text, p_delimiter text) RETURNS text
    LANGUAGE sql IMMUTABLE
    AS $$
SELECT CASE
    WHEN position(p_delimiter IN substring(p_key FROM length(p_prefix) + 1)) > 0
    THEN left(p_key, length(p_prefix) + position(p_delimiter IN substring(p_key FROM length(p_prefix) + 1)))
    ELSE NULL
END;
$$;


--
-- Name: get_size_by_bucket(); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.get_size_by_bucket() RETURNS TABLE(size bigint, bucket_id text)
    LANGUAGE plpgsql STABLE
    AS $$
BEGIN
    return query
        select sum((metadata->>'size')::bigint)::bigint as size, obj.bucket_id
        from "storage".objects as obj
        group by obj.bucket_id;
END
$$;


--
-- Name: list_multipart_uploads_with_delimiter(text, text, text, integer, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.list_multipart_uploads_with_delimiter(bucket_id text, prefix_param text, delimiter_param text, max_keys integer DEFAULT 100, next_key_token text DEFAULT ''::text, next_upload_token text DEFAULT ''::text) RETURNS TABLE(key text, id text, created_at timestamp with time zone)
    LANGUAGE plpgsql
    AS $_$
BEGIN
    RETURN QUERY EXECUTE
        'SELECT DISTINCT ON(key COLLATE "C") * from (
            SELECT
                CASE
                    WHEN position($2 IN substring(key from length($1) + 1)) > 0 THEN
                        substring(key from 1 for length($1) + position($2 IN substring(key from length($1) + 1)))
                    ELSE
                        key
                END AS key, id, created_at
            FROM
                storage.s3_multipart_uploads
            WHERE
                bucket_id = $5 AND
                key ILIKE $1 || ''%'' AND
                CASE
                    WHEN $4 != '''' AND $6 = '''' THEN
                        CASE
                            WHEN position($2 IN substring(key from length($1) + 1)) > 0 THEN
                                substring(key from 1 for length($1) + position($2 IN substring(key from length($1) + 1))) COLLATE "C" > $4
                            ELSE
                                key COLLATE "C" > $4
                            END
                    ELSE
                        true
                END AND
                CASE
                    WHEN $6 != '''' THEN
                        id COLLATE "C" > $6
                    ELSE
                        true
                    END
            ORDER BY
                key COLLATE "C" ASC, created_at ASC) as e order by key COLLATE "C" LIMIT $3'
        USING prefix_param, delimiter_param, max_keys, next_key_token, bucket_id, next_upload_token;
END;
$_$;


--
-- Name: list_objects_with_delimiter(text, text, text, integer, text, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.list_objects_with_delimiter(_bucket_id text, prefix_param text, delimiter_param text, max_keys integer DEFAULT 100, start_after text DEFAULT ''::text, next_token text DEFAULT ''::text, sort_order text DEFAULT 'asc'::text) RETURNS TABLE(name text, id uuid, metadata jsonb, updated_at timestamp with time zone, created_at timestamp with time zone, last_accessed_at timestamp with time zone)
    LANGUAGE plpgsql STABLE
    AS $_$
DECLARE
    v_peek_name TEXT;
    v_current RECORD;
    v_common_prefix TEXT;

    -- Configuration
    v_is_asc BOOLEAN;
    v_prefix TEXT;
    v_start TEXT;
    v_upper_bound TEXT;
    v_file_batch_size INT;

    -- Seek state
    v_next_seek TEXT;
    v_count INT := 0;

    -- Dynamic SQL for batch query only
    v_batch_query TEXT;

BEGIN
    -- ========================================================================
    -- INITIALIZATION
    -- ========================================================================
    v_is_asc := lower(coalesce(sort_order, 'asc')) = 'asc';
    v_prefix := coalesce(prefix_param, '');
    v_start := CASE WHEN coalesce(next_token, '') <> '' THEN next_token ELSE coalesce(start_after, '') END;
    v_file_batch_size := LEAST(GREATEST(max_keys * 2, 100), 1000);

    -- Calculate upper bound for prefix filtering (bytewise, using COLLATE "C")
    IF v_prefix = '' THEN
        v_upper_bound := NULL;
    ELSIF right(v_prefix, 1) = delimiter_param THEN
        v_upper_bound := left(v_prefix, -1) || chr(ascii(delimiter_param) + 1);
    ELSE
        v_upper_bound := left(v_prefix, -1) || chr(ascii(right(v_prefix, 1)) + 1);
    END IF;

    -- Build batch query (dynamic SQL - called infrequently, amortized over many rows)
    IF v_is_asc THEN
        IF v_upper_bound IS NOT NULL THEN
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND o.name COLLATE "C" >= $2 ' ||
                'AND o.name COLLATE "C" < $3 ORDER BY o.name COLLATE "C" ASC LIMIT $4';
        ELSE
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND o.name COLLATE "C" >= $2 ' ||
                'ORDER BY o.name COLLATE "C" ASC LIMIT $4';
        END IF;
    ELSE
        IF v_upper_bound IS NOT NULL THEN
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND o.name COLLATE "C" < $2 ' ||
                'AND o.name COLLATE "C" >= $3 ORDER BY o.name COLLATE "C" DESC LIMIT $4';
        ELSE
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND o.name COLLATE "C" < $2 ' ||
                'ORDER BY o.name COLLATE "C" DESC LIMIT $4';
        END IF;
    END IF;

    -- ========================================================================
    -- SEEK INITIALIZATION: Determine starting position
    -- ========================================================================
    IF v_start = '' THEN
        IF v_is_asc THEN
            v_next_seek := v_prefix;
        ELSE
            -- DESC without cursor: find the last item in range
            IF v_upper_bound IS NOT NULL THEN
                SELECT o.name INTO v_next_seek FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" >= v_prefix AND o.name COLLATE "C" < v_upper_bound
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            ELSIF v_prefix <> '' THEN
                SELECT o.name INTO v_next_seek FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" >= v_prefix
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            ELSE
                SELECT o.name INTO v_next_seek FROM storage.objects o
                WHERE o.bucket_id = _bucket_id
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            END IF;

            IF v_next_seek IS NOT NULL THEN
                v_next_seek := v_next_seek || delimiter_param;
            ELSE
                RETURN;
            END IF;
        END IF;
    ELSE
        -- Cursor provided: determine if it refers to a folder or leaf
        IF EXISTS (
            SELECT 1 FROM storage.objects o
            WHERE o.bucket_id = _bucket_id
              AND o.name COLLATE "C" LIKE v_start || delimiter_param || '%'
            LIMIT 1
        ) THEN
            -- Cursor refers to a folder
            IF v_is_asc THEN
                v_next_seek := v_start || chr(ascii(delimiter_param) + 1);
            ELSE
                v_next_seek := v_start || delimiter_param;
            END IF;
        ELSE
            -- Cursor refers to a leaf object
            IF v_is_asc THEN
                v_next_seek := v_start || delimiter_param;
            ELSE
                v_next_seek := v_start;
            END IF;
        END IF;
    END IF;

    -- ========================================================================
    -- MAIN LOOP: Hybrid peek-then-batch algorithm
    -- Uses STATIC SQL for peek (hot path) and DYNAMIC SQL for batch
    -- ========================================================================
    LOOP
        EXIT WHEN v_count >= max_keys;

        -- STEP 1: PEEK using STATIC SQL (plan cached, very fast)
        IF v_is_asc THEN
            IF v_upper_bound IS NOT NULL THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" >= v_next_seek AND o.name COLLATE "C" < v_upper_bound
                ORDER BY o.name COLLATE "C" ASC LIMIT 1;
            ELSE
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" >= v_next_seek
                ORDER BY o.name COLLATE "C" ASC LIMIT 1;
            END IF;
        ELSE
            IF v_upper_bound IS NOT NULL THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" < v_next_seek AND o.name COLLATE "C" >= v_prefix
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            ELSIF v_prefix <> '' THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" < v_next_seek AND o.name COLLATE "C" >= v_prefix
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            ELSE
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = _bucket_id AND o.name COLLATE "C" < v_next_seek
                ORDER BY o.name COLLATE "C" DESC LIMIT 1;
            END IF;
        END IF;

        EXIT WHEN v_peek_name IS NULL;

        -- STEP 2: Check if this is a FOLDER or FILE
        v_common_prefix := storage.get_common_prefix(v_peek_name, v_prefix, delimiter_param);

        IF v_common_prefix IS NOT NULL THEN
            -- FOLDER: Emit and skip to next folder (no heap access needed)
            name := rtrim(v_common_prefix, delimiter_param);
            id := NULL;
            updated_at := NULL;
            created_at := NULL;
            last_accessed_at := NULL;
            metadata := NULL;
            RETURN NEXT;
            v_count := v_count + 1;

            -- Advance seek past the folder range
            IF v_is_asc THEN
                v_next_seek := left(v_common_prefix, -1) || chr(ascii(delimiter_param) + 1);
            ELSE
                v_next_seek := v_common_prefix;
            END IF;
        ELSE
            -- FILE: Batch fetch using DYNAMIC SQL (overhead amortized over many rows)
            -- For ASC: upper_bound is the exclusive upper limit (< condition)
            -- For DESC: prefix is the inclusive lower limit (>= condition)
            FOR v_current IN EXECUTE v_batch_query USING _bucket_id, v_next_seek,
                CASE WHEN v_is_asc THEN COALESCE(v_upper_bound, v_prefix) ELSE v_prefix END, v_file_batch_size
            LOOP
                v_common_prefix := storage.get_common_prefix(v_current.name, v_prefix, delimiter_param);

                IF v_common_prefix IS NOT NULL THEN
                    -- Hit a folder: exit batch, let peek handle it
                    v_next_seek := v_current.name;
                    EXIT;
                END IF;

                -- Emit file
                name := v_current.name;
                id := v_current.id;
                updated_at := v_current.updated_at;
                created_at := v_current.created_at;
                last_accessed_at := v_current.last_accessed_at;
                metadata := v_current.metadata;
                RETURN NEXT;
                v_count := v_count + 1;

                -- Advance seek past this file
                IF v_is_asc THEN
                    v_next_seek := v_current.name || delimiter_param;
                ELSE
                    v_next_seek := v_current.name;
                END IF;

                EXIT WHEN v_count >= max_keys;
            END LOOP;
        END IF;
    END LOOP;
END;
$_$;


--
-- Name: operation(); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.operation() RETURNS text
    LANGUAGE plpgsql STABLE
    AS $$
BEGIN
    RETURN current_setting('storage.operation', true);
END;
$$;


--
-- Name: protect_delete(); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.protect_delete() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- Check if storage.allow_delete_query is set to 'true'
    IF COALESCE(current_setting('storage.allow_delete_query', true), 'false') != 'true' THEN
        RAISE EXCEPTION 'Direct deletion from storage tables is not allowed. Use the Storage API instead.'
            USING HINT = 'This prevents accidental data loss from orphaned objects.',
                  ERRCODE = '42501';
    END IF;
    RETURN NULL;
END;
$$;


--
-- Name: search(text, text, integer, integer, integer, text, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.search(prefix text, bucketname text, limits integer DEFAULT 100, levels integer DEFAULT 1, offsets integer DEFAULT 0, search text DEFAULT ''::text, sortcolumn text DEFAULT 'name'::text, sortorder text DEFAULT 'asc'::text) RETURNS TABLE(name text, id uuid, updated_at timestamp with time zone, created_at timestamp with time zone, last_accessed_at timestamp with time zone, metadata jsonb)
    LANGUAGE plpgsql STABLE
    AS $_$
DECLARE
    v_peek_name TEXT;
    v_current RECORD;
    v_common_prefix TEXT;
    v_delimiter CONSTANT TEXT := '/';

    -- Configuration
    v_limit INT;
    v_prefix TEXT;
    v_prefix_lower TEXT;
    v_is_asc BOOLEAN;
    v_order_by TEXT;
    v_sort_order TEXT;
    v_upper_bound TEXT;
    v_file_batch_size INT;

    -- Dynamic SQL for batch query only
    v_batch_query TEXT;

    -- Seek state
    v_next_seek TEXT;
    v_count INT := 0;
    v_skipped INT := 0;
BEGIN
    -- ========================================================================
    -- INITIALIZATION
    -- ========================================================================
    v_limit := LEAST(coalesce(limits, 100), 1500);
    v_prefix := coalesce(prefix, '') || coalesce(search, '');
    v_prefix_lower := lower(v_prefix);
    v_is_asc := lower(coalesce(sortorder, 'asc')) = 'asc';
    v_file_batch_size := LEAST(GREATEST(v_limit * 2, 100), 1000);

    -- Validate sort column
    CASE lower(coalesce(sortcolumn, 'name'))
        WHEN 'name' THEN v_order_by := 'name';
        WHEN 'updated_at' THEN v_order_by := 'updated_at';
        WHEN 'created_at' THEN v_order_by := 'created_at';
        WHEN 'last_accessed_at' THEN v_order_by := 'last_accessed_at';
        ELSE v_order_by := 'name';
    END CASE;

    v_sort_order := CASE WHEN v_is_asc THEN 'asc' ELSE 'desc' END;

    -- ========================================================================
    -- NON-NAME SORTING: Use path_tokens approach (unchanged)
    -- ========================================================================
    IF v_order_by != 'name' THEN
        RETURN QUERY EXECUTE format(
            $sql$
            WITH folders AS (
                SELECT path_tokens[$1] AS folder
                FROM storage.objects
                WHERE objects.name ILIKE $2 || '%%'
                  AND bucket_id = $3
                  AND array_length(objects.path_tokens, 1) <> $1
                GROUP BY folder
                ORDER BY folder %s
            )
            (SELECT folder AS "name",
                   NULL::uuid AS id,
                   NULL::timestamptz AS updated_at,
                   NULL::timestamptz AS created_at,
                   NULL::timestamptz AS last_accessed_at,
                   NULL::jsonb AS metadata FROM folders)
            UNION ALL
            (SELECT path_tokens[$1] AS "name",
                   id, updated_at, created_at, last_accessed_at, metadata
             FROM storage.objects
             WHERE objects.name ILIKE $2 || '%%'
               AND bucket_id = $3
               AND array_length(objects.path_tokens, 1) = $1
             ORDER BY %I %s)
            LIMIT $4 OFFSET $5
            $sql$, v_sort_order, v_order_by, v_sort_order
        ) USING levels, v_prefix, bucketname, v_limit, offsets;
        RETURN;
    END IF;

    -- ========================================================================
    -- NAME SORTING: Hybrid skip-scan with batch optimization
    -- ========================================================================

    -- Calculate upper bound for prefix filtering
    IF v_prefix_lower = '' THEN
        v_upper_bound := NULL;
    ELSIF right(v_prefix_lower, 1) = v_delimiter THEN
        v_upper_bound := left(v_prefix_lower, -1) || chr(ascii(v_delimiter) + 1);
    ELSE
        v_upper_bound := left(v_prefix_lower, -1) || chr(ascii(right(v_prefix_lower, 1)) + 1);
    END IF;

    -- Build batch query (dynamic SQL - called infrequently, amortized over many rows)
    IF v_is_asc THEN
        IF v_upper_bound IS NOT NULL THEN
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND lower(o.name) COLLATE "C" >= $2 ' ||
                'AND lower(o.name) COLLATE "C" < $3 ORDER BY lower(o.name) COLLATE "C" ASC LIMIT $4';
        ELSE
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND lower(o.name) COLLATE "C" >= $2 ' ||
                'ORDER BY lower(o.name) COLLATE "C" ASC LIMIT $4';
        END IF;
    ELSE
        IF v_upper_bound IS NOT NULL THEN
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND lower(o.name) COLLATE "C" < $2 ' ||
                'AND lower(o.name) COLLATE "C" >= $3 ORDER BY lower(o.name) COLLATE "C" DESC LIMIT $4';
        ELSE
            v_batch_query := 'SELECT o.name, o.id, o.updated_at, o.created_at, o.last_accessed_at, o.metadata ' ||
                'FROM storage.objects o WHERE o.bucket_id = $1 AND lower(o.name) COLLATE "C" < $2 ' ||
                'ORDER BY lower(o.name) COLLATE "C" DESC LIMIT $4';
        END IF;
    END IF;

    -- Initialize seek position
    IF v_is_asc THEN
        v_next_seek := v_prefix_lower;
    ELSE
        -- DESC: find the last item in range first (static SQL)
        IF v_upper_bound IS NOT NULL THEN
            SELECT o.name INTO v_peek_name FROM storage.objects o
            WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" >= v_prefix_lower AND lower(o.name) COLLATE "C" < v_upper_bound
            ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
        ELSIF v_prefix_lower <> '' THEN
            SELECT o.name INTO v_peek_name FROM storage.objects o
            WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" >= v_prefix_lower
            ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
        ELSE
            SELECT o.name INTO v_peek_name FROM storage.objects o
            WHERE o.bucket_id = bucketname
            ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
        END IF;

        IF v_peek_name IS NOT NULL THEN
            v_next_seek := lower(v_peek_name) || v_delimiter;
        ELSE
            RETURN;
        END IF;
    END IF;

    -- ========================================================================
    -- MAIN LOOP: Hybrid peek-then-batch algorithm
    -- Uses STATIC SQL for peek (hot path) and DYNAMIC SQL for batch
    -- ========================================================================
    LOOP
        EXIT WHEN v_count >= v_limit;

        -- STEP 1: PEEK using STATIC SQL (plan cached, very fast)
        IF v_is_asc THEN
            IF v_upper_bound IS NOT NULL THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" >= v_next_seek AND lower(o.name) COLLATE "C" < v_upper_bound
                ORDER BY lower(o.name) COLLATE "C" ASC LIMIT 1;
            ELSE
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" >= v_next_seek
                ORDER BY lower(o.name) COLLATE "C" ASC LIMIT 1;
            END IF;
        ELSE
            IF v_upper_bound IS NOT NULL THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" < v_next_seek AND lower(o.name) COLLATE "C" >= v_prefix_lower
                ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
            ELSIF v_prefix_lower <> '' THEN
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" < v_next_seek AND lower(o.name) COLLATE "C" >= v_prefix_lower
                ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
            ELSE
                SELECT o.name INTO v_peek_name FROM storage.objects o
                WHERE o.bucket_id = bucketname AND lower(o.name) COLLATE "C" < v_next_seek
                ORDER BY lower(o.name) COLLATE "C" DESC LIMIT 1;
            END IF;
        END IF;

        EXIT WHEN v_peek_name IS NULL;

        -- STEP 2: Check if this is a FOLDER or FILE
        v_common_prefix := storage.get_common_prefix(lower(v_peek_name), v_prefix_lower, v_delimiter);

        IF v_common_prefix IS NOT NULL THEN
            -- FOLDER: Handle offset, emit if needed, skip to next folder
            IF v_skipped < offsets THEN
                v_skipped := v_skipped + 1;
            ELSE
                name := split_part(rtrim(storage.get_common_prefix(v_peek_name, v_prefix, v_delimiter), v_delimiter), v_delimiter, levels);
                id := NULL;
                updated_at := NULL;
                created_at := NULL;
                last_accessed_at := NULL;
                metadata := NULL;
                RETURN NEXT;
                v_count := v_count + 1;
            END IF;

            -- Advance seek past the folder range
            IF v_is_asc THEN
                v_next_seek := lower(left(v_common_prefix, -1)) || chr(ascii(v_delimiter) + 1);
            ELSE
                v_next_seek := lower(v_common_prefix);
            END IF;
        ELSE
            -- FILE: Batch fetch using DYNAMIC SQL (overhead amortized over many rows)
            -- For ASC: upper_bound is the exclusive upper limit (< condition)
            -- For DESC: prefix_lower is the inclusive lower limit (>= condition)
            FOR v_current IN EXECUTE v_batch_query
                USING bucketname, v_next_seek,
                    CASE WHEN v_is_asc THEN COALESCE(v_upper_bound, v_prefix_lower) ELSE v_prefix_lower END, v_file_batch_size
            LOOP
                v_common_prefix := storage.get_common_prefix(lower(v_current.name), v_prefix_lower, v_delimiter);

                IF v_common_prefix IS NOT NULL THEN
                    -- Hit a folder: exit batch, let peek handle it
                    v_next_seek := lower(v_current.name);
                    EXIT;
                END IF;

                -- Handle offset skipping
                IF v_skipped < offsets THEN
                    v_skipped := v_skipped + 1;
                ELSE
                    -- Emit file
                    name := split_part(v_current.name, v_delimiter, levels);
                    id := v_current.id;
                    updated_at := v_current.updated_at;
                    created_at := v_current.created_at;
                    last_accessed_at := v_current.last_accessed_at;
                    metadata := v_current.metadata;
                    RETURN NEXT;
                    v_count := v_count + 1;
                END IF;

                -- Advance seek past this file
                IF v_is_asc THEN
                    v_next_seek := lower(v_current.name) || v_delimiter;
                ELSE
                    v_next_seek := lower(v_current.name);
                END IF;

                EXIT WHEN v_count >= v_limit;
            END LOOP;
        END IF;
    END LOOP;
END;
$_$;


--
-- Name: search_by_timestamp(text, text, integer, integer, text, text, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.search_by_timestamp(p_prefix text, p_bucket_id text, p_limit integer, p_level integer, p_start_after text, p_sort_order text, p_sort_column text, p_sort_column_after text) RETURNS TABLE(key text, name text, id uuid, updated_at timestamp with time zone, created_at timestamp with time zone, last_accessed_at timestamp with time zone, metadata jsonb)
    LANGUAGE plpgsql STABLE
    AS $_$
DECLARE
    v_cursor_op text;
    v_query text;
    v_prefix text;
BEGIN
    v_prefix := coalesce(p_prefix, '');

    IF p_sort_order = 'asc' THEN
        v_cursor_op := '>';
    ELSE
        v_cursor_op := '<';
    END IF;

    v_query := format($sql$
        WITH raw_objects AS (
            SELECT
                o.name AS obj_name,
                o.id AS obj_id,
                o.updated_at AS obj_updated_at,
                o.created_at AS obj_created_at,
                o.last_accessed_at AS obj_last_accessed_at,
                o.metadata AS obj_metadata,
                storage.get_common_prefix(o.name, $1, '/') AS common_prefix
            FROM storage.objects o
            WHERE o.bucket_id = $2
              AND o.name COLLATE "C" LIKE $1 || '%%'
        ),
        -- Aggregate common prefixes (folders)
        -- Both created_at and updated_at use MIN(obj_created_at) to match the old prefixes table behavior
        aggregated_prefixes AS (
            SELECT
                rtrim(common_prefix, '/') AS name,
                NULL::uuid AS id,
                MIN(obj_created_at) AS updated_at,
                MIN(obj_created_at) AS created_at,
                NULL::timestamptz AS last_accessed_at,
                NULL::jsonb AS metadata,
                TRUE AS is_prefix
            FROM raw_objects
            WHERE common_prefix IS NOT NULL
            GROUP BY common_prefix
        ),
        leaf_objects AS (
            SELECT
                obj_name AS name,
                obj_id AS id,
                obj_updated_at AS updated_at,
                obj_created_at AS created_at,
                obj_last_accessed_at AS last_accessed_at,
                obj_metadata AS metadata,
                FALSE AS is_prefix
            FROM raw_objects
            WHERE common_prefix IS NULL
        ),
        combined AS (
            SELECT * FROM aggregated_prefixes
            UNION ALL
            SELECT * FROM leaf_objects
        ),
        filtered AS (
            SELECT *
            FROM combined
            WHERE (
                $5 = ''
                OR ROW(
                    date_trunc('milliseconds', %I),
                    name COLLATE "C"
                ) %s ROW(
                    COALESCE(NULLIF($6, '')::timestamptz, 'epoch'::timestamptz),
                    $5
                )
            )
        )
        SELECT
            split_part(name, '/', $3) AS key,
            name,
            id,
            updated_at,
            created_at,
            last_accessed_at,
            metadata
        FROM filtered
        ORDER BY
            COALESCE(date_trunc('milliseconds', %I), 'epoch'::timestamptz) %s,
            name COLLATE "C" %s
        LIMIT $4
    $sql$,
        p_sort_column,
        v_cursor_op,
        p_sort_column,
        p_sort_order,
        p_sort_order
    );

    RETURN QUERY EXECUTE v_query
    USING v_prefix, p_bucket_id, p_level, p_limit, p_start_after, p_sort_column_after;
END;
$_$;


--
-- Name: search_v2(text, text, integer, integer, text, text, text, text); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.search_v2(prefix text, bucket_name text, limits integer DEFAULT 100, levels integer DEFAULT 1, start_after text DEFAULT ''::text, sort_order text DEFAULT 'asc'::text, sort_column text DEFAULT 'name'::text, sort_column_after text DEFAULT ''::text) RETURNS TABLE(key text, name text, id uuid, updated_at timestamp with time zone, created_at timestamp with time zone, last_accessed_at timestamp with time zone, metadata jsonb)
    LANGUAGE plpgsql STABLE
    AS $$
DECLARE
    v_sort_col text;
    v_sort_ord text;
    v_limit int;
BEGIN
    -- Cap limit to maximum of 1500 records
    v_limit := LEAST(coalesce(limits, 100), 1500);

    -- Validate and normalize sort_order
    v_sort_ord := lower(coalesce(sort_order, 'asc'));
    IF v_sort_ord NOT IN ('asc', 'desc') THEN
        v_sort_ord := 'asc';
    END IF;

    -- Validate and normalize sort_column
    v_sort_col := lower(coalesce(sort_column, 'name'));
    IF v_sort_col NOT IN ('name', 'updated_at', 'created_at') THEN
        v_sort_col := 'name';
    END IF;

    -- Route to appropriate implementation
    IF v_sort_col = 'name' THEN
        -- Use list_objects_with_delimiter for name sorting (most efficient: O(k * log n))
        RETURN QUERY
        SELECT
            split_part(l.name, '/', levels) AS key,
            l.name AS name,
            l.id,
            l.updated_at,
            l.created_at,
            l.last_accessed_at,
            l.metadata
        FROM storage.list_objects_with_delimiter(
            bucket_name,
            coalesce(prefix, ''),
            '/',
            v_limit,
            start_after,
            '',
            v_sort_ord
        ) l;
    ELSE
        -- Use aggregation approach for timestamp sorting
        -- Not efficient for large datasets but supports correct pagination
        RETURN QUERY SELECT * FROM storage.search_by_timestamp(
            prefix, bucket_name, v_limit, levels, start_after,
            v_sort_ord, v_sort_col, sort_column_after
        );
    END IF;
END;
$$;


--
-- Name: update_updated_at_column(); Type: FUNCTION; Schema: storage; Owner: -
--

CREATE FUNCTION storage.update_updated_at_column() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW; 
END;
$$;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: audit_log_entries; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.audit_log_entries (
    instance_id uuid,
    id uuid NOT NULL,
    payload json,
    created_at timestamp with time zone,
    ip_address character varying(64) DEFAULT ''::character varying NOT NULL
);


--
-- Name: TABLE audit_log_entries; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.audit_log_entries IS 'Auth: Audit trail for user actions.';


--
-- Name: custom_oauth_providers; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.custom_oauth_providers (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    provider_type text NOT NULL,
    identifier text NOT NULL,
    name text NOT NULL,
    client_id text NOT NULL,
    client_secret text NOT NULL,
    acceptable_client_ids text[] DEFAULT '{}'::text[] NOT NULL,
    scopes text[] DEFAULT '{}'::text[] NOT NULL,
    pkce_enabled boolean DEFAULT true NOT NULL,
    attribute_mapping jsonb DEFAULT '{}'::jsonb NOT NULL,
    authorization_params jsonb DEFAULT '{}'::jsonb NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    email_optional boolean DEFAULT false NOT NULL,
    issuer text,
    discovery_url text,
    skip_nonce_check boolean DEFAULT false NOT NULL,
    cached_discovery jsonb,
    discovery_cached_at timestamp with time zone,
    authorization_url text,
    token_url text,
    userinfo_url text,
    jwks_uri text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT custom_oauth_providers_authorization_url_https CHECK (((authorization_url IS NULL) OR (authorization_url ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_authorization_url_length CHECK (((authorization_url IS NULL) OR (char_length(authorization_url) <= 2048))),
    CONSTRAINT custom_oauth_providers_client_id_length CHECK (((char_length(client_id) >= 1) AND (char_length(client_id) <= 512))),
    CONSTRAINT custom_oauth_providers_discovery_url_length CHECK (((discovery_url IS NULL) OR (char_length(discovery_url) <= 2048))),
    CONSTRAINT custom_oauth_providers_identifier_format CHECK ((identifier ~ '^[a-z0-9][a-z0-9:-]{0,48}[a-z0-9]$'::text)),
    CONSTRAINT custom_oauth_providers_issuer_length CHECK (((issuer IS NULL) OR ((char_length(issuer) >= 1) AND (char_length(issuer) <= 2048)))),
    CONSTRAINT custom_oauth_providers_jwks_uri_https CHECK (((jwks_uri IS NULL) OR (jwks_uri ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_jwks_uri_length CHECK (((jwks_uri IS NULL) OR (char_length(jwks_uri) <= 2048))),
    CONSTRAINT custom_oauth_providers_name_length CHECK (((char_length(name) >= 1) AND (char_length(name) <= 100))),
    CONSTRAINT custom_oauth_providers_oauth2_requires_endpoints CHECK (((provider_type <> 'oauth2'::text) OR ((authorization_url IS NOT NULL) AND (token_url IS NOT NULL) AND (userinfo_url IS NOT NULL)))),
    CONSTRAINT custom_oauth_providers_oidc_discovery_url_https CHECK (((provider_type <> 'oidc'::text) OR (discovery_url IS NULL) OR (discovery_url ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_oidc_issuer_https CHECK (((provider_type <> 'oidc'::text) OR (issuer IS NULL) OR (issuer ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_oidc_requires_issuer CHECK (((provider_type <> 'oidc'::text) OR (issuer IS NOT NULL))),
    CONSTRAINT custom_oauth_providers_provider_type_check CHECK ((provider_type = ANY (ARRAY['oauth2'::text, 'oidc'::text]))),
    CONSTRAINT custom_oauth_providers_token_url_https CHECK (((token_url IS NULL) OR (token_url ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_token_url_length CHECK (((token_url IS NULL) OR (char_length(token_url) <= 2048))),
    CONSTRAINT custom_oauth_providers_userinfo_url_https CHECK (((userinfo_url IS NULL) OR (userinfo_url ~~ 'https://%'::text))),
    CONSTRAINT custom_oauth_providers_userinfo_url_length CHECK (((userinfo_url IS NULL) OR (char_length(userinfo_url) <= 2048)))
);


--
-- Name: flow_state; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.flow_state (
    id uuid NOT NULL,
    user_id uuid,
    auth_code text,
    code_challenge_method auth.code_challenge_method,
    code_challenge text,
    provider_type text NOT NULL,
    provider_access_token text,
    provider_refresh_token text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    authentication_method text NOT NULL,
    auth_code_issued_at timestamp with time zone,
    invite_token text,
    referrer text,
    oauth_client_state_id uuid,
    linking_target_id uuid,
    email_optional boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE flow_state; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.flow_state IS 'Stores metadata for all OAuth/SSO login flows';


--
-- Name: identities; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.identities (
    provider_id text NOT NULL,
    user_id uuid NOT NULL,
    identity_data jsonb NOT NULL,
    provider text NOT NULL,
    last_sign_in_at timestamp with time zone,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    email text GENERATED ALWAYS AS (lower((identity_data ->> 'email'::text))) STORED,
    id uuid DEFAULT gen_random_uuid() NOT NULL
);


--
-- Name: TABLE identities; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.identities IS 'Auth: Stores identities associated to a user.';


--
-- Name: COLUMN identities.email; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.identities.email IS 'Auth: Email is a generated column that references the optional email property in the identity_data';


--
-- Name: instances; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.instances (
    id uuid NOT NULL,
    uuid uuid,
    raw_base_config text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone
);


--
-- Name: TABLE instances; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.instances IS 'Auth: Manages users across multiple sites.';


--
-- Name: mfa_amr_claims; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.mfa_amr_claims (
    session_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    authentication_method text NOT NULL,
    id uuid NOT NULL
);


--
-- Name: TABLE mfa_amr_claims; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.mfa_amr_claims IS 'auth: stores authenticator method reference claims for multi factor authentication';


--
-- Name: mfa_challenges; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.mfa_challenges (
    id uuid NOT NULL,
    factor_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    verified_at timestamp with time zone,
    ip_address inet NOT NULL,
    otp_code text,
    web_authn_session_data jsonb
);


--
-- Name: TABLE mfa_challenges; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.mfa_challenges IS 'auth: stores metadata about challenge requests made';


--
-- Name: mfa_factors; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.mfa_factors (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    friendly_name text,
    factor_type auth.factor_type NOT NULL,
    status auth.factor_status NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    secret text,
    phone text,
    last_challenged_at timestamp with time zone,
    web_authn_credential jsonb,
    web_authn_aaguid uuid,
    last_webauthn_challenge_data jsonb
);


--
-- Name: TABLE mfa_factors; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.mfa_factors IS 'auth: stores metadata about factors';


--
-- Name: COLUMN mfa_factors.last_webauthn_challenge_data; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.mfa_factors.last_webauthn_challenge_data IS 'Stores the latest WebAuthn challenge data including attestation/assertion for customer verification';


--
-- Name: oauth_authorizations; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.oauth_authorizations (
    id uuid NOT NULL,
    authorization_id text NOT NULL,
    client_id uuid NOT NULL,
    user_id uuid,
    redirect_uri text NOT NULL,
    scope text NOT NULL,
    state text,
    resource text,
    code_challenge text,
    code_challenge_method auth.code_challenge_method,
    response_type auth.oauth_response_type DEFAULT 'code'::auth.oauth_response_type NOT NULL,
    status auth.oauth_authorization_status DEFAULT 'pending'::auth.oauth_authorization_status NOT NULL,
    authorization_code text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    expires_at timestamp with time zone DEFAULT (now() + '00:03:00'::interval) NOT NULL,
    approved_at timestamp with time zone,
    nonce text,
    CONSTRAINT oauth_authorizations_authorization_code_length CHECK ((char_length(authorization_code) <= 255)),
    CONSTRAINT oauth_authorizations_code_challenge_length CHECK ((char_length(code_challenge) <= 128)),
    CONSTRAINT oauth_authorizations_expires_at_future CHECK ((expires_at > created_at)),
    CONSTRAINT oauth_authorizations_nonce_length CHECK ((char_length(nonce) <= 255)),
    CONSTRAINT oauth_authorizations_redirect_uri_length CHECK ((char_length(redirect_uri) <= 2048)),
    CONSTRAINT oauth_authorizations_resource_length CHECK ((char_length(resource) <= 2048)),
    CONSTRAINT oauth_authorizations_scope_length CHECK ((char_length(scope) <= 4096)),
    CONSTRAINT oauth_authorizations_state_length CHECK ((char_length(state) <= 4096))
);


--
-- Name: oauth_client_states; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.oauth_client_states (
    id uuid NOT NULL,
    provider_type text NOT NULL,
    code_verifier text,
    created_at timestamp with time zone NOT NULL
);


--
-- Name: TABLE oauth_client_states; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.oauth_client_states IS 'Stores OAuth states for third-party provider authentication flows where Supabase acts as the OAuth client.';


--
-- Name: oauth_clients; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.oauth_clients (
    id uuid NOT NULL,
    client_secret_hash text,
    registration_type auth.oauth_registration_type NOT NULL,
    redirect_uris text NOT NULL,
    grant_types text NOT NULL,
    client_name text,
    client_uri text,
    logo_uri text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone,
    client_type auth.oauth_client_type DEFAULT 'confidential'::auth.oauth_client_type NOT NULL,
    token_endpoint_auth_method text NOT NULL,
    CONSTRAINT oauth_clients_client_name_length CHECK ((char_length(client_name) <= 1024)),
    CONSTRAINT oauth_clients_client_uri_length CHECK ((char_length(client_uri) <= 2048)),
    CONSTRAINT oauth_clients_logo_uri_length CHECK ((char_length(logo_uri) <= 2048)),
    CONSTRAINT oauth_clients_token_endpoint_auth_method_check CHECK ((token_endpoint_auth_method = ANY (ARRAY['client_secret_basic'::text, 'client_secret_post'::text, 'none'::text])))
);


--
-- Name: oauth_consents; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.oauth_consents (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    client_id uuid NOT NULL,
    scopes text NOT NULL,
    granted_at timestamp with time zone DEFAULT now() NOT NULL,
    revoked_at timestamp with time zone,
    CONSTRAINT oauth_consents_revoked_after_granted CHECK (((revoked_at IS NULL) OR (revoked_at >= granted_at))),
    CONSTRAINT oauth_consents_scopes_length CHECK ((char_length(scopes) <= 2048)),
    CONSTRAINT oauth_consents_scopes_not_empty CHECK ((char_length(TRIM(BOTH FROM scopes)) > 0))
);


--
-- Name: one_time_tokens; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.one_time_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token_type auth.one_time_token_type NOT NULL,
    token_hash text NOT NULL,
    relates_to text NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    updated_at timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT one_time_tokens_token_hash_check CHECK ((char_length(token_hash) > 0))
);


--
-- Name: refresh_tokens; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.refresh_tokens (
    instance_id uuid,
    id bigint NOT NULL,
    token character varying(255),
    user_id character varying(255),
    revoked boolean,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    parent character varying(255),
    session_id uuid
);


--
-- Name: TABLE refresh_tokens; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.refresh_tokens IS 'Auth: Store of tokens used to refresh JWT tokens once they expire.';


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE; Schema: auth; Owner: -
--

CREATE SEQUENCE auth.refresh_tokens_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE OWNED BY; Schema: auth; Owner: -
--

ALTER SEQUENCE auth.refresh_tokens_id_seq OWNED BY auth.refresh_tokens.id;


--
-- Name: saml_providers; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.saml_providers (
    id uuid NOT NULL,
    sso_provider_id uuid NOT NULL,
    entity_id text NOT NULL,
    metadata_xml text NOT NULL,
    metadata_url text,
    attribute_mapping jsonb,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    name_id_format text,
    CONSTRAINT "entity_id not empty" CHECK ((char_length(entity_id) > 0)),
    CONSTRAINT "metadata_url not empty" CHECK (((metadata_url = NULL::text) OR (char_length(metadata_url) > 0))),
    CONSTRAINT "metadata_xml not empty" CHECK ((char_length(metadata_xml) > 0))
);


--
-- Name: TABLE saml_providers; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.saml_providers IS 'Auth: Manages SAML Identity Provider connections.';


--
-- Name: saml_relay_states; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.saml_relay_states (
    id uuid NOT NULL,
    sso_provider_id uuid NOT NULL,
    request_id text NOT NULL,
    for_email text,
    redirect_to text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    flow_state_id uuid,
    CONSTRAINT "request_id not empty" CHECK ((char_length(request_id) > 0))
);


--
-- Name: TABLE saml_relay_states; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.saml_relay_states IS 'Auth: Contains SAML Relay State information for each Service Provider initiated login.';


--
-- Name: schema_migrations; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.schema_migrations (
    version character varying(255) NOT NULL
);


--
-- Name: TABLE schema_migrations; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.schema_migrations IS 'Auth: Manages updates to the auth system.';


--
-- Name: sessions; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.sessions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    factor_id uuid,
    aal auth.aal_level,
    not_after timestamp with time zone,
    refreshed_at timestamp without time zone,
    user_agent text,
    ip inet,
    tag text,
    oauth_client_id uuid,
    refresh_token_hmac_key text,
    refresh_token_counter bigint,
    scopes text,
    CONSTRAINT sessions_scopes_length CHECK ((char_length(scopes) <= 4096))
);


--
-- Name: TABLE sessions; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.sessions IS 'Auth: Stores session data associated to a user.';


--
-- Name: COLUMN sessions.not_after; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.sessions.not_after IS 'Auth: Not after is a nullable column that contains a timestamp after which the session should be regarded as expired.';


--
-- Name: COLUMN sessions.refresh_token_hmac_key; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.sessions.refresh_token_hmac_key IS 'Holds a HMAC-SHA256 key used to sign refresh tokens for this session.';


--
-- Name: COLUMN sessions.refresh_token_counter; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.sessions.refresh_token_counter IS 'Holds the ID (counter) of the last issued refresh token.';


--
-- Name: sso_domains; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.sso_domains (
    id uuid NOT NULL,
    sso_provider_id uuid NOT NULL,
    domain text NOT NULL,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    CONSTRAINT "domain not empty" CHECK ((char_length(domain) > 0))
);


--
-- Name: TABLE sso_domains; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.sso_domains IS 'Auth: Manages SSO email address domain mapping to an SSO Identity Provider.';


--
-- Name: sso_providers; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.sso_providers (
    id uuid NOT NULL,
    resource_id text,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    disabled boolean,
    CONSTRAINT "resource_id not empty" CHECK (((resource_id = NULL::text) OR (char_length(resource_id) > 0)))
);


--
-- Name: TABLE sso_providers; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.sso_providers IS 'Auth: Manages SSO identity provider information; see saml_providers for SAML.';


--
-- Name: COLUMN sso_providers.resource_id; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.sso_providers.resource_id IS 'Auth: Uniquely identifies a SSO provider according to a user-chosen resource ID (case insensitive), useful in infrastructure as code.';


--
-- Name: users; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.users (
    instance_id uuid,
    id uuid NOT NULL,
    aud character varying(255),
    role character varying(255),
    email character varying(255),
    encrypted_password character varying(255),
    email_confirmed_at timestamp with time zone,
    invited_at timestamp with time zone,
    confirmation_token character varying(255),
    confirmation_sent_at timestamp with time zone,
    recovery_token character varying(255),
    recovery_sent_at timestamp with time zone,
    email_change_token_new character varying(255),
    email_change character varying(255),
    email_change_sent_at timestamp with time zone,
    last_sign_in_at timestamp with time zone,
    raw_app_meta_data jsonb,
    raw_user_meta_data jsonb,
    is_super_admin boolean,
    created_at timestamp with time zone,
    updated_at timestamp with time zone,
    phone text DEFAULT NULL::character varying,
    phone_confirmed_at timestamp with time zone,
    phone_change text DEFAULT ''::character varying,
    phone_change_token character varying(255) DEFAULT ''::character varying,
    phone_change_sent_at timestamp with time zone,
    confirmed_at timestamp with time zone GENERATED ALWAYS AS (LEAST(email_confirmed_at, phone_confirmed_at)) STORED,
    email_change_token_current character varying(255) DEFAULT ''::character varying,
    email_change_confirm_status smallint DEFAULT 0,
    banned_until timestamp with time zone,
    reauthentication_token character varying(255) DEFAULT ''::character varying,
    reauthentication_sent_at timestamp with time zone,
    is_sso_user boolean DEFAULT false NOT NULL,
    deleted_at timestamp with time zone,
    is_anonymous boolean DEFAULT false NOT NULL,
    CONSTRAINT users_email_change_confirm_status_check CHECK (((email_change_confirm_status >= 0) AND (email_change_confirm_status <= 2)))
);


--
-- Name: TABLE users; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON TABLE auth.users IS 'Auth: Stores user login data within a secure schema.';


--
-- Name: COLUMN users.is_sso_user; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON COLUMN auth.users.is_sso_user IS 'Auth: Set this column to true when the account comes from SSO. These accounts can have duplicate emails.';


--
-- Name: webauthn_challenges; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.webauthn_challenges (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid,
    challenge_type text NOT NULL,
    session_data jsonb NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    CONSTRAINT webauthn_challenges_challenge_type_check CHECK ((challenge_type = ANY (ARRAY['signup'::text, 'registration'::text, 'authentication'::text])))
);


--
-- Name: webauthn_credentials; Type: TABLE; Schema: auth; Owner: -
--

CREATE TABLE auth.webauthn_credentials (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    credential_id bytea NOT NULL,
    public_key bytea NOT NULL,
    attestation_type text DEFAULT ''::text NOT NULL,
    aaguid uuid,
    sign_count bigint DEFAULT 0 NOT NULL,
    transports jsonb DEFAULT '[]'::jsonb NOT NULL,
    backup_eligible boolean DEFAULT false NOT NULL,
    backed_up boolean DEFAULT false NOT NULL,
    friendly_name text DEFAULT ''::text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    last_used_at timestamp with time zone
);


--
-- Name: _backup_sportitems_20260404; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public._backup_sportitems_20260404 (
    id integer,
    category_id integer,
    name text,
    sku text,
    size text,
    color text,
    cost_price numeric(12,2),
    selling_price numeric(12,2),
    stock_quantity integer,
    low_stock_threshold integer,
    image_urls text[]
);


--
-- Name: categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.categories (
    id integer NOT NULL,
    name text NOT NULL,
    description text
);


--
-- Name: categories_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.categories_id_seq OWNED BY public.categories.id;


--
-- Name: customerorders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.customerorders (
    id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    customer_name text NOT NULL,
    customer_phone text NOT NULL,
    shipping_address text,
    order_type text,
    status text DEFAULT 'Pending'::text,
    payment_status text DEFAULT 'Unpaid'::text,
    total_amount numeric(12,2) DEFAULT 0,
    notes text,
    seller_id integer,
    seller_name character varying(100) DEFAULT ''::character varying NOT NULL,
    customer_id integer,
    payment_method text,
    received_amount numeric DEFAULT 0,
    shift_id integer,
    CONSTRAINT customerorders_order_type_check CHECK ((order_type = ANY (ARRAY['AtStore'::text, 'Delivery'::text]))),
    CONSTRAINT customerorders_payment_method_check CHECK ((payment_method = ANY (ARRAY['Cash'::text, 'BankTransfer'::text, 'COD'::text])))
);


--
-- Name: customerorders_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.customerorders_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: customerorders_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.customerorders_id_seq OWNED BY public.customerorders.id;


--
-- Name: customers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.customers (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    phone character varying(20) NOT NULL,
    address text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: customers_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.customers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: customers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.customers_id_seq OWNED BY public.customers.id;


--
-- Name: orderdetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.orderdetails (
    id integer NOT NULL,
    order_id integer,
    item_id integer,
    quantity integer NOT NULL,
    unit_price numeric(12,2) NOT NULL,
    item_name text,
    variant_id bigint,
    size text,
    color text
);


--
-- Name: orderdetails_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.orderdetails_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: orderdetails_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.orderdetails_id_seq OWNED BY public.orderdetails.id;


--
-- Name: shifts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.shifts (
    id integer NOT NULL,
    user_id integer NOT NULL,
    start_time timestamp with time zone DEFAULT now(),
    end_time timestamp with time zone,
    starting_cash numeric DEFAULT 0,
    actual_cash_total numeric DEFAULT 0,
    notes text,
    status text DEFAULT 'Open'::text
);


--
-- Name: shifts_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.shifts ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.shifts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sportitem_variants; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sportitem_variants (
    id bigint NOT NULL,
    sportitem_id integer NOT NULL,
    size text,
    color text,
    stock_quantity integer DEFAULT 0 NOT NULL,
    sku text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT sportitem_variants_stock_non_negative CHECK ((stock_quantity >= 0))
);


--
-- Name: sportitem_variants_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.sportitem_variants_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: sportitem_variants_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.sportitem_variants_id_seq OWNED BY public.sportitem_variants.id;


--
-- Name: sportitems; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sportitems (
    id integer NOT NULL,
    category_id integer,
    name text NOT NULL,
    cost_price numeric(12,2) DEFAULT 0,
    selling_price numeric(12,2) DEFAULT 0,
    stock_quantity integer DEFAULT 0,
    low_stock_threshold integer DEFAULT 5,
    image_urls text[] DEFAULT '{}'::text[],
    description text
);


--
-- Name: sportitems_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.sportitems_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: sportitems_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.sportitems_id_seq OWNED BY public.sportitems.id;


--
-- Name: suppliers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.suppliers (
    id integer NOT NULL,
    name text NOT NULL,
    contact_phone text,
    supplier_type text
);


--
-- Name: suppliers_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.suppliers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: suppliers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.suppliers_id_seq OWNED BY public.suppliers.id;


--
-- Name: supplydetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.supplydetails (
    id integer NOT NULL,
    supply_id integer,
    item_id integer,
    quantity integer NOT NULL,
    import_price numeric(12,2) NOT NULL,
    variant_id integer
);


--
-- Name: supplydetails_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.supplydetails_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: supplydetails_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.supplydetails_id_seq OWNED BY public.supplydetails.id;


--
-- Name: supplyorders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.supplyorders (
    id integer NOT NULL,
    supplier_id integer,
    import_date timestamp with time zone DEFAULT now(),
    total_cost numeric(12,2) DEFAULT 0
);


--
-- Name: supplyorders_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.supplyorders_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: supplyorders_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.supplyorders_id_seq OWNED BY public.supplyorders.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id integer NOT NULL,
    email character varying(255) NOT NULL,
    password character varying(255) NOT NULL,
    role character varying(20) DEFAULT 'sale'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: view_low_stock_alert; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.view_low_stock_alert AS
 SELECT s.name,
    (COALESCE(sum(v.stock_quantity), (0)::bigint))::integer AS stock_quantity
   FROM (public.sportitems s
     LEFT JOIN public.sportitem_variants v ON ((v.sportitem_id = s.id)))
  GROUP BY s.id, s.name, s.low_stock_threshold
 HAVING (COALESCE(sum(v.stock_quantity), (0)::bigint) <= COALESCE(s.low_stock_threshold, 5));


--
-- Name: view_revenue_report; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public.view_revenue_report AS
 SELECT date_trunc('day'::text, created_at) AS date,
    count(id) AS total_orders,
    sum(total_amount) AS gross_revenue
   FROM public.customerorders
  WHERE (status = 'Completed'::text)
  GROUP BY (date_trunc('day'::text, created_at));


--
-- Name: messages; Type: TABLE; Schema: realtime; Owner: -
--

CREATE TABLE realtime.messages (
    topic text NOT NULL,
    extension text NOT NULL,
    payload jsonb,
    event text,
    private boolean DEFAULT false,
    updated_at timestamp without time zone DEFAULT now() NOT NULL,
    inserted_at timestamp without time zone DEFAULT now() NOT NULL,
    id uuid DEFAULT gen_random_uuid() NOT NULL
)
PARTITION BY RANGE (inserted_at);


--
-- Name: schema_migrations; Type: TABLE; Schema: realtime; Owner: -
--

CREATE TABLE realtime.schema_migrations (
    version bigint NOT NULL,
    inserted_at timestamp(0) without time zone
);


--
-- Name: subscription; Type: TABLE; Schema: realtime; Owner: -
--

CREATE TABLE realtime.subscription (
    id bigint NOT NULL,
    subscription_id uuid NOT NULL,
    entity regclass NOT NULL,
    filters realtime.user_defined_filter[] DEFAULT '{}'::realtime.user_defined_filter[] NOT NULL,
    claims jsonb NOT NULL,
    claims_role regrole GENERATED ALWAYS AS (realtime.to_regrole((claims ->> 'role'::text))) STORED NOT NULL,
    created_at timestamp without time zone DEFAULT timezone('utc'::text, now()) NOT NULL,
    action_filter text DEFAULT '*'::text,
    CONSTRAINT subscription_action_filter_check CHECK ((action_filter = ANY (ARRAY['*'::text, 'INSERT'::text, 'UPDATE'::text, 'DELETE'::text])))
);


--
-- Name: subscription_id_seq; Type: SEQUENCE; Schema: realtime; Owner: -
--

ALTER TABLE realtime.subscription ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME realtime.subscription_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: buckets; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.buckets (
    id text NOT NULL,
    name text NOT NULL,
    owner uuid,
    created_at timestamp with time zone DEFAULT now(),
    updated_at timestamp with time zone DEFAULT now(),
    public boolean DEFAULT false,
    avif_autodetection boolean DEFAULT false,
    file_size_limit bigint,
    allowed_mime_types text[],
    owner_id text,
    type storage.buckettype DEFAULT 'STANDARD'::storage.buckettype NOT NULL
);


--
-- Name: COLUMN buckets.owner; Type: COMMENT; Schema: storage; Owner: -
--

COMMENT ON COLUMN storage.buckets.owner IS 'Field is deprecated, use owner_id instead';


--
-- Name: buckets_analytics; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.buckets_analytics (
    name text NOT NULL,
    type storage.buckettype DEFAULT 'ANALYTICS'::storage.buckettype NOT NULL,
    format text DEFAULT 'ICEBERG'::text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    deleted_at timestamp with time zone
);


--
-- Name: buckets_vectors; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.buckets_vectors (
    id text NOT NULL,
    type storage.buckettype DEFAULT 'VECTOR'::storage.buckettype NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


--
-- Name: migrations; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.migrations (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    hash character varying(40) NOT NULL,
    executed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: objects; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.objects (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    bucket_id text,
    name text,
    owner uuid,
    created_at timestamp with time zone DEFAULT now(),
    updated_at timestamp with time zone DEFAULT now(),
    last_accessed_at timestamp with time zone DEFAULT now(),
    metadata jsonb,
    path_tokens text[] GENERATED ALWAYS AS (string_to_array(name, '/'::text)) STORED,
    version text,
    owner_id text,
    user_metadata jsonb
);


--
-- Name: COLUMN objects.owner; Type: COMMENT; Schema: storage; Owner: -
--

COMMENT ON COLUMN storage.objects.owner IS 'Field is deprecated, use owner_id instead';


--
-- Name: s3_multipart_uploads; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.s3_multipart_uploads (
    id text NOT NULL,
    in_progress_size bigint DEFAULT 0 NOT NULL,
    upload_signature text NOT NULL,
    bucket_id text NOT NULL,
    key text NOT NULL COLLATE pg_catalog."C",
    version text NOT NULL,
    owner_id text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    user_metadata jsonb,
    metadata jsonb
);


--
-- Name: s3_multipart_uploads_parts; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.s3_multipart_uploads_parts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    upload_id text NOT NULL,
    size bigint DEFAULT 0 NOT NULL,
    part_number integer NOT NULL,
    bucket_id text NOT NULL,
    key text NOT NULL COLLATE pg_catalog."C",
    etag text NOT NULL,
    owner_id text,
    version text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


--
-- Name: vector_indexes; Type: TABLE; Schema: storage; Owner: -
--

CREATE TABLE storage.vector_indexes (
    id text DEFAULT gen_random_uuid() NOT NULL,
    name text NOT NULL COLLATE pg_catalog."C",
    bucket_id text NOT NULL,
    data_type text NOT NULL,
    dimension integer NOT NULL,
    distance_metric text NOT NULL,
    metadata_configuration jsonb,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


--
-- Name: refresh_tokens id; Type: DEFAULT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.refresh_tokens ALTER COLUMN id SET DEFAULT nextval('auth.refresh_tokens_id_seq'::regclass);


--
-- Name: categories id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.categories ALTER COLUMN id SET DEFAULT nextval('public.categories_id_seq'::regclass);


--
-- Name: customerorders id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customerorders ALTER COLUMN id SET DEFAULT nextval('public.customerorders_id_seq'::regclass);


--
-- Name: customers id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers ALTER COLUMN id SET DEFAULT nextval('public.customers_id_seq'::regclass);


--
-- Name: orderdetails id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.orderdetails ALTER COLUMN id SET DEFAULT nextval('public.orderdetails_id_seq'::regclass);


--
-- Name: sportitem_variants id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitem_variants ALTER COLUMN id SET DEFAULT nextval('public.sportitem_variants_id_seq'::regclass);


--
-- Name: sportitems id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitems ALTER COLUMN id SET DEFAULT nextval('public.sportitems_id_seq'::regclass);


--
-- Name: suppliers id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.suppliers ALTER COLUMN id SET DEFAULT nextval('public.suppliers_id_seq'::regclass);


--
-- Name: supplydetails id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplydetails ALTER COLUMN id SET DEFAULT nextval('public.supplydetails_id_seq'::regclass);


--
-- Name: supplyorders id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplyorders ALTER COLUMN id SET DEFAULT nextval('public.supplyorders_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: audit_log_entries; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.audit_log_entries (instance_id, id, payload, created_at, ip_address) FROM stdin;
\.


--
-- Data for Name: custom_oauth_providers; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.custom_oauth_providers (id, provider_type, identifier, name, client_id, client_secret, acceptable_client_ids, scopes, pkce_enabled, attribute_mapping, authorization_params, enabled, email_optional, issuer, discovery_url, skip_nonce_check, cached_discovery, discovery_cached_at, authorization_url, token_url, userinfo_url, jwks_uri, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: flow_state; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.flow_state (id, user_id, auth_code, code_challenge_method, code_challenge, provider_type, provider_access_token, provider_refresh_token, created_at, updated_at, authentication_method, auth_code_issued_at, invite_token, referrer, oauth_client_state_id, linking_target_id, email_optional) FROM stdin;
\.


--
-- Data for Name: identities; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.identities (provider_id, user_id, identity_data, provider, last_sign_in_at, created_at, updated_at, id) FROM stdin;
fef6f914-35bf-4934-b214-84ee361796ef	fef6f914-35bf-4934-b214-84ee361796ef	{"sub": "fef6f914-35bf-4934-b214-84ee361796ef", "email": "admin@prosport.com", "email_verified": false, "phone_verified": false}	email	2026-04-09 12:10:52.315561+00	2026-04-09 12:10:52.315619+00	2026-04-09 12:10:52.315619+00	6bd5409e-dd78-4f9a-9801-31b4977289f6
\.


--
-- Data for Name: instances; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.instances (id, uuid, raw_base_config, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: mfa_amr_claims; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.mfa_amr_claims (session_id, created_at, updated_at, authentication_method, id) FROM stdin;
\.


--
-- Data for Name: mfa_challenges; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.mfa_challenges (id, factor_id, created_at, verified_at, ip_address, otp_code, web_authn_session_data) FROM stdin;
\.


--
-- Data for Name: mfa_factors; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.mfa_factors (id, user_id, friendly_name, factor_type, status, created_at, updated_at, secret, phone, last_challenged_at, web_authn_credential, web_authn_aaguid, last_webauthn_challenge_data) FROM stdin;
\.


--
-- Data for Name: oauth_authorizations; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.oauth_authorizations (id, authorization_id, client_id, user_id, redirect_uri, scope, state, resource, code_challenge, code_challenge_method, response_type, status, authorization_code, created_at, expires_at, approved_at, nonce) FROM stdin;
\.


--
-- Data for Name: oauth_client_states; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.oauth_client_states (id, provider_type, code_verifier, created_at) FROM stdin;
\.


--
-- Data for Name: oauth_clients; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.oauth_clients (id, client_secret_hash, registration_type, redirect_uris, grant_types, client_name, client_uri, logo_uri, created_at, updated_at, deleted_at, client_type, token_endpoint_auth_method) FROM stdin;
\.


--
-- Data for Name: oauth_consents; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.oauth_consents (id, user_id, client_id, scopes, granted_at, revoked_at) FROM stdin;
\.


--
-- Data for Name: one_time_tokens; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.one_time_tokens (id, user_id, token_type, token_hash, relates_to, created_at, updated_at) FROM stdin;
c68fb45a-d72a-4c11-ad4d-6212ab54e2ff	fef6f914-35bf-4934-b214-84ee361796ef	recovery_token	7a3bb2a3253dcf7006fe2a79b184b2bc284ed34a61d023ab7a7aec94	admin@prosport.com	2026-04-09 12:48:17.187122	2026-04-09 12:48:17.187122
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.refresh_tokens (instance_id, id, token, user_id, revoked, created_at, updated_at, parent, session_id) FROM stdin;
\.


--
-- Data for Name: saml_providers; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.saml_providers (id, sso_provider_id, entity_id, metadata_xml, metadata_url, attribute_mapping, created_at, updated_at, name_id_format) FROM stdin;
\.


--
-- Data for Name: saml_relay_states; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.saml_relay_states (id, sso_provider_id, request_id, for_email, redirect_to, created_at, updated_at, flow_state_id) FROM stdin;
\.


--
-- Data for Name: schema_migrations; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.schema_migrations (version) FROM stdin;
20171026211738
20171026211808
20171026211834
20180103212743
20180108183307
20180119214651
20180125194653
00
20210710035447
20210722035447
20210730183235
20210909172000
20210927181326
20211122151130
20211124214934
20211202183645
20220114185221
20220114185340
20220224000811
20220323170000
20220429102000
20220531120530
20220614074223
20220811173540
20221003041349
20221003041400
20221011041400
20221020193600
20221021073300
20221021082433
20221027105023
20221114143122
20221114143410
20221125140132
20221208132122
20221215195500
20221215195800
20221215195900
20230116124310
20230116124412
20230131181311
20230322519590
20230402418590
20230411005111
20230508135423
20230523124323
20230818113222
20230914180801
20231027141322
20231114161723
20231117164230
20240115144230
20240214120130
20240306115329
20240314092811
20240427152123
20240612123726
20240729123726
20240802193726
20240806073726
20241009103726
20250717082212
20250731150234
20250804100000
20250901200500
20250903112500
20250904133000
20250925093508
20251007112900
20251104100000
20251111201300
20251201000000
20260115000000
20260121000000
20260219120000
20260302000000
\.


--
-- Data for Name: sessions; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.sessions (id, user_id, created_at, updated_at, factor_id, aal, not_after, refreshed_at, user_agent, ip, tag, oauth_client_id, refresh_token_hmac_key, refresh_token_counter, scopes) FROM stdin;
\.


--
-- Data for Name: sso_domains; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.sso_domains (id, sso_provider_id, domain, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: sso_providers; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.sso_providers (id, resource_id, created_at, updated_at, disabled) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.users (instance_id, id, aud, role, email, encrypted_password, email_confirmed_at, invited_at, confirmation_token, confirmation_sent_at, recovery_token, recovery_sent_at, email_change_token_new, email_change, email_change_sent_at, last_sign_in_at, raw_app_meta_data, raw_user_meta_data, is_super_admin, created_at, updated_at, phone, phone_confirmed_at, phone_change, phone_change_token, phone_change_sent_at, email_change_token_current, email_change_confirm_status, banned_until, reauthentication_token, reauthentication_sent_at, is_sso_user, deleted_at, is_anonymous) FROM stdin;
00000000-0000-0000-0000-000000000000	fef6f914-35bf-4934-b214-84ee361796ef	authenticated	authenticated	admin@prosport.com	$2a$10$YwMz.3kcXK89WV.8mAOAtupASf6mCcA924hfNlnLd8u4kZjHFaylK	2026-04-09 12:10:52.322491+00	\N		\N	7a3bb2a3253dcf7006fe2a79b184b2bc284ed34a61d023ab7a7aec94	2026-04-09 12:48:14.374754+00			\N	\N	{"provider": "email", "providers": ["email"]}	{"role": "owner", "display_name": "ProSport Owner"}	\N	2026-04-09 12:10:52.277313+00	2026-04-09 12:48:17.178+00	\N	\N			\N		0	\N		\N	f	\N	f
\.


--
-- Data for Name: webauthn_challenges; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.webauthn_challenges (id, user_id, challenge_type, session_data, created_at, expires_at) FROM stdin;
\.


--
-- Data for Name: webauthn_credentials; Type: TABLE DATA; Schema: auth; Owner: -
--

COPY auth.webauthn_credentials (id, user_id, credential_id, public_key, attestation_type, aaguid, sign_count, transports, backup_eligible, backed_up, friendly_name, created_at, updated_at, last_used_at) FROM stdin;
\.


--
-- Data for Name: _backup_sportitems_20260404; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public._backup_sportitems_20260404 (id, category_id, name, sku, size, color, cost_price, selling_price, stock_quantity, low_stock_threshold, image_urls) FROM stdin;
45	3	Balo Nike Brasilia	PK001	M	Đen	7.20	14.00	8	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
7	1	Adidas NMD R1	GIAY007	40-45	Đỏ	64.00	114.00	9	5	{https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1608231387042-66d1773070a5}
4	1	Nike Zoom Fly 5	GIAY004	40-43	Cam	60.00	107.60	6	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1605348532760-6753d2c43329}
5	1	Nike Revolution 6	GIAY005	38-45	Xám	30.00	54.00	20	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1539185441755-769473a23570,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
55	3	Bình Nước Adidas 600ml	PK011	600ml	Đen	2.00	4.00	38	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1602143407151-11115cd0a7c8,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
11	1	Puma RS-X	GIAY011	39-44	Vàng	48.00	84.00	10	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1551107696-a4b0c5a0d9a2}
12	1	Puma Suede Classic	GIAY012	40-45	Nâu	38.00	70.00	13	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
13	1	Puma Future Rider	GIAY013	38-44	Đen/Trắng	44.00	78.00	16	5	{https://images.unsplash.com/photo-1463100099107-aa0980c362e6,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
14	1	New Balance 574	GIAY014	39-45	Xám	56.00	98.00	9	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
15	1	New Balance 327	GIAY015	38-44	Cam	64.00	112.00	7	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
16	1	Converse Chuck Taylor	GIAY016	36-46	Đen	32.00	60.00	22	5	{https://images.unsplash.com/photo-1463100099107-aa0980c362e6,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
17	1	Converse Run Star Hike	GIAY017	37-43	Trắng	52.00	92.00	8	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1607522370275-f14206abe5d3}
18	1	Vans Old Skool	GIAY018	36-45	Đen/Trắng	34.00	62.00	17	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
69	1	SP bieu do	34234	32	red	\N	\N	\N	\N	{"https://qijxiegqhgkipqrmwupg.supabase.co/storage/v1/object/public/sport image/1c7958d7-78e8-40fd-b821-2dd2f195555c_Screenshot 2026-03-26 at 12.56.29.png"}
2	1	Nike Air Max 270	GIAY002	39-44	Đen	54.00	94.00	1	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1600185365926-3a2ce3cdb9eb}
58	3	Vớ Thể Thao Nike 3 Đôi	PK014	39-45	Đen	4.80	8.80	31	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1586350977771-b3b0abd50c82,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
10	1	Adidas Lite Racer 3.0	GIAY010	38-45	Xám	34.00	62.00	-6	5	{https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a,https://images.unsplash.com/photo-1549298916-b41d501d3772,"https://qijxiegqhgkipqrmwupg.supabase.co/storage/v1/object/public/sport image/1e726def-1af4-45a1-9932-4aee9e42a880_Screenshot 2026-04-02 at 00.20.49.png"}
51	3	Găng Tay Gym Adidas	PK007	S/M/L	Đen/Đỏ	2.80	5.60	25	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1598289431512-b97b0917affc,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
52	3	Băng Cuộn Y Band	PK008	5cm	Be	0.60	1.20	60	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576091160550-2173dba999ef,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
53	3	Băng Cuộn Kintex 5cm	PK009	5cm	Trắng	0.48	1.00	55	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576671081837-49000212a370,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
9	1	Adidas Stan Smith	GIAY009	39-46	Xanh Lá	52.00	90.00	14	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1606889464198-fcb18894cf50}
54	3	Bình Nước Nike 750ml	PK010	750ml	Xanh	2.20	4.40	40	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1523362628745-0c100150b504,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
41	2	Áo Tank Nike Pro	AOO019	S-XL	Đen	6.00	11.60	40	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
59	3	Vớ Thể Thao Adidas 3 Đôi	PK015	39-45	Trắng	4.40	8.00	42	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1582576163090-09d3b6f8a969,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
60	3	Túi Lưới Nike	PK016	M	Xanh	1.80	3.60	50	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1591561954557-26941169b49e,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
61	3	Dây Nhảy Be Fit	PK017	2.8m	Hồng	1.20	2.40	35	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
19	1	Vans Sk8-Hi	GIAY019	38-44	Đỏ	36.00	66.00	12	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1551107696-a4b0c5a0d9a2}
20	1	Vans Slip-On	GIAY020	36-44	Trắng	28.00	52.00	19	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1560769629-975ec94e6a86}
21	1	Anta KT9 Basketball	GIAY021	41-46	Đỏ/Đen	68.00	119.60	5	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1575537302964-96cd47c06b1b}
22	1	Li-Ning Way of Wade 10	GIAY022	40-45	Vàng	76.00	134.00	4	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1579338559194-a162d19bf842}
24	2	Áo Thun Nike Dri-FIT	AOO002	S-XXL	Xanh Navy	7.20	14.00	30	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1581655353564-df123a1eb820}
25	2	Áo Thun Nike Dri-FIT	AOO003	S-XXL	Trắng	7.20	14.00	28	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
27	2	Áo Polo Adidas Climalite	AOO005	S-XXL	Đen	8.00	15.60	22	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf}
28	2	Áo Polo Lacoste Sport	AOO006	S-XXL	Trắng	10.00	19.20	18	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1618354691373-d851c5c3a990}
30	2	Quần Jogger Adidas Essential	AOO008	S-XXL	Xám	10.40	20.80	23	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1594938298603-c8148c4dae35}
31	2	Quần Jogger Puma Core	AOO009	S-XXL	Navy	9.60	19.20	19	8	{https://images.unsplash.com/photo-1506629082955-511b1aa562c8,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
32	2	Quần Short Nike Flex	AOO010	S-XXL	Đen	8.80	16.80	32	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1591195853828-11db59a44f6b}
33	2	Quần Short Adidas Linear	AOO011	S-XXL	Xanh Navy	8.00	15.60	28	8	{https://images.unsplash.com/photo-1434682772747-f16d3ea162c3,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
34	2	Quần Short Uniqlo Dry	AOO012	S-XXL	Trắng	7.60	14.80	25	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1566241440091-ec10de8db2e1}
37	2	Bộ Đồ Thể Thao Nữ Nike	AOO015	S-XL	Hồng	16.80	32.00	16	5	{https://images.unsplash.com/photo-1485965120184-e220f721d03e,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
38	2	Bộ Đồ Thể Thao Nữ Adidas	AOO016	S-XL	Tím	16.00	31.20	13	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1518310383802-640c2de311b2,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
39	2	Áo Khoác Nike Windrunner	AOO017	S-XXL	Đen	15.20	28.80	10	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1591047139829-d91aecb6caea}
40	2	Áo Khoác Adidas SST	AOO018	S-XXL	Navy	14.00	27.20	11	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1551028719-00167b16eac5,https://images.unsplash.com/photo-1562157873-818bc0726f68}
42	2	Quần Legging Nike Power	AOO020	S-XL	Đen	10.00	19.20	22	8	{https://images.unsplash.com/photo-1506629082955-511b1aa562c8,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
43	2	Áo Jersey Real Madrid	AOO021	S-XXL	Trắng	12.00	23.20	15	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1551958219-acbc608c6377,https://images.unsplash.com/photo-1562157873-818bc0726f68}
44	2	Áo Jersey Manchester United	AOO022	S-XXL	Đỏ	12.00	23.20	15	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1574629810360-7efbbe195018}
46	3	Balo Adidas Classic 3S	PK002	M	Xám	6.40	12.80	18	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
48	3	Túi Đeo Chéo Nike	PK004	M	Đen	3.20	6.40	30	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1548036328-c9fa89d128fa,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
49	3	Túi Đeo Chéo Adidas	PK005	M	Navy	3.00	6.00	28	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1591561954557-26941169b49e,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
50	3	Găng Tay Chạy Bộ Nike	PK006	S/M/L	Đen	2.40	4.80	35	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
56	3	Nón Nike Dri-FIT Cap	PK012	M	Đen	3.60	7.20	25	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1588850561407-ed78c282e89b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
57	3	Nón Adidas Golf Cap	PK013	M	Navy	3.40	6.80	22	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1534215754734-18e55d13e346,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
62	3	Bóng Yoga 65cm	PK018	65cm	Tím	3.20	6.40	15	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1544367567-0f2fcb009e0b,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
63	3	Băng Đỡ Cổ Chân	PK019	S/M	Be	1.00	2.00	30	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
64	3	Băng Đỡ Đầu Gối	PK020	S/M	Đen	1.20	2.40	28	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576671081837-49000212a370,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
65	3	Khay Đựng Phụ Kiện	PK021	M	Đen	1.40	2.80	20	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
66	3	Túi Chườm Đá Thể Thao	PK022	M	Xanh Navy	1.80	3.60	18	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
1	1	Nike Air Max 90	GIAY001	40-45	Trắng	48.00	86.00	10	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}
23	2	Áo Thun Nike Dri-FIT	AOO001	S-XXL	Đen	7.20	14.00	31	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
35	2	Bộ Đồ Thể Thao Nam Nike	AOO013	S-XXL	Đen/Đỏ	18.00	34.00	12	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}
6	1	Adidas Ultraboost 22	GIAY006	39-44	Trắng/Đen	72.00	126.00	0	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1556906781-9a412961c28c}
3	1	Nike Pegasus 40	GIAY003	38-44	Xanh Navy	44.00	78.00	6	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa}
29	2	Quần Jogger Nike Sportswear	AOO007	S-XXL	Đen	11.20	22.00	11	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1552902865-b72c031ac5ea,https://images.unsplash.com/photo-1562157873-818bc0726f68}
36	2	Bộ Đồ Thể Thao Nam Adidas	AOO014	S-XXL	Navy/Xám	16.80	32.80	7	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1556821840-3a63f95609a7,https://images.unsplash.com/photo-1562157873-818bc0726f68}
47	3	Balo Puma Backpack	PK003	M	Đen	5.60	11.20	17	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1581605405669-fcdf81165afa,https://images.unsplash.com/photo-1622560480654-d96214fdc887}
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.categories (id, name, description) FROM stdin;
2	Quần Áo Thể Thao	Áo thun, áo polo, quần jogger, quần short, bộ đồ, áo khoác, legging, jersey bóng đá nam/nữ
3	Phụ Kiện Thể Thao	Balo, túi đeo chéo, găng tay, băng cuộn, bình nước, nón, vớ, dây nhảy, bóng yoga, băng đỡ khớp
1	Giày Thể Thao	Giày sneaker, running, basketball Nike, Adidas, Puma, New Balance, Converse, Vans, Anta, Li-Ning
18	Running Shoes	\N
19	Casual Shoes	\N
20	Tops	\N
21	Bottoms	\N
22	Gloves	\N
23	Accessories	\N
24	Bags	\N
25	Shoes	\N
26	Equipment	\N
27	Clothing	\N
\.


--
-- Data for Name: customerorders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.customerorders (id, created_at, customer_name, customer_phone, shipping_address, order_type, status, payment_status, total_amount, notes, seller_id, seller_name, customer_id, payment_method, received_amount, shift_id) FROM stdin;
22	2026-05-03 10:02:01.772756+00	Walk-in Customer	0123456789	\N	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	13	\N	0	\N
23	2026-05-03 10:02:49.384475+00	Walk-in Customer	0000000000	abc	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	14	\N	0	\N
24	2026-05-03 10:03:52.571035+00	gege	1111	\N	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	9	\N	0	\N
25	2026-05-03 10:40:20.459721+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	11	\N	0	\N
26	2026-05-07 16:38:18.245933+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	90.00	Created from POS	2	admin@prosport.com	11	\N	0	\N
10	2026-04-05 16:58:24.839869+00	hihi	123123	\N	\N	Cancelled	Paid	90.00		\N		3	\N	0	\N
27	2026-05-07 16:40:27.445126+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	11	\N	0	\N
13	2026-04-06 07:50:29.708267+00	gege	1111	\N	\N	Pending	Paid	434.00		\N		9	\N	0	\N
12	2026-04-06 07:36:32.572561+00	Quoc 	111	DHKHTN	Delivery	Processing	Unpaid	170.00		\N		8	\N	0	\N
4	2026-03-24 11:32:14.120972+00	Phạm Thu Hà	0905554321	78 Trần Hưng Đạo, Q.10, TP.HCM	Delivery	Delivered	Paid	2500000.00	Chờ chuyển khoản	\N		4	\N	0	\N
3	2026-03-24 11:32:14.120972+00	Lê Hoàng Nam	0933123456	\N	AtStore	Pending	Unpaid	0.00	Mua tại quầy	\N		1	\N	0	\N
1	2026-03-24 11:32:14.120972+00	Nguyễn Văn Minh	0909123456	123 Lê Lợi, Q.1, TP.HCM	Delivery	Pending	Paid	3700000.00	Giao giờ hành chính	\N		6	\N	0	\N
7	2026-04-05 11:18:52.10119+00	Le Mai Hoai Bao	0943802900	Pasteur	AtStore	Delivered	Paid	10000.00	\N	\N		2	\N	0	\N
8	2026-04-05 16:51:52.628955+00	Toi la john	1234	\N	AtStore	Cancelled	Paid	0.00		\N		7	\N	0	\N
9	2026-04-05 16:53:35.39155+00	toi la hihi	094323434	\N	\N	Processing	Unpaid	0.00		\N		5	\N	0	\N
14	2026-04-26 05:19:52.668582+00	Bao	0943802900	\N	\N	Pending	Unpaid	62.00		2	admin@prosport.com	\N	\N	0	\N
15	2026-04-26 06:21:00.035503+00	Bao	0943802900	\N	\N	Pending	Unpaid	124.00		2	admin@prosport.com	\N	\N	0	\N
28	2026-05-08 17:46:18.829187+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	114.00	Created from POS	2	admin@prosport.com	11	Cash	300	1
29	2026-05-08 17:46:44.156467+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	114.00	Created from POS	2	admin@prosport.com	11	Cash	500	1
30	2026-05-08 17:47:10.632188+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	237.00	Created from POS	2	admin@prosport.com	11	Cash	500	1
31	2026-05-08 17:48:18.904983+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	119.60	Created from POS	2	admin@prosport.com	11	COD	129.17	1
32	2026-05-08 19:23:01.425711+00	Walk-in Customer	0918448449	\N	AtStore	Pending	Paid	123.00	Created from POS	2	admin@prosport.com	11	Cash	10000	2
33	2026-05-11 12:54:33.187634+00	Hoai Bao	0943802900	\N	AtStore	Pending	Paid	176.00		2	admin@prosport.com	\N	Cash	0	\N
37	2026-05-11 14:41:23.46607+00	john	09432	\N	\N	Pending	Unpaid	62.00		2	admin@prosport.com	\N	BankTransfer	0	\N
18	2026-05-03 09:50:14.267178+00	Le Mai Hoai Bao	0943802900	Pasteur	AtStore	Completed	Paid	228.00	Created from POS	2	admin@prosport.com	2	\N	0	\N
19	2026-05-03 10:00:29.107085+00	Walk-in Customer	0918448449	\N	AtStore	Completed	Paid	123.00	Created from POS	2	admin@prosport.com	11	\N	0	\N
20	2026-05-03 10:00:57.962317+00	Walk-in Customer	0356988346	\N	AtStore	Completed	Paid	62.00	Created from POS	2	admin@prosport.com	12	\N	0	\N
21	2026-05-03 10:01:33.971063+00	Walk-in Customer	0123456789	\N	AtStore	Completed	Paid	369.00	Created from POS	2	admin@prosport.com	13	\N	0	\N
34	2026-05-11 13:12:00.478819+00	Le Mai Hoai Bao	0943802900	\N	AtStore	Pending	Paid	90.00		2	admin@prosport.com	\N	Cash	0	\N
38	2026-05-13 03:06:56.422258+00	Khánh 	123	\N	AtStore	Pending	Unpaid	25.00		2	admin@prosport.com	\N	Cash	0	\N
35	2026-05-11 13:26:08.721344+00	Lam Huu Khanh	113	Quan Cam	Delivery	Delivered	Unpaid	114.00		2	admin@prosport.com	\N	Cash	0	\N
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.customers (id, name, phone, address, created_at) FROM stdin;
1	Lê Hoàng Nam	0933123456	\N	2026-04-24 17:21:36.59437+00
2	Le Mai Hoai Bao	0943802900	Pasteur	2026-04-24 17:21:36.59437+00
3	hihi	123123	\N	2026-04-24 17:21:36.59437+00
4	Phạm Thu Hà	0905554321	78 Trần Hưng Đạo, Q.10, TP.HCM	2026-04-24 17:21:36.59437+00
5	toi la hihi	094323434	\N	2026-04-24 17:21:36.59437+00
6	Nguyễn Văn Minh	0909123456	123 Lê Lợi, Q.1, TP.HCM	2026-04-24 17:21:36.59437+00
7	Toi la john	1234	\N	2026-04-24 17:21:36.59437+00
8	Quoc 	111	DHKHTN	2026-04-24 17:21:36.59437+00
9	gege	1111	\N	2026-04-24 17:21:36.59437+00
11	Walk-in Customer	0918448449	\N	2026-05-03 10:00:28.879599+00
12	Walk-in Customer	0356988346	\N	2026-05-03 10:00:57.740584+00
13	Walk-in Customer	0123456789	\N	2026-05-03 10:01:33.736462+00
14	Walk-in Customer	0000000000	abc	2026-05-03 10:02:49.16522+00
\.


--
-- Data for Name: orderdetails; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.orderdetails (id, order_id, item_id, quantity, unit_price, item_name, variant_id, size, color) FROM stdin;
113	10	9	1	90.00	Adidas Stan Smith	\N	\N	\N
115	12	35	5	34.00	Bộ Đồ Thể Thao Nam Nike	\N	\N	\N
116	13	10	7	62.00	Adidas Lite Racer 3.0	\N	\N	\N
117	14	10	1	62.00	Adidas Lite Racer 3.0	76	38-44	Đen
118	15	10	2	62.00	Adidas Lite Racer 3.0	75	38-45	Xám
121	18	7	2	114.00	Adidas NMD R1	\N	\N	\N
122	19	77	1	123.00	aaaa	\N	\N	\N
123	20	10	1	62.00	Adidas Lite Racer 3.0	\N	\N	\N
124	21	77	3	123.00	aaaa	\N	\N	\N
125	22	77	1	123.00	aaaa	\N	\N	\N
126	23	71	1	123.00		\N	\N	\N
127	24	71	1	123.00		\N	\N	\N
128	25	71	1	123.00		\N	\N	\N
129	26	9	1	90.00	Adidas Stan Smith	\N	\N	\N
130	27	71	1	123.00	Vòng tay thể thao	\N	\N	\N
131	28	7	1	114.00	Adidas NMD R1	2	40-45	Đỏ
132	29	7	1	114.00	Adidas NMD R1	2	40-45	Đỏ
133	30	7	1	114.00	Adidas NMD R1	2	40-45	Đỏ
134	30	77	1	123.00	aaaa	80	123	red
135	31	21	1	119.60	Anta KT9 Basketball	29	41-46	Đỏ/Đen
136	32	77	1	123.00	aaaa	80	123	red
137	33	7	1	114.00	Adidas NMD R1	2	40-45	Đỏ
138	33	10	1	62.00	Adidas Lite Racer 3.0	75	38-45	Xám
139	34	9	1	90.00	Adidas Stan Smith	21	39-46	Xanh Lá
140	35	7	1	114.00	Adidas NMD R1	2	40-45	Đỏ
141	37	10	1	62.00	Adidas Lite Racer 3.0	75	38-45	Xám
142	38	80	1	25.00	HCMUS Mascot Limited Edition Sport Tee	85	S	Blue/White
105	1	1	1	2150000.00	Nike Air Max 90	\N	\N	\N
106	1	23	2	350000.00	Áo Thun Nike Dri-FIT	\N	\N	\N
107	1	35	1	850000.00	Bộ Đồ Thể Thao Nam Nike	\N	\N	\N
108	1	1	1	2150000.00	Nike Air Max 90	\N	\N	\N
109	1	23	2	350000.00	Áo Thun Nike Dri-FIT	\N	\N	\N
110	1	35	1	850000.00	Bộ Đồ Thể Thao Nam Nike	\N	\N	\N
60	4	3	1	1950000.00	Nike Pegasus 40	\N	\N	\N
61	4	29	1	550000.00	Quần Jogger Nike Sportswear	\N	\N	\N
\.


--
-- Data for Name: shifts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.shifts (id, user_id, start_time, end_time, starting_cash, actual_cash_total, notes, status) FROM stdin;
1	2	2026-05-08 17:45:18.035059+00	2026-05-08 19:10:26.692905+00	300	464	\N	Closed
2	2	2026-05-08 19:22:49.339242+00	2026-05-08 19:23:22.198086+00	500	122	Nah I dont know	Closed
\.


--
-- Data for Name: sportitem_variants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sportitem_variants (id, sportitem_id, size, color, stock_quantity, sku, created_at, updated_at) FROM stdin;
85	80	S	Blue/White	9	HCMUS-TEE-S	2026-04-24 15:54:34.979253+00	2026-04-24 15:54:34.979253+00
3	4	40-43	Cam	6	GIAY004	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
4	5	38-45	Xám	20	GIAY005	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
158	108	40	White/Blue	0	TD-CANV-40	2026-05-11 14:57:52.575789+00	2026-05-11 14:57:52.575789+00
6	11	39-44	Vàng	10	GIAY011	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
7	12	40-45	Nâu	13	GIAY012	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
8	13	38-44	Đen/Trắng	16	GIAY013	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
9	14	39-45	Xám	9	GIAY014	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
10	15	38-44	Cam	7	GIAY015	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
11	16	36-46	Đen	22	GIAY016	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
12	17	37-43	Trắng	8	GIAY017	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
13	18	36-45	Đen/Trắng	17	GIAY018	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
21	9	39-46	Xanh Lá	12	GIAY009	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
15	2	39-44	Đen	1	GIAY002	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
159	108	41	White/Blue	0	TD-CANV-41	2026-05-11 14:57:52.690194+00	2026-05-11 14:57:52.690194+00
23	41	S-XL	Đen	40	AOO019	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
27	19	38-44	Đỏ	12	GIAY019	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
28	20	36-44	Trắng	19	GIAY020	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
30	22	40-45	Vàng	4	GIAY022	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
33	27	S-XXL	Đen	22	AOO005	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
34	28	S-XXL	Trắng	18	AOO006	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
35	30	S-XXL	Xám	23	AOO008	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
36	31	S-XXL	Navy	19	AOO009	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
37	32	S-XXL	Đen	32	AOO010	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
38	33	S-XXL	Xanh Navy	28	AOO011	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
39	34	S-XXL	Trắng	25	AOO012	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
40	37	S-XL	Hồng	16	AOO015	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
41	38	S-XL	Tím	13	AOO016	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
42	39	S-XXL	Đen	10	AOO017	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
43	40	S-XXL	Navy	11	AOO018	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
44	42	S-XL	Đen	22	AOO020	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
45	43	S-XXL	Trắng	15	AOO021	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
58	1	40-45	Trắng	10	GIAY001	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
60	35	S-XXL	Đen/Đỏ	12	AOO013	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
61	6	39-44	Trắng/Đen	0	GIAY006	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
63	29	S-XXL	Đen	11	AOO007	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
64	36	S-XXL	Navy/Xám	7	AOO014	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
72	23	S-XXL	Đen	30	AOO001	2026-04-04 10:13:21.333574+00	2026-04-04 10:13:21.333574+00
73	23	S-XXL	Trắng	28	AOO003	2026-04-04 10:13:21.44491+00	2026-04-04 10:13:21.44491+00
74	23	S-XXL	Xanh Navy	30	AOO002	2026-04-04 10:13:21.550359+00	2026-04-04 10:13:21.550359+00
82	79	8	White	10	AF1-WHT-008	2026-04-24 08:48:29.869388+00	2026-04-24 08:48:29.869388+00
83	79	9	White	10	AF1-WHT-009	2026-04-24 08:48:30.01169+00	2026-04-24 08:48:30.01169+00
84	79	10	White	10	AF1-WHT-010	2026-04-24 08:48:30.159336+00	2026-04-24 08:48:30.159336+00
86	80	M	Blue/White	10	HCMUS-TEE-M	2026-04-24 15:54:35.117634+00	2026-04-24 15:54:35.117634+00
87	80	L	Blue/White	10	HCMUS-TEE-L	2026-04-24 15:54:35.244183+00	2026-04-24 15:54:35.244183+00
88	3	38-44	Xanh Navy	6	GIAY003	2026-04-24 15:57:56.001103+00	2026-04-24 15:57:56.001103+00
92	106	M	Navy/White	0	HCM-MAS-M	2026-05-10 12:20:04.775029+00	2026-05-10 12:20:04.775029+00
93	106	L	Navy/White	0	HCM-MAS-L	2026-05-10 12:20:04.912214+00	2026-05-10 12:20:04.912214+00
94	106	XL	Navy/White	0	HCM-MAS-XL	2026-05-10 12:20:05.015112+00	2026-05-10 12:20:05.015112+00
95	107	9	White	0	AF1-WHT-09	2026-05-10 15:37:07.779733+00	2026-05-10 15:37:07.779733+00
96	107	10	White	0	AF1-WHT-10	2026-05-10 15:37:07.897239+00	2026-05-10 15:37:07.897239+00
29	21	41-46	Đỏ/Đen	4	GIAY021	2026-04-04 09:21:39.88407+00	2026-04-04 09:21:39.88407+00
102	66	M	Xanh Navy	18	PK022	2026-05-11 11:26:13.467389+00	2026-05-11 11:26:13.467389+00
101	71	123	red	119	sdasdasd	2026-05-11 11:26:13.160383+00	2026-05-11 11:26:13.160383+00
89	105	M	Navy/White	0	HCM-POLO-NAVY-M	2026-05-10 12:15:56.431949+00	2026-05-10 12:15:56.431949+00
90	105	L	Navy/White	0	HCM-POLO-NAVY-L	2026-05-10 12:15:56.581258+00	2026-05-10 12:15:56.581258+00
91	105	XL	Navy/White	0	HCM-POLO-NAVY-XL	2026-05-10 12:15:56.683003+00	2026-05-10 12:15:56.683003+00
97	107	11	White	0	AF1-WHT-11	2026-05-10 15:37:07.996292+00	2026-05-10 15:37:07.996292+00
99	78	10	Red	2	21adsa	2026-05-11 11:26:12.53842+00	2026-05-11 11:26:12.53842+00
100	77	123	red	116	dasdad	2026-05-11 11:26:12.854586+00	2026-05-11 11:26:12.854586+00
103	65	M	Đen	20	PK021	2026-05-11 11:26:13.774871+00	2026-05-11 11:26:13.774871+00
104	64	S/M	Đen	28	PK020	2026-05-11 11:26:14.082024+00	2026-05-11 11:26:14.082024+00
105	63	S/M	Be	30	PK019	2026-05-11 11:26:14.388217+00	2026-05-11 11:26:14.388217+00
139	62	65cm	Tím	15	PK018	2026-05-11 11:30:49.855158+00	2026-05-11 11:30:49.855158+00
140	61	2.8m	Hồng	35	PK017	2026-05-11 11:30:50.161944+00	2026-05-11 11:30:50.161944+00
141	60	M	Xanh	50	PK016	2026-05-11 11:30:50.472147+00	2026-05-11 11:30:50.472147+00
142	59	39-45	Trắng	42	PK015	2026-05-11 11:30:50.776459+00	2026-05-11 11:30:50.776459+00
143	58	39-45	Đen	31	PK014	2026-05-11 11:30:51.073924+00	2026-05-11 11:30:51.073924+00
144	57	M	Navy	22	PK013	2026-05-11 11:30:51.362188+00	2026-05-11 11:30:51.362188+00
145	56	M	Đen	25	PK012	2026-05-11 11:30:51.652218+00	2026-05-11 11:30:51.652218+00
146	55	600ml	Đen	38	PK011	2026-05-11 11:30:51.948742+00	2026-05-11 11:30:51.948742+00
147	54	750ml	Xanh	40	PK010	2026-05-11 11:30:52.2879+00	2026-05-11 11:30:52.2879+00
148	53	5cm	Trắng	55	PK009	2026-05-11 11:30:52.596081+00	2026-05-11 11:30:52.596081+00
149	52	5cm	Be	60	PK008	2026-05-11 11:30:52.905652+00	2026-05-11 11:30:52.905652+00
150	51	S/M/L	Đen/Đỏ	25	PK007	2026-05-11 11:30:53.209514+00	2026-05-11 11:30:53.209514+00
151	50	S/M/L	Đen	35	PK006	2026-05-11 11:30:53.507883+00	2026-05-11 11:30:53.507883+00
152	49	M	Navy	28	PK005	2026-05-11 11:30:53.8023+00	2026-05-11 11:30:53.8023+00
153	48	M	Đen	30	PK004	2026-05-11 11:30:54.093594+00	2026-05-11 11:30:54.093594+00
154	47	M	Đen	17	PK003	2026-05-11 11:30:54.439744+00	2026-05-11 11:30:54.439744+00
155	46	M	Xám	18	PK002	2026-05-11 11:30:54.759055+00	2026-05-11 11:30:54.759055+00
156	45	M	Đen	8	PK001	2026-05-11 11:30:55.068203+00	2026-05-11 11:30:55.068203+00
157	44	S-XXL	Đỏ	15	AOO022	2026-05-11 11:30:55.374937+00	2026-05-11 11:30:55.374937+00
160	108	42	White/Blue	0	TD-CANV-42	2026-05-11 14:57:52.792252+00	2026-05-11 14:57:52.792252+00
163	10	38-45	Xám	5	GIAY010	2026-05-12 17:15:42.270247+00	2026-05-12 17:15:42.270247+00
164	10	38-44	Đen	1	GIAY023	2026-05-12 17:15:42.422109+00	2026-05-12 17:15:42.422109+00
165	7	8	White	0	PUMA-SMASH-WHT-08	2026-05-12 17:31:54.963504+00	2026-05-12 17:31:54.963504+00
166	7	9	White	0	PUMA-SMASH-WHT-09	2026-05-12 17:31:55.071952+00	2026-05-12 17:31:55.071952+00
167	7	10	White	0	PUMA-SMASH-WHT-10	2026-05-12 17:31:55.226675+00	2026-05-12 17:31:55.226675+00
\.


--
-- Data for Name: sportitems; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sportitems (id, category_id, name, cost_price, selling_price, stock_quantity, low_stock_threshold, image_urls, description) FROM stdin;
2	1	Nike Air Max 270	54.00	94.00	1	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1600185365926-3a2ce3cdb9eb}	\N
4	1	Nike Zoom Fly 5	60.00	107.60	6	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1605348532760-6753d2c43329}	\N
5	1	Nike Revolution 6	30.00	54.00	20	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1539185441755-769473a23570,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
19	1	Vans Sk8-Hi	36.00	66.00	12	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1551107696-a4b0c5a0d9a2}	\N
61	3	Dây Nhảy Be Fit	1.20	2.40	35	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Dây nhảy thể thao Be Fit thiết kế bền bỉ, hỗ trợ tập luyện cardio hiệu quả. Sản phẩm phù hợp cho việc rèn luyện sức bền tại nhà hoặc phòng gym.
54	3	Bình Nước Nike 750ml	2.20	4.40	40	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1523362628745-0c100150b504,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Bình nước thể thao Nike dung tích 750ml với thiết kế bền bỉ, tiện lợi. Phù hợp để bù nước trong quá trình tập luyện cường độ cao.
52	3	Băng Cuộn Y Band	0.60	1.20	60	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576091160550-2173dba999ef,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Băng dán cơ thể thao Y-Band hỗ trợ cố định cơ bắp và giảm chấn thương khi vận động. Chất liệu co giãn, thoáng khí, đảm bảo độ bám dính tốt trên da.
86	18	Nike Air Zoom Pegasus 40	0.00	119.99	0	10	{}	Lightweight daily training shoe with React foam midsole.
87	18	Adidas Ultraboost 23	0.00	179.99	0	8	{}	Responsive Boost cushioning for long-distance runs.
88	19	Puma RS-X Reinvention	0.00	89.99	0	12	{}	Retro-inspired chunky sneaker with RS technology.
89	20	Nike Pro Compression Tee	0.00	39.99	0	20	{}	Dri-FIT tight-fit training top for men.
51	3	Găng Tay Gym Adidas	2.80	5.60	25	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1598289431512-b97b0917affc,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Găng tay tập gym Adidas hỗ trợ bảo vệ lòng bàn tay hiệu quả trong quá trình luyện tập. Thiết kế ôm sát, thoáng khí giúp tăng độ bám và sự thoải mái cho người sử dụng.
90	20	Adidas Techfit Tank	0.00	29.99	0	15	{}	Moisture-wicking sleeveless training tank.
91	20	Under Armour HeatGear Tee	0.00	34.99	0	10	{}	Ultra-lightweight heat-management fabric.
92	21	Nike Dri-FIT Shorts 5in	0.00	44.99	0	20	{}	Lightweight running shorts with liner.
93	21	Adidas Tiro 23 Pants	0.00	59.99	0	10	{}	Classic football training pants with taper fit.
94	21	Puma Run Favourite Leggings	0.00	49.99	0	8	{}	High-waist compression leggings for running.
95	22	Nike Vapor Glove 5	0.00	49.99	0	5	{}	Minimal road racing gloves with natural motion fit.
96	22	Adidas Terrex Gloves	0.00	44.99	0	5	{}	Windproof gloves designed for trail running.
97	23	Nike Elite Crew Socks	0.00	14.99	0	30	{}	Cushioned running socks with targeted support zones.
98	23	Adidas No-Show Socks 3-Pack	0.00	12.99	0	25	{}	Low-cut performance socks, 3 pairs included.
99	23	Nike Sport Band Headband	0.00	9.99	0	15	{}	Elastic moisture-wicking headband for training.
100	24	Puma Gym Duffle Bag	0.00	54.99	0	5	{}	Spacious gym bag with shoe compartment.
101	24	Adidas Stadium Backpack	0.00	69.99	0	5	{}	Large-volume training backpack with laptop sleeve.
102	18	Nike React Infinity Run 4	0.00	159.99	0	6	{}	Injury-reduction running shoe with wider base.
103	18	Under Armour HOVR Phantom 3	0.00	139.99	0	5	{}	Connected running shoe with MapMyRun integration.
104	20	Nike Dri-FIT ADV Polo	0.00	79.99	0	8	{}	Tour-ready polo with premium moisture management.
10	1	Adidas Lite Racer 3.0	34.00	62.00	6	5	{https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a}	
77	3	aaaa	123.00	123.00	116	123	{"/Users/mac/Library/Application Support/MyShop/Images/36561a69-bbc8-4b12-91d0-d2d83bba6dc7.png","/Users/mac/Library/Application Support/MyShop/Images/f414898b-6102-42ff-9468-a2a4efee140f.png","/Users/mac/Library/Application Support/MyShop/Images/ae7a5404-a1b1-4993-8327-58a9251b9f66.png"}	Phụ kiện thể thao đa năng hỗ trợ tối ưu cho các hoạt động tập luyện. Sản phẩm có thiết kế bền bỉ và chất liệu cao cấp giúp nâng cao hiệu suất người dùng.
7	19	Puma Smash v2 Perforated Leather Sneakers	42.00	65.00	0	10	{https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1608231387042-66d1773070a5}	Experience timeless style with these clean, minimalist Puma sneakers featuring premium perforated leather details. Perfect for everyday wear, they offer superior comfort and a versatile look that pairs effortlessly with any casual outfit.
6	1	Adidas Ultraboost 22	72.00	126.00	0	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1556906781-9a412961c28c}	\N
14	1	New Balance 574	56.00	98.00	9	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
53	3	Băng Cuộn Kintex 5cm	0.48	1.00	55	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576671081837-49000212a370,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Băng cuộn Kintex 5cm hỗ trợ cơ bắp và khớp trong các hoạt động thể thao. Sản phẩm giúp giảm đau, hạn chế chấn thương và tăng cường khả năng vận động.
9	1	Adidas Stan Smith	52.00	90.00	12	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1606889464198-fcb18894cf50}	\N
71	3	Vòng tay thể thao	123.00	123.00	119	123	{"/Users/mac/Library/Application Support/MyShop/Images/d7472ad1-c334-49ef-a220-9fd2fd993fe5.png"}	Vòng tay thể thao hỗ trợ theo dõi chỉ số vận động hàng ngày. Thiết kế bền bỉ, thoáng khí, phù hợp cho nhiều môn thể thao.
21	1	Anta KT9 Basketball	68.00	119.60	4	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1575537302964-96cd47c06b1b}	\N
49	3	Túi Đeo Chéo Adidas	3.00	6.00	28	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1591561954557-26941169b49e,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Túi đeo chéo Adidas thiết kế nhỏ gọn, bền bỉ và tiện lợi. Phù hợp để đựng các vật dụng cá nhân nhỏ khi tham gia các hoạt động thể thao hoặc dạo phố.
55	3	Bình Nước Adidas 600ml	2.00	4.00	38	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1602143407151-11115cd0a7c8,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Bình nước thể thao Adidas dung tích 600ml. Thiết kế gọn nhẹ, tiện lợi cho việc tập luyện và mang theo hàng ngày.
78	2	hehe	20.00	50.00	2	3	{}	This sports garment is designed for optimal performance and comfort during exercise. It features a lightweight, breathable fabric that ensures mobility.
15	1	New Balance 327	64.00	112.00	7	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
60	3	Túi Lưới Nike	1.80	3.60	50	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1591561954557-26941169b49e,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Túi lưới Nike thiết kế gọn nhẹ, thuận tiện đựng đồ tập thể thao. Chất liệu lưới thoáng khí giúp bảo quản vật dụng cá nhân khô ráo và sạch sẽ.
58	3	Vớ Thể Thao Nike 3 Đôi	4.80	8.80	31	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1586350977771-b3b0abd50c82,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Bộ 3 đôi vớ thể thao Nike chất lượng cao. Thiết kế thoáng khí, hỗ trợ đệm êm ái cho bàn chân khi vận động.
79	1	Nike Air Force 1 Low 'Triple White'	75.00	115.00	30	10	{"C:\\\\Users\\\\lamhu\\\\AppData\\\\Roaming\\\\MyShop\\\\Images\\\\5e4d7729-7436-488f-91d3-4e1d9421511e.jpg"}	Step into timeless style with these iconic crisp white sneakers that offer unparalleled versatility for any wardrobe. Featuring a durable leather construction and signature Nike Air cushioning, these shoes provide all-day comfort without sacrificing your sleek aesthetic.
46	3	Balo Adidas Classic 3S	6.40	12.80	18	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	The Adidas Classic 3S backpack features a durable construction with the iconic three-stripe design. It offers spacious storage for daily essentials and an ergonomic build for comfortable carrying.
105	2	HCMUS Mascot Limited Edition Sports Polo	0.00	35.00	0	10	{"C:\\\\Users\\\\lamhu\\\\AppData\\\\Roaming\\\\MyShop\\\\Images\\\\80d41980-7fac-4e52-9fee-f9b98d15c54b.jpg"}	Show your school pride with this stylish and comfortable HCMUS mascot sports polo. Crafted from breathable, high-performance fabric, it is designed to keep you cool during campus activities or casual weekend matches.
108	1	Thuong Dinh Classic Canvas Trainer	0.00	25.00	0	10	{"https://qijxiegqhgkipqrmwupg.supabase.co/storage/v1/object/public/sport image/a4e55002-52fb-4f21-bc38-072c499c85fc_giay_thuong_dinh.jpg"}	Experience timeless Vietnamese craftsmanship with these lightweight canvas trainers. Perfect for casual wear or light physical activity, they feature a durable rubber gum sole for excellent traction.
11	1	Puma RS-X	48.00	84.00	10	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1551107696-a4b0c5a0d9a2}	\N
12	1	Puma Suede Classic	38.00	70.00	13	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
13	1	Puma Future Rider	44.00	78.00	16	5	{https://images.unsplash.com/photo-1463100099107-aa0980c362e6,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
80	2	HCMUS Mascot Limited Edition Sport Tee	16.00	25.00	29	10	{"C:\\\\Users\\\\lamhu\\\\AppData\\\\Roaming\\\\MyShop\\\\Images\\\\20ba0e0e-f29b-4d55-b026-0345ce02f335.jpg"}	Show your school spirit with this exclusive HCMUS mascot performance t-shirt. Designed with lightweight, breathable fabric, it keeps you cool during intense training sessions or casual campus activities.
106	20	HCMUS Mascot Limited Edition Graphic Tee	0.00	25.00	0	10	{"https://qijxiegqhgkipqrmwupg.supabase.co/storage/v1/object/public/sport image/b62be967-731b-4974-a6bf-39e07396fd3d.jpg"}	Show your school spirit with this exclusive HCMUS mascot graphic tee. Crafted from premium breathable cotton, this shirt offers superior comfort for campus life or casual training sessions.
109	25	Nike Air Zoom Pegasus 39	0.00	120.00	0	5	{https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800&q=80,https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=800&q=80}	Professional running shoes with responsive cushioning.
110	25	Adidas Ultraboost 22	0.00	180.00	0	5	{https://images.unsplash.com/photo-1518002171953-a080ee817e1f?w=800&q=80,https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800&q=80}	High-performance running shoes featuring Boost technology.
111	26	Wilson Evolution Basketball	0.00	59.99	0	10	{https://images.unsplash.com/photo-1519861531473-9200262188bf?w=800&q=80,https://images.unsplash.com/photo-1546519638-68e109498ffc?w=800&q=80}	The #1 indoor game basketball in America.
112	23	Premium Yoga Mat (6mm)	0.00	25.50	0	20	{https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=800&q=80,https://images.unsplash.com/photo-1592432678016-e910b452f9a2?w=800&q=80}	Extra thick non-slip yoga mat for all types of yoga.
113	26	Wilson Pro Staff 97 V13	0.00	249.00	0	3	{https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?w=800&q=80,https://images.unsplash.com/photo-1595435934249-5df7ed86e1c0?w=800&q=80}	Precision tennis racket designed for advanced players.
114	23	Speedo Vanquisher 2.0	0.00	19.99	0	15	{https://images.unsplash.com/photo-1557800636-894a64c1696f?w=800&q=80,https://images.unsplash.com/photo-1530549387789-4c1017266635?w=800&q=80}	Low profile competition goggles with panoramic view.
115	27	UA Tech 2.0 Training Tee	0.00	29.99	0	50	{https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=800&q=80,https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?w=800&q=80}	UA Tech fabric is quick-drying, ultra-soft & has a more natural feel.
116	26	Everlast Pro Style Gloves	0.00	44.95	0	8	{https://images.unsplash.com/photo-1549719386-74dfcbf7dbed?w=800&q=80,https://images.unsplash.com/photo-1549719386-74dfcbf7dbed?w=800&q=75}	Premium synthetic leather along with superior construction increases durability.
117	23	Garmin Forerunner 245	0.00	299.99	0	2	{https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=800&q=80,https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=800&q=80}	GPS running smartwatch with advanced training features.
118	26	Spalding NBA Street Ball	0.00	19.99	0	30	{https://images.unsplash.com/photo-1515523110800-9415d13b84a8?w=800&q=80,https://images.unsplash.com/photo-1518459031867-a89b944bffe4?w=800&q=80}	Performance outdoor cover with deep channel design for grip.
31	2	Quần Jogger Puma Core	9.60	19.20	19	8	{https://images.unsplash.com/photo-1506629082955-511b1aa562c8,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
32	2	Quần Short Nike Flex	8.80	16.80	32	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1591195853828-11db59a44f6b}	\N
3	1	Nike Pegasus 40	44.00	78.00	6	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa}	Elevate your daily training with the Nike Pegasus 40, a masterclass in responsive performance and sleek versatility. Designed for runners who demand consistency, this silhouette features a precision-engineered mesh upper for maximum breathability and a supportive, locked-in fit that moves seamlessly with your stride.\n\nThe refined midsole construction delivers an energetic, springy ride, while the durable traction pattern provides confident grip across various surfaces. Finished in a sophisticated deep navy, these shoes blend high-end athletic utility with a modern aesthetic, making them as effective on the track as they are on the street.
107	19	Classic Air Force 1 Triple White Low-Top Sneakers	0.00	110.00	0	10	{"https://qijxiegqhgkipqrmwupg.supabase.co/storage/v1/object/public/sport image/765c6544-3652-4f88-ab94-156b532a104e_af1.jpg"}	Elevate your daily rotation with the iconic, all-white aesthetic of these classic low-top sneakers. Featuring premium construction and legendary cushioned comfort, they provide a timeless look that pairs perfectly with any outfit from gym gear to street style.
33	2	Quần Short Adidas Linear	8.00	15.60	28	8	{https://images.unsplash.com/photo-1434682772747-f16d3ea162c3,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
34	2	Quần Short Uniqlo Dry	7.60	14.80	25	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1566241440091-ec10de8db2e1}	\N
35	2	Bộ Đồ Thể Thao Nam Nike	18.00	34.00	12	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
16	1	Converse Chuck Taylor	32.00	60.00	22	5	{https://images.unsplash.com/photo-1463100099107-aa0980c362e6,https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
64	3	Băng Đỡ Đầu Gối	1.20	2.40	28	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1576671081837-49000212a370,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Băng đỡ đầu gối giúp bảo vệ khớp và giảm chấn thương khi vận động mạnh. Thiết kế co giãn, thoáng khí, phù hợp cho nhiều hoạt động thể thao.
44	2	Áo Jersey Manchester United	12.00	23.20	15	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1574629810360-7efbbe195018}	Manchester United official team jersey. Designed for comfort and durability during athletic activities.
17	1	Converse Run Star Hike	52.00	92.00	8	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1607522370275-f14206abe5d3}	\N
18	1	Vans Old Skool	34.00	62.00	17	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
20	1	Vans Slip-On	28.00	52.00	19	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1560769629-975ec94e6a86}	\N
22	1	Li-Ning Way of Wade 10	76.00	134.00	4	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772,https://images.unsplash.com/photo-1579338559194-a162d19bf842}	\N
27	2	Áo Polo Adidas Climalite	8.00	15.60	22	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf}	\N
28	2	Áo Polo Lacoste Sport	10.00	19.20	18	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1618354691373-d851c5c3a990}	\N
29	2	Quần Jogger Nike Sportswear	11.20	22.00	11	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1552902865-b72c031ac5ea,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
30	2	Quần Jogger Adidas Essential	10.40	20.80	23	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1594938298603-c8148c4dae35}	\N
37	2	Bộ Đồ Thể Thao Nữ Nike	16.80	32.00	16	5	{https://images.unsplash.com/photo-1485965120184-e220f721d03e,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
38	2	Bộ Đồ Thể Thao Nữ Adidas	16.00	31.20	13	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1518310383802-640c2de311b2,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
39	2	Áo Khoác Nike Windrunner	15.20	28.80	10	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68,https://images.unsplash.com/photo-1591047139829-d91aecb6caea}	\N
40	2	Áo Khoác Adidas SST	14.00	27.20	11	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1551028719-00167b16eac5,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
42	2	Quần Legging Nike Power	10.00	19.20	22	8	{https://images.unsplash.com/photo-1506629082955-511b1aa562c8,https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
43	2	Áo Jersey Real Madrid	12.00	23.20	15	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1551958219-acbc608c6377,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
41	2	Áo Tank Nike Pro	6.00	11.60	40	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
1	1	Nike Air Max 90	48.00	86.00	10	5	{https://images.unsplash.com/photo-1491553895911-0055eca6402d,https://images.unsplash.com/photo-1542291026-7eec264c27ff,https://images.unsplash.com/photo-1549298916-b41d501d3772}	\N
23	2	Áo Thun Nike Dri-FIT	7.20	14.00	88	8	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
66	3	Túi Chườm Đá Thể Thao	1.80	3.60	18	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Túi chườm đá chuyên dụng dùng để giảm sưng và đau sau khi vận động thể thao. Sản phẩm có khả năng giữ lạnh tốt, thiết kế nhỏ gọn và dễ dàng sử dụng tại chỗ.
65	3	Khay Đựng Phụ Kiện	1.40	2.80	20	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Khay đựng phụ kiện thể thao giúp sắp xếp gọn gàng các vật dụng cá nhân và linh kiện nhỏ. Thiết kế bền bỉ, tiện lợi cho việc lưu trữ tại phòng tập hoặc tại nhà.
63	3	Băng Đỡ Cổ Chân	1.00	2.00	30	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Băng bảo vệ cổ chân giúp hỗ trợ ổn định khớp và giảm chấn thương khi vận động. Thiết kế co giãn, thấm hút mồ hôi tốt, phù hợp cho các hoạt động thể thao.
36	2	Bộ Đồ Thể Thao Nam Adidas	16.80	32.80	7	5	{https://images.unsplash.com/photo-1515886657613-9f3515b0c78f,https://images.unsplash.com/photo-1521572163474-6864f9cf17ab,https://images.unsplash.com/photo-1556821840-3a63f95609a7,https://images.unsplash.com/photo-1562157873-818bc0726f68}	\N
59	3	Vớ Thể Thao Adidas 3 Đôi	4.40	8.00	42	10	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1582576163090-09d3b6f8a969,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Pack of 3 Adidas athletic socks. Designed for comfort and durability during sports activities.
57	3	Nón Adidas Golf Cap	3.40	6.80	22	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1534215754734-18e55d13e346,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	This Adidas golf cap is designed for performance on the course. It features moisture-wicking fabric and an adjustable strap for a secure, comfortable fit.
56	3	Nón Nike Dri-FIT Cap	3.60	7.20	25	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1588850561407-ed78c282e89b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Nike Dri-FIT cap designed with moisture-wicking fabric to keep you cool and dry. Features an adjustable strap for a secure, comfortable fit. Ideal for sports and outdoor activities.
50	3	Găng Tay Chạy Bộ Nike	2.40	4.80	35	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Nike running gloves designed for optimal comfort and grip during workouts. Features breathable fabric to keep hands dry and warm in cool conditions.
48	3	Túi Đeo Chéo Nike	3.20	6.40	30	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1548036328-c9fa89d128fa,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Nike crossbody bag featuring a durable design and adjustable strap. Perfect for carrying daily essentials securely during sports or casual outings.
62	3	Bóng Yoga 65cm	3.20	6.40	15	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1544367567-0f2fcb009e0b,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Bóng tập Yoga đường kính 65cm, hỗ trợ các bài tập cân bằng và giãn cơ. Sản phẩm được làm từ chất liệu cao su bền bỉ, chịu lực tốt và chống trượt.
47	3	Balo Puma Backpack	5.60	11.20	17	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1581605405669-fcdf81165afa,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	This Puma backpack features a durable construction suitable for daily use or sports activities. It offers a spacious main compartment and adjustable straps for comfortable carrying.
45	3	Balo Nike Brasilia	7.20	14.00	8	5	{https://images.unsplash.com/photo-1517836357463-d25dfeac3438,https://images.unsplash.com/photo-1553062407-98eeb64c6a62,https://images.unsplash.com/photo-1622560480654-d96214fdc887}	Nike Brasilia backpack designed for durable storage. Features multiple compartments to organize your gear securely. Ideal for daily commutes or gym sessions.
\.


--
-- Data for Name: suppliers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.suppliers (id, name, contact_phone, supplier_type) FROM stdin;
1	Công ty Động Lực	0243123456	Hàng công ty
2	Kho Sỉ Tân Bình	0909123456	Hàng nhập sỉ
3	Xách tay JP	0988777666	Hàng xách tay
4	Tổng công ty HB	0943802900	HIHI
5	toi la hihi	12334344	huhu
\.


--
-- Data for Name: supplydetails; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.supplydetails (id, supply_id, item_id, quantity, import_price, variant_id) FROM stdin;
1	2	10	10	34.00	\N
\.


--
-- Data for Name: supplyorders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.supplyorders (id, supplier_id, import_date, total_cost) FROM stdin;
1	1	2026-03-15 14:15:28.065066+00	10000000.00
2	1	2026-04-26 13:20:24.599853+00	340.00
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (id, email, password, role, created_at) FROM stdin;
2	admin@prosport.com	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	owner	2026-04-09 15:24:18.391736+00
3	sale@prosport.com	7r/fOZ2o8rtH0kJ0m/uljUWpSeKx6HLeaW63QPALmmY=	sale	2026-04-09 15:36:22.452897+00
4	tkinculi@gmail.com	/5ZnMgXcciMgWY6/j4gyWyrFaSLVohZLV2WGgnS8DXM=	sale	2026-05-09 04:36:04.073344+00
5	sale1@prosport.com	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	sale	2026-05-10 08:22:36.490454+00
\.


--
-- Data for Name: schema_migrations; Type: TABLE DATA; Schema: realtime; Owner: -
--

COPY realtime.schema_migrations (version, inserted_at) FROM stdin;
20211116024918	2026-03-15 13:23:15
20211116045059	2026-03-15 13:23:16
20211116050929	2026-03-15 13:23:16
20211116051442	2026-03-15 13:23:17
20211116212300	2026-03-15 13:23:18
20211116213355	2026-03-15 13:23:18
20211116213934	2026-03-15 13:23:19
20211116214523	2026-03-15 13:23:20
20211122062447	2026-03-15 13:23:21
20211124070109	2026-03-15 13:23:21
20211202204204	2026-03-15 13:23:22
20211202204605	2026-03-15 13:23:23
20211210212804	2026-03-15 13:23:25
20211228014915	2026-03-15 13:23:26
20220107221237	2026-03-15 13:23:26
20220228202821	2026-03-15 13:23:27
20220312004840	2026-03-15 13:23:28
20220603231003	2026-03-15 13:23:29
20220603232444	2026-03-15 13:23:29
20220615214548	2026-03-15 13:23:30
20220712093339	2026-03-15 13:23:31
20220908172859	2026-03-15 13:23:31
20220916233421	2026-03-15 13:23:32
20230119133233	2026-03-15 13:23:32
20230128025114	2026-03-15 13:23:33
20230128025212	2026-03-15 13:23:34
20230227211149	2026-03-15 13:23:34
20230228184745	2026-03-15 13:23:35
20230308225145	2026-03-15 13:23:36
20230328144023	2026-03-15 13:23:36
20231018144023	2026-03-15 13:23:37
20231204144023	2026-03-15 13:23:38
20231204144024	2026-03-15 13:23:39
20231204144025	2026-03-15 13:23:39
20240108234812	2026-03-15 13:23:40
20240109165339	2026-03-15 13:23:41
20240227174441	2026-03-15 13:23:42
20240311171622	2026-03-15 13:23:43
20240321100241	2026-03-15 13:23:44
20240401105812	2026-03-15 13:23:46
20240418121054	2026-03-15 13:23:47
20240523004032	2026-03-15 13:23:49
20240618124746	2026-03-15 13:23:49
20240801235015	2026-03-15 13:23:50
20240805133720	2026-03-15 13:23:51
20240827160934	2026-03-15 13:23:51
20240919163303	2026-03-15 13:23:52
20240919163305	2026-03-15 13:23:53
20241019105805	2026-03-15 13:23:53
20241030150047	2026-03-15 13:23:56
20241108114728	2026-03-15 13:23:57
20241121104152	2026-03-15 13:23:57
20241130184212	2026-03-15 13:23:58
20241220035512	2026-03-15 13:23:58
20241220123912	2026-03-15 13:23:59
20241224161212	2026-03-15 13:24:00
20250107150512	2026-03-15 13:24:00
20250110162412	2026-03-15 13:24:01
20250123174212	2026-03-15 13:24:02
20250128220012	2026-03-15 13:24:02
20250506224012	2026-03-15 13:24:03
20250523164012	2026-03-15 13:24:03
20250714121412	2026-03-15 13:24:04
20250905041441	2026-03-15 13:24:05
20251103001201	2026-03-15 13:24:05
20251120212548	2026-03-15 13:24:06
20251120215549	2026-03-15 13:24:07
20260218120000	2026-03-15 13:24:07
20260326120000	2026-04-13 05:51:41
\.


--
-- Data for Name: subscription; Type: TABLE DATA; Schema: realtime; Owner: -
--

COPY realtime.subscription (id, subscription_id, entity, filters, claims, created_at, action_filter) FROM stdin;
\.


--
-- Data for Name: buckets; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.buckets (id, name, owner, created_at, updated_at, public, avif_autodetection, file_size_limit, allowed_mime_types, owner_id, type) FROM stdin;
sport image	sport image	\N	2026-03-28 15:59:51.116017+00	2026-03-28 15:59:51.116017+00	t	f	10485760	{image/*}	\N	STANDARD
\.


--
-- Data for Name: buckets_analytics; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.buckets_analytics (name, type, format, created_at, updated_at, id, deleted_at) FROM stdin;
\.


--
-- Data for Name: buckets_vectors; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.buckets_vectors (id, type, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: migrations; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.migrations (id, name, hash, executed_at) FROM stdin;
0	create-migrations-table	e18db593bcde2aca2a408c4d1100f6abba2195df	2026-03-15 11:33:26.727989
1	initialmigration	6ab16121fbaa08bbd11b712d05f358f9b555d777	2026-03-15 11:33:26.767311
2	storage-schema	f6a1fa2c93cbcd16d4e487b362e45fca157a8dbd	2026-03-15 11:33:26.77027
3	pathtoken-column	2cb1b0004b817b29d5b0a971af16bafeede4b70d	2026-03-15 11:33:26.796581
4	add-migrations-rls	427c5b63fe1c5937495d9c635c263ee7a5905058	2026-03-15 11:33:26.842008
5	add-size-functions	79e081a1455b63666c1294a440f8ad4b1e6a7f84	2026-03-15 11:33:26.846158
6	change-column-name-in-get-size	ded78e2f1b5d7e616117897e6443a925965b30d2	2026-03-15 11:33:26.850946
7	add-rls-to-buckets	e7e7f86adbc51049f341dfe8d30256c1abca17aa	2026-03-15 11:33:26.855408
8	add-public-to-buckets	fd670db39ed65f9d08b01db09d6202503ca2bab3	2026-03-15 11:33:26.858452
9	fix-search-function	af597a1b590c70519b464a4ab3be54490712796b	2026-03-15 11:33:26.861727
10	search-files-search-function	b595f05e92f7e91211af1bbfe9c6a13bb3391e16	2026-03-15 11:33:26.865236
11	add-trigger-to-auto-update-updated_at-column	7425bdb14366d1739fa8a18c83100636d74dcaa2	2026-03-15 11:33:26.868615
12	add-automatic-avif-detection-flag	8e92e1266eb29518b6a4c5313ab8f29dd0d08df9	2026-03-15 11:33:26.872371
13	add-bucket-custom-limits	cce962054138135cd9a8c4bcd531598684b25e7d	2026-03-15 11:33:26.875673
14	use-bytes-for-max-size	941c41b346f9802b411f06f30e972ad4744dad27	2026-03-15 11:33:26.879142
15	add-can-insert-object-function	934146bc38ead475f4ef4b555c524ee5d66799e5	2026-03-15 11:33:26.910336
16	add-version	76debf38d3fd07dcfc747ca49096457d95b1221b	2026-03-15 11:33:26.914
17	drop-owner-foreign-key	f1cbb288f1b7a4c1eb8c38504b80ae2a0153d101	2026-03-15 11:33:26.917661
18	add_owner_id_column_deprecate_owner	e7a511b379110b08e2f214be852c35414749fe66	2026-03-15 11:33:26.920908
19	alter-default-value-objects-id	02e5e22a78626187e00d173dc45f58fa66a4f043	2026-03-15 11:33:26.926356
20	list-objects-with-delimiter	cd694ae708e51ba82bf012bba00caf4f3b6393b7	2026-03-15 11:33:26.929689
21	s3-multipart-uploads	8c804d4a566c40cd1e4cc5b3725a664a9303657f	2026-03-15 11:33:26.934114
22	s3-multipart-uploads-big-ints	9737dc258d2397953c9953d9b86920b8be0cdb73	2026-03-15 11:33:26.945308
23	optimize-search-function	9d7e604cddc4b56a5422dc68c9313f4a1b6f132c	2026-03-15 11:33:26.954716
24	operation-function	8312e37c2bf9e76bbe841aa5fda889206d2bf8aa	2026-03-15 11:33:26.958307
25	custom-metadata	d974c6057c3db1c1f847afa0e291e6165693b990	2026-03-15 11:33:26.961903
26	objects-prefixes	215cabcb7f78121892a5a2037a09fedf9a1ae322	2026-03-15 11:33:26.965454
27	search-v2	859ba38092ac96eb3964d83bf53ccc0b141663a6	2026-03-15 11:33:26.968523
28	object-bucket-name-sorting	c73a2b5b5d4041e39705814fd3a1b95502d38ce4	2026-03-15 11:33:26.971604
29	create-prefixes	ad2c1207f76703d11a9f9007f821620017a66c21	2026-03-15 11:33:26.974555
30	update-object-levels	2be814ff05c8252fdfdc7cfb4b7f5c7e17f0bed6	2026-03-15 11:33:26.977531
31	objects-level-index	b40367c14c3440ec75f19bbce2d71e914ddd3da0	2026-03-15 11:33:26.980452
32	backward-compatible-index-on-objects	e0c37182b0f7aee3efd823298fb3c76f1042c0f7	2026-03-15 11:33:26.983332
33	backward-compatible-index-on-prefixes	b480e99ed951e0900f033ec4eb34b5bdcb4e3d49	2026-03-15 11:33:26.986211
34	optimize-search-function-v1	ca80a3dc7bfef894df17108785ce29a7fc8ee456	2026-03-15 11:33:26.989122
35	add-insert-trigger-prefixes	458fe0ffd07ec53f5e3ce9df51bfdf4861929ccc	2026-03-15 11:33:26.991913
36	optimise-existing-functions	6ae5fca6af5c55abe95369cd4f93985d1814ca8f	2026-03-15 11:33:26.994755
37	add-bucket-name-length-trigger	3944135b4e3e8b22d6d4cbb568fe3b0b51df15c1	2026-03-15 11:33:26.997564
38	iceberg-catalog-flag-on-buckets	02716b81ceec9705aed84aa1501657095b32e5c5	2026-03-15 11:33:27.001475
39	add-search-v2-sort-support	6706c5f2928846abee18461279799ad12b279b78	2026-03-15 11:33:27.011511
40	fix-prefix-race-conditions-optimized	7ad69982ae2d372b21f48fc4829ae9752c518f6b	2026-03-15 11:33:27.014395
41	add-object-level-update-trigger	07fcf1a22165849b7a029deed059ffcde08d1ae0	2026-03-15 11:33:27.017363
42	rollback-prefix-triggers	771479077764adc09e2ea2043eb627503c034cd4	2026-03-15 11:33:27.020435
43	fix-object-level	84b35d6caca9d937478ad8a797491f38b8c2979f	2026-03-15 11:33:27.023379
44	vector-bucket-type	99c20c0ffd52bb1ff1f32fb992f3b351e3ef8fb3	2026-03-15 11:33:27.026217
45	vector-buckets	049e27196d77a7cb76497a85afae669d8b230953	2026-03-15 11:33:27.03038
46	buckets-objects-grants	fedeb96d60fefd8e02ab3ded9fbde05632f84aed	2026-03-15 11:33:27.041081
47	iceberg-table-metadata	649df56855c24d8b36dd4cc1aeb8251aa9ad42c2	2026-03-15 11:33:27.044645
48	iceberg-catalog-ids	e0e8b460c609b9999ccd0df9ad14294613eed939	2026-03-15 11:33:27.047864
49	buckets-objects-grants-postgres	072b1195d0d5a2f888af6b2302a1938dd94b8b3d	2026-03-15 11:33:27.064432
50	search-v2-optimised	6323ac4f850aa14e7387eb32102869578b5bd478	2026-03-15 11:33:27.069767
51	index-backward-compatible-search	2ee395d433f76e38bcd3856debaf6e0e5b674011	2026-03-15 11:33:27.087524
52	drop-not-used-indexes-and-functions	5cc44c8696749ac11dd0dc37f2a3802075f3a171	2026-03-15 11:33:27.089169
53	drop-index-lower-name	d0cb18777d9e2a98ebe0bc5cc7a42e57ebe41854	2026-03-15 11:33:27.098662
54	drop-index-object-level	6289e048b1472da17c31a7eba1ded625a6457e67	2026-03-15 11:33:27.101004
55	prevent-direct-deletes	262a4798d5e0f2e7c8970232e03ce8be695d5819	2026-03-15 11:33:27.102557
57	s3-multipart-uploads-metadata	f127886e00d1b374fadbc7c6b31e09336aad5287	2026-04-08 03:39:10.211724
58	operation-ergonomics	00ca5d483b3fe0d522133d9002ccc5df98365120	2026-04-08 03:39:10.243087
56	fix-optimized-search-function	b823ed1e418101032fa01374edc9a436e54e3ed4	2026-03-15 11:33:27.106847
59	drop-unused-functions	38456f13e39691c2bbb4b5151d0d1cdbabd4a8c4	2026-05-07 16:23:15.232193
60	optimize-existing-functions-again	db35e1c91a9201e59f4fef8d972c2f277d68b157	2026-05-07 16:23:15.24692
\.


--
-- Data for Name: objects; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.objects (id, bucket_id, name, owner, created_at, updated_at, last_accessed_at, metadata, version, owner_id, user_metadata) FROM stdin;
28b02fc0-06f4-463d-b72b-81341f6773be	sport image	.emptyFolderPlaceholder	\N	2026-04-01 04:09:24.499888+00	2026-04-01 04:09:24.499888+00	2026-04-01 04:09:24.499888+00	{"eTag": "\\"d41d8cd98f00b204e9800998ecf8427e\\"", "size": 0, "mimetype": "application/octet-stream", "cacheControl": "max-age=3600", "lastModified": "2026-04-01T04:09:24.503Z", "contentLength": 0, "httpStatusCode": 200}	9e1085c0-c707-4872-b5f2-638335bc9ca9	\N	{}
5c7be36c-40bf-4ca4-9f6d-97e489211a7f	sport image	ae5a0e0a-4142-456e-9300-be8a0f80bb1b_logo.png	5cb1a108-6128-4cec-9d49-24f5310a5ca9	2026-04-02 07:24:17.644093+00	2026-04-02 07:24:17.644093+00	2026-04-02 07:24:17.644093+00	{"eTag": "\\"803c1fc7d9606ff0c78cacbd52142898\\"", "size": 51764, "mimetype": "image/png", "cacheControl": "max-age=3600", "lastModified": "2026-04-02T07:24:18.000Z", "contentLength": 51764, "httpStatusCode": 200}	ed626153-ddc4-410a-bdde-a67317ee973b	5cb1a108-6128-4cec-9d49-24f5310a5ca9	{}
f7f4b503-34f0-4abe-8159-89f849af8f9e	sport image	b62be967-731b-4974-a6bf-39e07396fd3d.jpg	\N	2026-05-10 12:19:47.588648+00	2026-05-10 12:19:47.588648+00	2026-05-10 12:19:47.588648+00	{"eTag": "\\"2d36f2d8a11eb302e3f2ea85f86440dc\\"", "size": 25373, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-10T12:19:48.000Z", "contentLength": 25373, "httpStatusCode": 200}	9756fc84-3dd5-46da-aa88-b16a4fce96d8	\N	{}
4aaa31bf-f1c0-45f5-9e0e-8725acaad9e7	sport image	765c6544-3652-4f88-ab94-156b532a104e_af1.jpg	\N	2026-05-10 15:36:54.484825+00	2026-05-10 15:36:54.484825+00	2026-05-10 15:36:54.484825+00	{"eTag": "\\"f9c6289b5602cc20839fc0830e14a945\\"", "size": 55624, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-10T15:36:55.000Z", "contentLength": 55624, "httpStatusCode": 200}	a52ccb8e-7a0a-4029-8918-b8f3aeddb613	\N	{}
c4516621-f0d6-4a0f-b388-b20a4757aa7c	sport image	fe88a43e-4f1b-4686-b960-734502bccac3_af1.jpg	\N	2026-05-11 14:14:56.457449+00	2026-05-11 14:14:56.457449+00	2026-05-11 14:14:56.457449+00	{"eTag": "\\"f9c6289b5602cc20839fc0830e14a945\\"", "size": 55624, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T14:14:57.000Z", "contentLength": 55624, "httpStatusCode": 200}	5163f321-21f6-4799-a5ca-4a68edf7f834	\N	{}
a5ac6974-2013-4a52-930f-26c24fce398d	sport image	35d6551f-898c-45bc-9e9d-30cae66d5bb7_US_capy.jpg	\N	2026-05-11 14:38:27.736118+00	2026-05-11 14:38:27.736118+00	2026-05-11 14:38:27.736118+00	{"eTag": "\\"7f0e6c4b9b128b9e834437f28cb7a769\\"", "size": 23586, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T14:38:28.000Z", "contentLength": 23586, "httpStatusCode": 200}	ddca9f6e-ad1f-42e9-83a7-30664fcb5784	\N	{}
a5d71789-31f2-4482-974b-5d080d1922c6	sport image	92e959c6-822c-4df8-8570-5ba8e9d5ee54_giay_thuong_dinh.jpg	\N	2026-05-11 14:42:59.42986+00	2026-05-11 14:42:59.42986+00	2026-05-11 14:42:59.42986+00	{"eTag": "\\"f31f0e3bf832839f176814f0deb75312\\"", "size": 37010, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T14:43:00.000Z", "contentLength": 37010, "httpStatusCode": 200}	a0fca5ff-38ed-4d98-949d-cab5b59eb468	\N	{}
290769f4-d496-4259-9888-b7698c1180c7	sport image	a4e55002-52fb-4f21-bc38-072c499c85fc_giay_thuong_dinh.jpg	\N	2026-05-11 14:56:59.584746+00	2026-05-11 14:56:59.584746+00	2026-05-11 14:56:59.584746+00	{"eTag": "\\"f31f0e3bf832839f176814f0deb75312\\"", "size": 37010, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T14:57:00.000Z", "contentLength": 37010, "httpStatusCode": 200}	828bed98-8122-4800-aea2-5e10e0a8f308	\N	{}
958c04ff-9863-429e-bef0-8a2ca7680452	sport image	5b229e1e-d42e-4fe5-83a9-251628af26c0_jd1.jpg	\N	2026-05-11 15:01:55.876205+00	2026-05-11 15:01:55.876205+00	2026-05-11 15:01:55.876205+00	{"eTag": "\\"7eb34f84fac8b99ccf6bc967e19b278d\\"", "size": 5572, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:01:56.000Z", "contentLength": 5572, "httpStatusCode": 200}	d7706c33-acd0-4ff3-935f-78aa8a9d8431	\N	{}
c269f240-8083-42cb-806f-ae147720a764	sport image	3d30dae1-2d68-4362-8e25-4f47c052ab91_jd1.jpg	\N	2026-05-11 15:03:46.130103+00	2026-05-11 15:03:46.130103+00	2026-05-11 15:03:46.130103+00	{"eTag": "\\"7eb34f84fac8b99ccf6bc967e19b278d\\"", "size": 5572, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:03:47.000Z", "contentLength": 5572, "httpStatusCode": 200}	d141c1a9-651f-4692-a4c1-b7918ba11707	\N	{}
fb79c003-eebc-487c-848f-a3456d02b93f	sport image	efe5b98d-05af-406b-94f0-26be6f301678_jd1.jpg	\N	2026-05-11 15:09:41.285378+00	2026-05-11 15:09:41.285378+00	2026-05-11 15:09:41.285378+00	{"eTag": "\\"7eb34f84fac8b99ccf6bc967e19b278d\\"", "size": 5572, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:09:42.000Z", "contentLength": 5572, "httpStatusCode": 200}	d79112ca-b723-447c-97db-52086005751c	\N	{}
01682bf0-c42e-4237-afd5-793d49c09c17	sport image	514ed05b-d64e-4491-93ae-81bd533a386c_giay_thuong_dinh.jpg	\N	2026-05-11 15:09:46.511507+00	2026-05-11 15:09:46.511507+00	2026-05-11 15:09:46.511507+00	{"eTag": "\\"f31f0e3bf832839f176814f0deb75312\\"", "size": 37010, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:09:47.000Z", "contentLength": 37010, "httpStatusCode": 200}	4ce84552-e386-40a9-bb7f-ea64a7c92c9b	\N	{}
e4c1108f-45aa-4f5c-a229-60133a6669dd	sport image	b042a996-d705-418b-ac78-fa6b14c199ec_jd1.jpg	\N	2026-05-11 15:12:16.29144+00	2026-05-11 15:12:16.29144+00	2026-05-11 15:12:16.29144+00	{"eTag": "\\"7eb34f84fac8b99ccf6bc967e19b278d\\"", "size": 5572, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:12:17.000Z", "contentLength": 5572, "httpStatusCode": 200}	1bd15aec-c3bc-4c1c-b009-fcb234a7846c	\N	{}
82b369c0-21b4-4862-8cf3-5019bfb3e7f0	sport image	180b065f-9e93-4e89-b2ae-f2ffc376e38d_af1.jpg	\N	2026-05-11 15:12:29.56505+00	2026-05-11 15:12:29.56505+00	2026-05-11 15:12:29.56505+00	{"eTag": "\\"f9c6289b5602cc20839fc0830e14a945\\"", "size": 55624, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-11T15:12:30.000Z", "contentLength": 55624, "httpStatusCode": 200}	952adb5e-7e0b-4ce1-bc2f-bb1a9b51f28f	\N	{}
0127209e-fa5a-48fc-bbdc-35808d37bb1f	sport image	8056c5bc-4e8e-47f8-bd1a-c05281c64e5e_af1.jpg	\N	2026-05-12 05:26:43.070276+00	2026-05-12 05:26:43.070276+00	2026-05-12 05:26:43.070276+00	{"eTag": "\\"f9c6289b5602cc20839fc0830e14a945\\"", "size": 55624, "mimetype": "image/jpeg", "cacheControl": "no-cache", "lastModified": "2026-05-12T05:26:44.000Z", "contentLength": 55624, "httpStatusCode": 200}	4097b594-0dc8-4d56-a143-8dcc4ba42fb5	\N	{}
\.


--
-- Data for Name: s3_multipart_uploads; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.s3_multipart_uploads (id, in_progress_size, upload_signature, bucket_id, key, version, owner_id, created_at, user_metadata, metadata) FROM stdin;
\.


--
-- Data for Name: s3_multipart_uploads_parts; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.s3_multipart_uploads_parts (id, upload_id, size, part_number, bucket_id, key, etag, owner_id, version, created_at) FROM stdin;
\.


--
-- Data for Name: vector_indexes; Type: TABLE DATA; Schema: storage; Owner: -
--

COPY storage.vector_indexes (id, name, bucket_id, data_type, dimension, distance_metric, metadata_configuration, created_at, updated_at) FROM stdin;
\.


--
-- Data for Name: secrets; Type: TABLE DATA; Schema: vault; Owner: -
--

COPY vault.secrets (id, name, description, secret, key_id, nonce, created_at, updated_at) FROM stdin;
\.


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE SET; Schema: auth; Owner: -
--

SELECT pg_catalog.setval('auth.refresh_tokens_id_seq', 149, true);


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.categories_id_seq', 27, true);


--
-- Name: customerorders_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.customerorders_id_seq', 38, true);


--
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.customers_id_seq', 14, true);


--
-- Name: orderdetails_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.orderdetails_id_seq', 142, true);


--
-- Name: shifts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.shifts_id_seq', 2, true);


--
-- Name: sportitem_variants_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sportitem_variants_id_seq', 167, true);


--
-- Name: sportitems_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sportitems_id_seq', 118, true);


--
-- Name: suppliers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.suppliers_id_seq', 5, true);


--
-- Name: supplydetails_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.supplydetails_id_seq', 1, true);


--
-- Name: supplyorders_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.supplyorders_id_seq', 2, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.users_id_seq', 5, true);


--
-- Name: subscription_id_seq; Type: SEQUENCE SET; Schema: realtime; Owner: -
--

SELECT pg_catalog.setval('realtime.subscription_id_seq', 1, false);


--
-- Name: mfa_amr_claims amr_id_pk; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_amr_claims
    ADD CONSTRAINT amr_id_pk PRIMARY KEY (id);


--
-- Name: audit_log_entries audit_log_entries_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.audit_log_entries
    ADD CONSTRAINT audit_log_entries_pkey PRIMARY KEY (id);


--
-- Name: custom_oauth_providers custom_oauth_providers_identifier_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.custom_oauth_providers
    ADD CONSTRAINT custom_oauth_providers_identifier_key UNIQUE (identifier);


--
-- Name: custom_oauth_providers custom_oauth_providers_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.custom_oauth_providers
    ADD CONSTRAINT custom_oauth_providers_pkey PRIMARY KEY (id);


--
-- Name: flow_state flow_state_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.flow_state
    ADD CONSTRAINT flow_state_pkey PRIMARY KEY (id);


--
-- Name: identities identities_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.identities
    ADD CONSTRAINT identities_pkey PRIMARY KEY (id);


--
-- Name: identities identities_provider_id_provider_unique; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.identities
    ADD CONSTRAINT identities_provider_id_provider_unique UNIQUE (provider_id, provider);


--
-- Name: instances instances_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.instances
    ADD CONSTRAINT instances_pkey PRIMARY KEY (id);


--
-- Name: mfa_amr_claims mfa_amr_claims_session_id_authentication_method_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_amr_claims
    ADD CONSTRAINT mfa_amr_claims_session_id_authentication_method_pkey UNIQUE (session_id, authentication_method);


--
-- Name: mfa_challenges mfa_challenges_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_challenges
    ADD CONSTRAINT mfa_challenges_pkey PRIMARY KEY (id);


--
-- Name: mfa_factors mfa_factors_last_challenged_at_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_factors
    ADD CONSTRAINT mfa_factors_last_challenged_at_key UNIQUE (last_challenged_at);


--
-- Name: mfa_factors mfa_factors_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_factors
    ADD CONSTRAINT mfa_factors_pkey PRIMARY KEY (id);


--
-- Name: oauth_authorizations oauth_authorizations_authorization_code_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_authorizations
    ADD CONSTRAINT oauth_authorizations_authorization_code_key UNIQUE (authorization_code);


--
-- Name: oauth_authorizations oauth_authorizations_authorization_id_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_authorizations
    ADD CONSTRAINT oauth_authorizations_authorization_id_key UNIQUE (authorization_id);


--
-- Name: oauth_authorizations oauth_authorizations_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_authorizations
    ADD CONSTRAINT oauth_authorizations_pkey PRIMARY KEY (id);


--
-- Name: oauth_client_states oauth_client_states_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_client_states
    ADD CONSTRAINT oauth_client_states_pkey PRIMARY KEY (id);


--
-- Name: oauth_clients oauth_clients_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_clients
    ADD CONSTRAINT oauth_clients_pkey PRIMARY KEY (id);


--
-- Name: oauth_consents oauth_consents_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_consents
    ADD CONSTRAINT oauth_consents_pkey PRIMARY KEY (id);


--
-- Name: oauth_consents oauth_consents_user_client_unique; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_consents
    ADD CONSTRAINT oauth_consents_user_client_unique UNIQUE (user_id, client_id);


--
-- Name: one_time_tokens one_time_tokens_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.one_time_tokens
    ADD CONSTRAINT one_time_tokens_pkey PRIMARY KEY (id);


--
-- Name: refresh_tokens refresh_tokens_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.refresh_tokens
    ADD CONSTRAINT refresh_tokens_pkey PRIMARY KEY (id);


--
-- Name: refresh_tokens refresh_tokens_token_unique; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.refresh_tokens
    ADD CONSTRAINT refresh_tokens_token_unique UNIQUE (token);


--
-- Name: saml_providers saml_providers_entity_id_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_providers
    ADD CONSTRAINT saml_providers_entity_id_key UNIQUE (entity_id);


--
-- Name: saml_providers saml_providers_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_providers
    ADD CONSTRAINT saml_providers_pkey PRIMARY KEY (id);


--
-- Name: saml_relay_states saml_relay_states_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_relay_states
    ADD CONSTRAINT saml_relay_states_pkey PRIMARY KEY (id);


--
-- Name: schema_migrations schema_migrations_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.schema_migrations
    ADD CONSTRAINT schema_migrations_pkey PRIMARY KEY (version);


--
-- Name: sessions sessions_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sessions
    ADD CONSTRAINT sessions_pkey PRIMARY KEY (id);


--
-- Name: sso_domains sso_domains_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sso_domains
    ADD CONSTRAINT sso_domains_pkey PRIMARY KEY (id);


--
-- Name: sso_providers sso_providers_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sso_providers
    ADD CONSTRAINT sso_providers_pkey PRIMARY KEY (id);


--
-- Name: users users_phone_key; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.users
    ADD CONSTRAINT users_phone_key UNIQUE (phone);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: webauthn_challenges webauthn_challenges_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.webauthn_challenges
    ADD CONSTRAINT webauthn_challenges_pkey PRIMARY KEY (id);


--
-- Name: webauthn_credentials webauthn_credentials_pkey; Type: CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.webauthn_credentials
    ADD CONSTRAINT webauthn_credentials_pkey PRIMARY KEY (id);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- Name: customerorders customerorders_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customerorders
    ADD CONSTRAINT customerorders_pkey PRIMARY KEY (id);


--
-- Name: customers customers_phone_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_phone_key UNIQUE (phone);


--
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- Name: orderdetails orderdetails_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.orderdetails
    ADD CONSTRAINT orderdetails_pkey PRIMARY KEY (id);


--
-- Name: shifts shifts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.shifts
    ADD CONSTRAINT shifts_pkey PRIMARY KEY (id);


--
-- Name: sportitem_variants sportitem_variants_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitem_variants
    ADD CONSTRAINT sportitem_variants_pkey PRIMARY KEY (id);


--
-- Name: sportitems sportitems_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitems
    ADD CONSTRAINT sportitems_pkey PRIMARY KEY (id);


--
-- Name: suppliers suppliers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT suppliers_pkey PRIMARY KEY (id);


--
-- Name: supplydetails supplydetails_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplydetails
    ADD CONSTRAINT supplydetails_pkey PRIMARY KEY (id);


--
-- Name: supplyorders supplyorders_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplyorders
    ADD CONSTRAINT supplyorders_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: messages messages_pkey; Type: CONSTRAINT; Schema: realtime; Owner: -
--

ALTER TABLE ONLY realtime.messages
    ADD CONSTRAINT messages_pkey PRIMARY KEY (id, inserted_at);


--
-- Name: subscription pk_subscription; Type: CONSTRAINT; Schema: realtime; Owner: -
--

ALTER TABLE ONLY realtime.subscription
    ADD CONSTRAINT pk_subscription PRIMARY KEY (id);


--
-- Name: schema_migrations schema_migrations_pkey; Type: CONSTRAINT; Schema: realtime; Owner: -
--

ALTER TABLE ONLY realtime.schema_migrations
    ADD CONSTRAINT schema_migrations_pkey PRIMARY KEY (version);


--
-- Name: buckets_analytics buckets_analytics_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.buckets_analytics
    ADD CONSTRAINT buckets_analytics_pkey PRIMARY KEY (id);


--
-- Name: buckets buckets_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.buckets
    ADD CONSTRAINT buckets_pkey PRIMARY KEY (id);


--
-- Name: buckets_vectors buckets_vectors_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.buckets_vectors
    ADD CONSTRAINT buckets_vectors_pkey PRIMARY KEY (id);


--
-- Name: migrations migrations_name_key; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.migrations
    ADD CONSTRAINT migrations_name_key UNIQUE (name);


--
-- Name: migrations migrations_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.migrations
    ADD CONSTRAINT migrations_pkey PRIMARY KEY (id);


--
-- Name: objects objects_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.objects
    ADD CONSTRAINT objects_pkey PRIMARY KEY (id);


--
-- Name: s3_multipart_uploads_parts s3_multipart_uploads_parts_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.s3_multipart_uploads_parts
    ADD CONSTRAINT s3_multipart_uploads_parts_pkey PRIMARY KEY (id);


--
-- Name: s3_multipart_uploads s3_multipart_uploads_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.s3_multipart_uploads
    ADD CONSTRAINT s3_multipart_uploads_pkey PRIMARY KEY (id);


--
-- Name: vector_indexes vector_indexes_pkey; Type: CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.vector_indexes
    ADD CONSTRAINT vector_indexes_pkey PRIMARY KEY (id);


--
-- Name: audit_logs_instance_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX audit_logs_instance_id_idx ON auth.audit_log_entries USING btree (instance_id);


--
-- Name: confirmation_token_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX confirmation_token_idx ON auth.users USING btree (confirmation_token) WHERE ((confirmation_token)::text !~ '^[0-9 ]*$'::text);


--
-- Name: custom_oauth_providers_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX custom_oauth_providers_created_at_idx ON auth.custom_oauth_providers USING btree (created_at);


--
-- Name: custom_oauth_providers_enabled_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX custom_oauth_providers_enabled_idx ON auth.custom_oauth_providers USING btree (enabled);


--
-- Name: custom_oauth_providers_identifier_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX custom_oauth_providers_identifier_idx ON auth.custom_oauth_providers USING btree (identifier);


--
-- Name: custom_oauth_providers_provider_type_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX custom_oauth_providers_provider_type_idx ON auth.custom_oauth_providers USING btree (provider_type);


--
-- Name: email_change_token_current_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX email_change_token_current_idx ON auth.users USING btree (email_change_token_current) WHERE ((email_change_token_current)::text !~ '^[0-9 ]*$'::text);


--
-- Name: email_change_token_new_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX email_change_token_new_idx ON auth.users USING btree (email_change_token_new) WHERE ((email_change_token_new)::text !~ '^[0-9 ]*$'::text);


--
-- Name: factor_id_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX factor_id_created_at_idx ON auth.mfa_factors USING btree (user_id, created_at);


--
-- Name: flow_state_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX flow_state_created_at_idx ON auth.flow_state USING btree (created_at DESC);


--
-- Name: identities_email_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX identities_email_idx ON auth.identities USING btree (email text_pattern_ops);


--
-- Name: INDEX identities_email_idx; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON INDEX auth.identities_email_idx IS 'Auth: Ensures indexed queries on the email column';


--
-- Name: identities_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX identities_user_id_idx ON auth.identities USING btree (user_id);


--
-- Name: idx_auth_code; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX idx_auth_code ON auth.flow_state USING btree (auth_code);


--
-- Name: idx_oauth_client_states_created_at; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX idx_oauth_client_states_created_at ON auth.oauth_client_states USING btree (created_at);


--
-- Name: idx_user_id_auth_method; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX idx_user_id_auth_method ON auth.flow_state USING btree (user_id, authentication_method);


--
-- Name: mfa_challenge_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX mfa_challenge_created_at_idx ON auth.mfa_challenges USING btree (created_at DESC);


--
-- Name: mfa_factors_user_friendly_name_unique; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX mfa_factors_user_friendly_name_unique ON auth.mfa_factors USING btree (friendly_name, user_id) WHERE (TRIM(BOTH FROM friendly_name) <> ''::text);


--
-- Name: mfa_factors_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX mfa_factors_user_id_idx ON auth.mfa_factors USING btree (user_id);


--
-- Name: oauth_auth_pending_exp_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX oauth_auth_pending_exp_idx ON auth.oauth_authorizations USING btree (expires_at) WHERE (status = 'pending'::auth.oauth_authorization_status);


--
-- Name: oauth_clients_deleted_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX oauth_clients_deleted_at_idx ON auth.oauth_clients USING btree (deleted_at);


--
-- Name: oauth_consents_active_client_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX oauth_consents_active_client_idx ON auth.oauth_consents USING btree (client_id) WHERE (revoked_at IS NULL);


--
-- Name: oauth_consents_active_user_client_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX oauth_consents_active_user_client_idx ON auth.oauth_consents USING btree (user_id, client_id) WHERE (revoked_at IS NULL);


--
-- Name: oauth_consents_user_order_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX oauth_consents_user_order_idx ON auth.oauth_consents USING btree (user_id, granted_at DESC);


--
-- Name: one_time_tokens_relates_to_hash_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX one_time_tokens_relates_to_hash_idx ON auth.one_time_tokens USING hash (relates_to);


--
-- Name: one_time_tokens_token_hash_hash_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX one_time_tokens_token_hash_hash_idx ON auth.one_time_tokens USING hash (token_hash);


--
-- Name: one_time_tokens_user_id_token_type_key; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX one_time_tokens_user_id_token_type_key ON auth.one_time_tokens USING btree (user_id, token_type);


--
-- Name: reauthentication_token_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX reauthentication_token_idx ON auth.users USING btree (reauthentication_token) WHERE ((reauthentication_token)::text !~ '^[0-9 ]*$'::text);


--
-- Name: recovery_token_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX recovery_token_idx ON auth.users USING btree (recovery_token) WHERE ((recovery_token)::text !~ '^[0-9 ]*$'::text);


--
-- Name: refresh_tokens_instance_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX refresh_tokens_instance_id_idx ON auth.refresh_tokens USING btree (instance_id);


--
-- Name: refresh_tokens_instance_id_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX refresh_tokens_instance_id_user_id_idx ON auth.refresh_tokens USING btree (instance_id, user_id);


--
-- Name: refresh_tokens_parent_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX refresh_tokens_parent_idx ON auth.refresh_tokens USING btree (parent);


--
-- Name: refresh_tokens_session_id_revoked_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX refresh_tokens_session_id_revoked_idx ON auth.refresh_tokens USING btree (session_id, revoked);


--
-- Name: refresh_tokens_updated_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX refresh_tokens_updated_at_idx ON auth.refresh_tokens USING btree (updated_at DESC);


--
-- Name: saml_providers_sso_provider_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX saml_providers_sso_provider_id_idx ON auth.saml_providers USING btree (sso_provider_id);


--
-- Name: saml_relay_states_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX saml_relay_states_created_at_idx ON auth.saml_relay_states USING btree (created_at DESC);


--
-- Name: saml_relay_states_for_email_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX saml_relay_states_for_email_idx ON auth.saml_relay_states USING btree (for_email);


--
-- Name: saml_relay_states_sso_provider_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX saml_relay_states_sso_provider_id_idx ON auth.saml_relay_states USING btree (sso_provider_id);


--
-- Name: sessions_not_after_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX sessions_not_after_idx ON auth.sessions USING btree (not_after DESC);


--
-- Name: sessions_oauth_client_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX sessions_oauth_client_id_idx ON auth.sessions USING btree (oauth_client_id);


--
-- Name: sessions_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX sessions_user_id_idx ON auth.sessions USING btree (user_id);


--
-- Name: sso_domains_domain_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX sso_domains_domain_idx ON auth.sso_domains USING btree (lower(domain));


--
-- Name: sso_domains_sso_provider_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX sso_domains_sso_provider_id_idx ON auth.sso_domains USING btree (sso_provider_id);


--
-- Name: sso_providers_resource_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX sso_providers_resource_id_idx ON auth.sso_providers USING btree (lower(resource_id));


--
-- Name: sso_providers_resource_id_pattern_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX sso_providers_resource_id_pattern_idx ON auth.sso_providers USING btree (resource_id text_pattern_ops);


--
-- Name: unique_phone_factor_per_user; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX unique_phone_factor_per_user ON auth.mfa_factors USING btree (user_id, phone);


--
-- Name: user_id_created_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX user_id_created_at_idx ON auth.sessions USING btree (user_id, created_at);


--
-- Name: users_email_partial_key; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX users_email_partial_key ON auth.users USING btree (email) WHERE (is_sso_user = false);


--
-- Name: INDEX users_email_partial_key; Type: COMMENT; Schema: auth; Owner: -
--

COMMENT ON INDEX auth.users_email_partial_key IS 'Auth: A partial unique index that applies only when is_sso_user is false';


--
-- Name: users_instance_id_email_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX users_instance_id_email_idx ON auth.users USING btree (instance_id, lower((email)::text));


--
-- Name: users_instance_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX users_instance_id_idx ON auth.users USING btree (instance_id);


--
-- Name: users_is_anonymous_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX users_is_anonymous_idx ON auth.users USING btree (is_anonymous);


--
-- Name: webauthn_challenges_expires_at_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX webauthn_challenges_expires_at_idx ON auth.webauthn_challenges USING btree (expires_at);


--
-- Name: webauthn_challenges_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX webauthn_challenges_user_id_idx ON auth.webauthn_challenges USING btree (user_id);


--
-- Name: webauthn_credentials_credential_id_key; Type: INDEX; Schema: auth; Owner: -
--

CREATE UNIQUE INDEX webauthn_credentials_credential_id_key ON auth.webauthn_credentials USING btree (credential_id);


--
-- Name: webauthn_credentials_user_id_idx; Type: INDEX; Schema: auth; Owner: -
--

CREATE INDEX webauthn_credentials_user_id_idx ON auth.webauthn_credentials USING btree (user_id);


--
-- Name: ix_sportitem_variants_item_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sportitem_variants_item_id ON public.sportitem_variants USING btree (sportitem_id);


--
-- Name: ux_sportitem_variants_combo; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_sportitem_variants_combo ON public.sportitem_variants USING btree (sportitem_id, COALESCE(size, ''::text), COALESCE(color, ''::text));


--
-- Name: ux_sportitem_variants_sku; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_sportitem_variants_sku ON public.sportitem_variants USING btree (sku) WHERE ((sku IS NOT NULL) AND (TRIM(BOTH FROM sku) <> ''::text));


--
-- Name: ix_realtime_subscription_entity; Type: INDEX; Schema: realtime; Owner: -
--

CREATE INDEX ix_realtime_subscription_entity ON realtime.subscription USING btree (entity);


--
-- Name: messages_inserted_at_topic_index; Type: INDEX; Schema: realtime; Owner: -
--

CREATE INDEX messages_inserted_at_topic_index ON ONLY realtime.messages USING btree (inserted_at DESC, topic) WHERE ((extension = 'broadcast'::text) AND (private IS TRUE));


--
-- Name: subscription_subscription_id_entity_filters_action_filter_key; Type: INDEX; Schema: realtime; Owner: -
--

CREATE UNIQUE INDEX subscription_subscription_id_entity_filters_action_filter_key ON realtime.subscription USING btree (subscription_id, entity, filters, action_filter);


--
-- Name: bname; Type: INDEX; Schema: storage; Owner: -
--

CREATE UNIQUE INDEX bname ON storage.buckets USING btree (name);


--
-- Name: bucketid_objname; Type: INDEX; Schema: storage; Owner: -
--

CREATE UNIQUE INDEX bucketid_objname ON storage.objects USING btree (bucket_id, name);


--
-- Name: buckets_analytics_unique_name_idx; Type: INDEX; Schema: storage; Owner: -
--

CREATE UNIQUE INDEX buckets_analytics_unique_name_idx ON storage.buckets_analytics USING btree (name) WHERE (deleted_at IS NULL);


--
-- Name: idx_multipart_uploads_list; Type: INDEX; Schema: storage; Owner: -
--

CREATE INDEX idx_multipart_uploads_list ON storage.s3_multipart_uploads USING btree (bucket_id, key, created_at);


--
-- Name: idx_objects_bucket_id_name; Type: INDEX; Schema: storage; Owner: -
--

CREATE INDEX idx_objects_bucket_id_name ON storage.objects USING btree (bucket_id, name COLLATE "C");


--
-- Name: idx_objects_bucket_id_name_lower; Type: INDEX; Schema: storage; Owner: -
--

CREATE INDEX idx_objects_bucket_id_name_lower ON storage.objects USING btree (bucket_id, lower(name) COLLATE "C");


--
-- Name: name_prefix_search; Type: INDEX; Schema: storage; Owner: -
--

CREATE INDEX name_prefix_search ON storage.objects USING btree (name text_pattern_ops);


--
-- Name: vector_indexes_name_bucket_id_idx; Type: INDEX; Schema: storage; Owner: -
--

CREATE UNIQUE INDEX vector_indexes_name_bucket_id_idx ON storage.vector_indexes USING btree (name, bucket_id);


--
-- Name: users on_auth_user_created; Type: TRIGGER; Schema: auth; Owner: -
--

CREATE TRIGGER on_auth_user_created AFTER INSERT ON auth.users FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();


--
-- Name: sportitem_variants sportitem_variants_refresh_aggregate; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER sportitem_variants_refresh_aggregate AFTER INSERT OR DELETE OR UPDATE ON public.sportitem_variants FOR EACH ROW EXECUTE FUNCTION public.trg_refresh_sportitem_aggregate();


--
-- Name: sportitem_variants sportitem_variants_refresh_parent_stock; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER sportitem_variants_refresh_parent_stock AFTER INSERT OR DELETE OR UPDATE ON public.sportitem_variants FOR EACH ROW EXECUTE FUNCTION public.trg_refresh_parent_stock_from_variants();


--
-- Name: sportitems sportitems_force_stock_from_variants; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER sportitems_force_stock_from_variants BEFORE INSERT OR UPDATE OF stock_quantity ON public.sportitems FOR EACH ROW EXECUTE FUNCTION public.trg_force_parent_stock_from_variants();


--
-- Name: supplydetails tr_add_stock_on_supply; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER tr_add_stock_on_supply AFTER INSERT ON public.supplydetails FOR EACH ROW EXECUTE FUNCTION public.handle_stock_on_supply();


--
-- Name: orderdetails tr_subtract_stock_on_sale; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER tr_subtract_stock_on_sale AFTER INSERT ON public.orderdetails FOR EACH ROW EXECUTE FUNCTION public.handle_stock_on_sale();


--
-- Name: subscription tr_check_filters; Type: TRIGGER; Schema: realtime; Owner: -
--

CREATE TRIGGER tr_check_filters BEFORE INSERT OR UPDATE ON realtime.subscription FOR EACH ROW EXECUTE FUNCTION realtime.subscription_check_filters();


--
-- Name: buckets enforce_bucket_name_length_trigger; Type: TRIGGER; Schema: storage; Owner: -
--

CREATE TRIGGER enforce_bucket_name_length_trigger BEFORE INSERT OR UPDATE OF name ON storage.buckets FOR EACH ROW EXECUTE FUNCTION storage.enforce_bucket_name_length();


--
-- Name: buckets protect_buckets_delete; Type: TRIGGER; Schema: storage; Owner: -
--

CREATE TRIGGER protect_buckets_delete BEFORE DELETE ON storage.buckets FOR EACH STATEMENT EXECUTE FUNCTION storage.protect_delete();


--
-- Name: objects protect_objects_delete; Type: TRIGGER; Schema: storage; Owner: -
--

CREATE TRIGGER protect_objects_delete BEFORE DELETE ON storage.objects FOR EACH STATEMENT EXECUTE FUNCTION storage.protect_delete();


--
-- Name: objects update_objects_updated_at; Type: TRIGGER; Schema: storage; Owner: -
--

CREATE TRIGGER update_objects_updated_at BEFORE UPDATE ON storage.objects FOR EACH ROW EXECUTE FUNCTION storage.update_updated_at_column();


--
-- Name: identities identities_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.identities
    ADD CONSTRAINT identities_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: mfa_amr_claims mfa_amr_claims_session_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_amr_claims
    ADD CONSTRAINT mfa_amr_claims_session_id_fkey FOREIGN KEY (session_id) REFERENCES auth.sessions(id) ON DELETE CASCADE;


--
-- Name: mfa_challenges mfa_challenges_auth_factor_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_challenges
    ADD CONSTRAINT mfa_challenges_auth_factor_id_fkey FOREIGN KEY (factor_id) REFERENCES auth.mfa_factors(id) ON DELETE CASCADE;


--
-- Name: mfa_factors mfa_factors_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.mfa_factors
    ADD CONSTRAINT mfa_factors_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: oauth_authorizations oauth_authorizations_client_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_authorizations
    ADD CONSTRAINT oauth_authorizations_client_id_fkey FOREIGN KEY (client_id) REFERENCES auth.oauth_clients(id) ON DELETE CASCADE;


--
-- Name: oauth_authorizations oauth_authorizations_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_authorizations
    ADD CONSTRAINT oauth_authorizations_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: oauth_consents oauth_consents_client_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_consents
    ADD CONSTRAINT oauth_consents_client_id_fkey FOREIGN KEY (client_id) REFERENCES auth.oauth_clients(id) ON DELETE CASCADE;


--
-- Name: oauth_consents oauth_consents_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.oauth_consents
    ADD CONSTRAINT oauth_consents_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: one_time_tokens one_time_tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.one_time_tokens
    ADD CONSTRAINT one_time_tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: refresh_tokens refresh_tokens_session_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.refresh_tokens
    ADD CONSTRAINT refresh_tokens_session_id_fkey FOREIGN KEY (session_id) REFERENCES auth.sessions(id) ON DELETE CASCADE;


--
-- Name: saml_providers saml_providers_sso_provider_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_providers
    ADD CONSTRAINT saml_providers_sso_provider_id_fkey FOREIGN KEY (sso_provider_id) REFERENCES auth.sso_providers(id) ON DELETE CASCADE;


--
-- Name: saml_relay_states saml_relay_states_flow_state_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_relay_states
    ADD CONSTRAINT saml_relay_states_flow_state_id_fkey FOREIGN KEY (flow_state_id) REFERENCES auth.flow_state(id) ON DELETE CASCADE;


--
-- Name: saml_relay_states saml_relay_states_sso_provider_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.saml_relay_states
    ADD CONSTRAINT saml_relay_states_sso_provider_id_fkey FOREIGN KEY (sso_provider_id) REFERENCES auth.sso_providers(id) ON DELETE CASCADE;


--
-- Name: sessions sessions_oauth_client_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sessions
    ADD CONSTRAINT sessions_oauth_client_id_fkey FOREIGN KEY (oauth_client_id) REFERENCES auth.oauth_clients(id) ON DELETE CASCADE;


--
-- Name: sessions sessions_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sessions
    ADD CONSTRAINT sessions_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: sso_domains sso_domains_sso_provider_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.sso_domains
    ADD CONSTRAINT sso_domains_sso_provider_id_fkey FOREIGN KEY (sso_provider_id) REFERENCES auth.sso_providers(id) ON DELETE CASCADE;


--
-- Name: webauthn_challenges webauthn_challenges_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.webauthn_challenges
    ADD CONSTRAINT webauthn_challenges_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: webauthn_credentials webauthn_credentials_user_id_fkey; Type: FK CONSTRAINT; Schema: auth; Owner: -
--

ALTER TABLE ONLY auth.webauthn_credentials
    ADD CONSTRAINT webauthn_credentials_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;


--
-- Name: customerorders customerorders_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customerorders
    ADD CONSTRAINT customerorders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id);


--
-- Name: customerorders customerorders_seller_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customerorders
    ADD CONSTRAINT customerorders_seller_id_fkey FOREIGN KEY (seller_id) REFERENCES public.users(id);


--
-- Name: sportitem_variants fk_sportitem_variants_sportitem; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitem_variants
    ADD CONSTRAINT fk_sportitem_variants_sportitem FOREIGN KEY (sportitem_id) REFERENCES public.sportitems(id) ON DELETE CASCADE;


--
-- Name: supplydetails fk_supplydetails_variant; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplydetails
    ADD CONSTRAINT fk_supplydetails_variant FOREIGN KEY (variant_id) REFERENCES public.sportitem_variants(id) ON DELETE SET NULL;


--
-- Name: orderdetails orderdetails_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.orderdetails
    ADD CONSTRAINT orderdetails_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.sportitems(id);


--
-- Name: orderdetails orderdetails_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.orderdetails
    ADD CONSTRAINT orderdetails_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.customerorders(id) ON DELETE CASCADE;


--
-- Name: shifts shifts_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.shifts
    ADD CONSTRAINT shifts_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: sportitem_variants sportitem_variants_sportitem_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitem_variants
    ADD CONSTRAINT sportitem_variants_sportitem_id_fkey FOREIGN KEY (sportitem_id) REFERENCES public.sportitems(id) ON DELETE CASCADE;


--
-- Name: sportitems sportitems_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sportitems
    ADD CONSTRAINT sportitems_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(id);


--
-- Name: supplydetails supplydetails_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplydetails
    ADD CONSTRAINT supplydetails_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.sportitems(id);


--
-- Name: supplydetails supplydetails_supply_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplydetails
    ADD CONSTRAINT supplydetails_supply_id_fkey FOREIGN KEY (supply_id) REFERENCES public.supplyorders(id) ON DELETE CASCADE;


--
-- Name: supplyorders supplyorders_supplier_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.supplyorders
    ADD CONSTRAINT supplyorders_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id);


--
-- Name: objects objects_bucketId_fkey; Type: FK CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.objects
    ADD CONSTRAINT "objects_bucketId_fkey" FOREIGN KEY (bucket_id) REFERENCES storage.buckets(id);


--
-- Name: s3_multipart_uploads s3_multipart_uploads_bucket_id_fkey; Type: FK CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.s3_multipart_uploads
    ADD CONSTRAINT s3_multipart_uploads_bucket_id_fkey FOREIGN KEY (bucket_id) REFERENCES storage.buckets(id);


--
-- Name: s3_multipart_uploads_parts s3_multipart_uploads_parts_bucket_id_fkey; Type: FK CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.s3_multipart_uploads_parts
    ADD CONSTRAINT s3_multipart_uploads_parts_bucket_id_fkey FOREIGN KEY (bucket_id) REFERENCES storage.buckets(id);


--
-- Name: s3_multipart_uploads_parts s3_multipart_uploads_parts_upload_id_fkey; Type: FK CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.s3_multipart_uploads_parts
    ADD CONSTRAINT s3_multipart_uploads_parts_upload_id_fkey FOREIGN KEY (upload_id) REFERENCES storage.s3_multipart_uploads(id) ON DELETE CASCADE;


--
-- Name: vector_indexes vector_indexes_bucket_id_fkey; Type: FK CONSTRAINT; Schema: storage; Owner: -
--

ALTER TABLE ONLY storage.vector_indexes
    ADD CONSTRAINT vector_indexes_bucket_id_fkey FOREIGN KEY (bucket_id) REFERENCES storage.buckets_vectors(id);


--
-- Name: audit_log_entries; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.audit_log_entries ENABLE ROW LEVEL SECURITY;

--
-- Name: flow_state; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.flow_state ENABLE ROW LEVEL SECURITY;

--
-- Name: identities; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.identities ENABLE ROW LEVEL SECURITY;

--
-- Name: instances; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.instances ENABLE ROW LEVEL SECURITY;

--
-- Name: mfa_amr_claims; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.mfa_amr_claims ENABLE ROW LEVEL SECURITY;

--
-- Name: mfa_challenges; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.mfa_challenges ENABLE ROW LEVEL SECURITY;

--
-- Name: mfa_factors; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.mfa_factors ENABLE ROW LEVEL SECURITY;

--
-- Name: one_time_tokens; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.one_time_tokens ENABLE ROW LEVEL SECURITY;

--
-- Name: refresh_tokens; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.refresh_tokens ENABLE ROW LEVEL SECURITY;

--
-- Name: saml_providers; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.saml_providers ENABLE ROW LEVEL SECURITY;

--
-- Name: saml_relay_states; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.saml_relay_states ENABLE ROW LEVEL SECURITY;

--
-- Name: schema_migrations; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.schema_migrations ENABLE ROW LEVEL SECURITY;

--
-- Name: sessions; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.sessions ENABLE ROW LEVEL SECURITY;

--
-- Name: sso_domains; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.sso_domains ENABLE ROW LEVEL SECURITY;

--
-- Name: sso_providers; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.sso_providers ENABLE ROW LEVEL SECURITY;

--
-- Name: users; Type: ROW SECURITY; Schema: auth; Owner: -
--

ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;

--
-- Name: customers; Type: ROW SECURITY; Schema: public; Owner: -
--

ALTER TABLE public.customers ENABLE ROW LEVEL SECURITY;

--
-- Name: shifts; Type: ROW SECURITY; Schema: public; Owner: -
--

ALTER TABLE public.shifts ENABLE ROW LEVEL SECURITY;

--
-- Name: messages; Type: ROW SECURITY; Schema: realtime; Owner: -
--

ALTER TABLE realtime.messages ENABLE ROW LEVEL SECURITY;

--
-- Name: objects Allow public uploads; Type: POLICY; Schema: storage; Owner: -
--

CREATE POLICY "Allow public uploads" ON storage.objects FOR INSERT WITH CHECK ((bucket_id = 'sport image'::text));


--
-- Name: objects Allow public view; Type: POLICY; Schema: storage; Owner: -
--

CREATE POLICY "Allow public view" ON storage.objects FOR SELECT USING ((bucket_id = 'sport image'::text));


--
-- Name: buckets; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.buckets ENABLE ROW LEVEL SECURITY;

--
-- Name: buckets_analytics; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.buckets_analytics ENABLE ROW LEVEL SECURITY;

--
-- Name: buckets_vectors; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.buckets_vectors ENABLE ROW LEVEL SECURITY;

--
-- Name: migrations; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.migrations ENABLE ROW LEVEL SECURITY;

--
-- Name: objects; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.objects ENABLE ROW LEVEL SECURITY;

--
-- Name: s3_multipart_uploads; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.s3_multipart_uploads ENABLE ROW LEVEL SECURITY;

--
-- Name: s3_multipart_uploads_parts; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.s3_multipart_uploads_parts ENABLE ROW LEVEL SECURITY;

--
-- Name: vector_indexes; Type: ROW SECURITY; Schema: storage; Owner: -
--

ALTER TABLE storage.vector_indexes ENABLE ROW LEVEL SECURITY;

--
-- Name: supabase_realtime; Type: PUBLICATION; Schema: -; Owner: -
--

CREATE PUBLICATION supabase_realtime WITH (publish = 'insert, update, delete, truncate');


--
-- Name: issue_graphql_placeholder; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER issue_graphql_placeholder ON sql_drop
         WHEN TAG IN ('DROP EXTENSION')
   EXECUTE FUNCTION extensions.set_graphql_placeholder();


--
-- Name: issue_pg_cron_access; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER issue_pg_cron_access ON ddl_command_end
         WHEN TAG IN ('CREATE EXTENSION')
   EXECUTE FUNCTION extensions.grant_pg_cron_access();


--
-- Name: issue_pg_graphql_access; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER issue_pg_graphql_access ON ddl_command_end
         WHEN TAG IN ('CREATE FUNCTION')
   EXECUTE FUNCTION extensions.grant_pg_graphql_access();


--
-- Name: issue_pg_net_access; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER issue_pg_net_access ON ddl_command_end
         WHEN TAG IN ('CREATE EXTENSION')
   EXECUTE FUNCTION extensions.grant_pg_net_access();


--
-- Name: pgrst_ddl_watch; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER pgrst_ddl_watch ON ddl_command_end
   EXECUTE FUNCTION extensions.pgrst_ddl_watch();


--
-- Name: pgrst_drop_watch; Type: EVENT TRIGGER; Schema: -; Owner: -
--

CREATE EVENT TRIGGER pgrst_drop_watch ON sql_drop
   EXECUTE FUNCTION extensions.pgrst_drop_watch();


--
-- PostgreSQL database dump complete
--

\unrestrict gB2BdTU6TlQEaeZ5sbVgYYG9Yctbzt7MQ8r3iKAlCRphoKYxCsWY4yMU8C5spPw

