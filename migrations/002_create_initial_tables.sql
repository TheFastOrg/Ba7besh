DO
$$
    BEGIN

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'businesses') THEN
            CREATE TABLE businesses
            (
                id            varchar(50) PRIMARY KEY,
                created_at    timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                deleted_at    timestamp with time zone,
                is_deleted    boolean                  DEFAULT false             NOT NULL,
                location      geography(Point, 4326)                             NOT NULL,
                updated_at    timestamp with time zone,
                address_line1 varchar(255),
                address_line2 varchar(255),
                ar_name       varchar(255)                                       NOT NULL,
                city          varchar(255),
                country       varchar(255)                                       NOT NULL,
                en_name       varchar(255)                                       NOT NULL,
                slug          varchar(255)                                       NOT NULL UNIQUE,
                status        varchar(255)                                       NOT NULL,
                type          varchar(255)                                       NOT NULL
            );

            CREATE INDEX businesses_location_idx ON businesses USING gist (location);
            CREATE INDEX businesses_slug_idx ON businesses (slug);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'categories') THEN
            CREATE TABLE categories
            (
                id         varchar(50) PRIMARY KEY,
                created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at timestamp with time zone,
                deleted_at timestamp with time zone,
                is_deleted boolean                  DEFAULT false             NOT NULL,
                slug       varchar(255)                                       NOT NULL UNIQUE,
                ar_name    varchar(255),
                en_name    varchar(255),
                parent_id  varchar(50) REFERENCES categories (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX categories_slug_idx ON categories (slug);
            CREATE INDEX categories_parent_id_idx ON categories (parent_id);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'features') THEN
            CREATE TABLE features
            (
                id          varchar(50) PRIMARY KEY,
                created_at  timestamp with time zone NOT NULL,
                updated_at  timestamp with time zone,
                deleted_at  timestamp with time zone,
                is_deleted  boolean                  NOT NULL,
                ar_name     varchar(255)             NOT NULL,
                en_name     varchar(255)             NOT NULL,
                category_id varchar(50)              NOT NULL REFERENCES categories (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX features_category_id_idx ON features (category_id);
        END IF;

    END
$$;