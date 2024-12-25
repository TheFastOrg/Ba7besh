DO
$$
    BEGIN

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_categories') THEN
            CREATE TABLE business_categories
            (
                id          varchar(50) PRIMARY KEY,
                created_at  timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at  timestamp with time zone,
                deleted_at  timestamp with time zone,
                is_deleted  boolean                  DEFAULT false             NOT NULL,
                business_id varchar(50)                                        NOT NULL REFERENCES businesses (id) DEFERRABLE INITIALLY DEFERRED,
                category_id varchar(50)                                        NOT NULL REFERENCES categories (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX business_categories_business_id_idx ON business_categories (business_id);
            CREATE INDEX business_categories_category_id_idx ON business_categories (category_id);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_contacts') THEN
            CREATE TABLE business_contacts
            (
                id            varchar(50) PRIMARY KEY,
                created_at    timestamp with time zone NOT NULL,
                updated_at    timestamp with time zone,
                deleted_at    timestamp with time zone,
                is_deleted    boolean                  NOT NULL,
                contact_type  varchar(15)              NOT NULL,
                contact_value varchar(255)             NOT NULL,
                business_id   varchar(50)              NOT NULL REFERENCES businesses (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX business_contacts_business_id_idx ON business_contacts (business_id);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_features') THEN
            CREATE TABLE business_features
            (
                id          varchar(50) PRIMARY KEY,
                created_at  timestamp with time zone NOT NULL,
                updated_at  timestamp with time zone,
                deleted_at  timestamp with time zone,
                is_deleted  boolean                  NOT NULL,
                business_id varchar(50)              NOT NULL REFERENCES businesses (id) DEFERRABLE INITIALLY DEFERRED,
                feature_id  varchar(50)              NOT NULL REFERENCES features (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX business_features_business_id_idx ON business_features (business_id);
            CREATE INDEX business_features_feature_id_idx ON business_features (feature_id);
        END IF;

    END
$$;