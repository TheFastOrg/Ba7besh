DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'review_photos') THEN
            CREATE TABLE review_photos
            (
                id          varchar(50) PRIMARY KEY,
                review_id   varchar(50)                                        NOT NULL REFERENCES reviews (id),
                photo_url   varchar(500)                                       NOT NULL,
                description text,
                mime_type   varchar(50)                                        NOT NULL,
                size_bytes  bigint                                             NOT NULL,
                created_at  timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at  timestamp with time zone,
                deleted_at  timestamp with time zone,
                is_deleted  boolean                  DEFAULT false             NOT NULL
            );

            CREATE INDEX review_photos_review_id_idx ON review_photos (review_id) WHERE is_deleted = FALSE;
        END IF;
    END
$$;