DO
$$
    BEGIN
        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'registered_devices') THEN
            CREATE TABLE registered_devices
            (
                id                   varchar(50) PRIMARY KEY,
                created_at           timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at           timestamp with time zone,
                deleted_at           timestamp with time zone,
                is_deleted           boolean                  DEFAULT false             NOT NULL,
                app_version          varchar(20)                                        NOT NULL,
                last_used_at         timestamp with time zone,
                status               varchar(20)              DEFAULT 'active'          NOT NULL,
                key_version          int                      DEFAULT 1                 NOT NULL,
                device_signature_key varchar(255)                                       NOT NULL,
                is_banned            boolean                  DEFAULT false             NOT NULL
            );

            CREATE INDEX registered_devices_status_idx ON registered_devices (status) WHERE is_deleted = false;
            CREATE INDEX registered_devices_last_used_at_idx ON registered_devices (last_used_at) WHERE is_deleted = false;
        END IF;
    END
$$;