DO
$$
    BEGIN

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_tags') THEN
            CREATE TABLE business_tags
            (
                id          varchar(50) PRIMARY KEY,
                created_at  timestamp with time zone NOT NULL,
                updated_at  timestamp with time zone,
                deleted_at  timestamp with time zone,
                is_deleted  boolean                  NOT NULL,
                tag         varchar(255)             NOT NULL,
                business_id varchar(50)              NOT NULL REFERENCES businesses (id) DEFERRABLE INITIALLY DEFERRED
            );

            CREATE INDEX business_tags_business_id_idx ON business_tags (business_id);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_working_hours') THEN
            CREATE TABLE business_working_hours
            (
                id           varchar(50) PRIMARY KEY,
                created_at   timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at   timestamp with time zone,
                deleted_at   timestamp with time zone,
                is_deleted   boolean                  DEFAULT false             NOT NULL,
                day          integer                                            NOT NULL CHECK (day >= 0),
                opening_time time                                               NOT NULL,
                closing_time time                                               NOT NULL,
                business_id  varchar(50)                                        NOT NULL REFERENCES businesses (id) DEFERRABLE INITIALLY DEFERRED,
                CONSTRAINT unique_business_day UNIQUE (business_id, day)
            );

            CREATE INDEX business_working_hours_business_id_idx ON business_working_hours (business_id);
        END IF;

    END
$$;