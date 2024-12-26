DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1
                       FROM pg_type
                       WHERE typname = 'review_dimension') THEN
            CREATE TYPE review_dimension AS ENUM ('taste', 'quality', 'price', 'service', 'atmosphere');
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'reviews') THEN
            CREATE TABLE reviews
            (
                id             varchar(50) PRIMARY KEY,
                business_id    varchar(50)                                        NOT NULL REFERENCES businesses (id),
                user_id        varchar(50)                                        NOT NULL,
                overall_rating decimal(2, 1)                                      NOT NULL CHECK (overall_rating BETWEEN 1 AND 5),
                content        text,
                status         varchar(20)                                        NOT NULL DEFAULT 'pending',
                created_at     timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                updated_at     timestamp with time zone,
                deleted_at     timestamp with time zone,
                is_deleted     boolean                  DEFAULT false             NOT NULL
            );

            CREATE INDEX reviews_business_id_idx ON reviews (business_id);
            CREATE INDEX reviews_user_id_idx ON reviews (user_id);
        END IF;

        IF NOT EXISTS (SELECT
                       FROM pg_tables
                       WHERE schemaname = 'public'
                         AND tablename = 'review_ratings') THEN
            CREATE TABLE review_ratings
            (
                review_id varchar(50) REFERENCES reviews (id),
                dimension review_dimension NOT NULL,
                rating    decimal(2, 1)    NOT NULL CHECK (rating BETWEEN 1 AND 5),
                note      text,
                PRIMARY KEY (review_id, dimension)
            );
        END IF;
    END
$$;