DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'review_comments') THEN
            CREATE TABLE review_comments
            (
                id         varchar(50) PRIMARY KEY,
                review_id  varchar(50)                                        NOT NULL REFERENCES reviews (id),
                user_id    varchar(50)                                        NOT NULL,
                content    text                                               NOT NULL,
                created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at timestamp with time zone,
                deleted_at timestamp with time zone,
                is_deleted boolean                  DEFAULT false             NOT NULL
            );

            CREATE INDEX review_comments_review_id_idx ON review_comments (review_id) WHERE is_deleted = false;
            CREATE INDEX review_comments_user_id_idx ON review_comments (user_id) WHERE is_deleted = false;
        END IF;
    END
$$;