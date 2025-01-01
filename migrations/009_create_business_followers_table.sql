DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'business_followers') THEN
            CREATE TABLE business_followers
            (
                id           varchar(50) PRIMARY KEY,
                business_id  varchar(50) NOT NULL REFERENCES businesses (id),
                user_id      varchar(50) NOT NULL,
                is_following boolean     NOT NULL     DEFAULT true,
                created_at   timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                CONSTRAINT unique_business_follower UNIQUE (business_id, user_id)
            );

            CREATE INDEX business_followers_user_id_idx ON business_followers (user_id) WHERE is_following = true;
            CREATE INDEX business_followers_business_id_idx ON business_followers (business_id) WHERE is_following = true;
        END IF;
    END
$$;