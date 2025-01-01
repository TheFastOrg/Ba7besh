DO
$$
    BEGIN
        IF NOT EXISTS (SELECT
                       FROM pg_type
                       WHERE typname = 'suggestion_status') THEN
            CREATE TYPE suggestion_status AS ENUM ('pending', 'approved', 'rejected');
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'suggested_businesses') THEN
            CREATE TABLE suggested_businesses
            (
                id          varchar(50) PRIMARY KEY,
                ar_name     varchar(255)           NOT NULL,
                en_name     varchar(255)           NOT NULL,
                location    geography(Point, 4326) NOT NULL,
                description text                   NOT NULL,
                user_id     varchar(50)            NOT NULL,
                status      suggestion_status      NOT NULL DEFAULT 'pending',
                admin_notes text,
                created_at  timestamp with time zone        DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at  timestamp with time zone,
                CONSTRAINT unique_business_names UNIQUE (ar_name, en_name)
            );

            CREATE INDEX suggested_businesses_user_id_idx ON suggested_businesses (user_id);
            CREATE INDEX suggested_businesses_status_idx ON suggested_businesses (status);
        END IF;
    END
$$;