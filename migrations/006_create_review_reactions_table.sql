DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'review_reaction') THEN
            CREATE TYPE review_reaction AS ENUM ('helpful', 'unhelpful');
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'review_reactions') THEN
            CREATE TABLE review_reactions (
                                              id varchar(50) PRIMARY KEY,
                                              review_id varchar(50) NOT NULL REFERENCES reviews(id),
                                              user_id varchar(50) NOT NULL,
                                              reaction review_reaction NOT NULL,
                                              created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
                                              updated_at timestamp with time zone,
                                              deleted_at timestamp with time zone,
                                              is_deleted boolean DEFAULT false NOT NULL,
                                              CONSTRAINT unique_user_review_reaction UNIQUE(user_id, review_id)
            );

            CREATE INDEX review_reactions_review_id_idx ON review_reactions(review_id) WHERE is_deleted = false;
            CREATE INDEX review_reactions_user_id_idx ON review_reactions(user_id) WHERE is_deleted = false;
        END IF;
    END $$;