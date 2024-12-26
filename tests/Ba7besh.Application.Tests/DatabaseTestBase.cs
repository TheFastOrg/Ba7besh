using System.Data;

namespace Ba7besh.Application.Tests;

public abstract class DatabaseTestBase(PostgresContainerFixture fixture)
    : IClassFixture<PostgresContainerFixture>, IAsyncLifetime
{
    protected IDbConnection Connection => fixture.Connection;

    public virtual async Task InitializeAsync()
    {
        await ClearDatabase();
        await SeedTestData();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected virtual Task SeedTestData() => Task.CompletedTask;

    private async Task ClearDatabase()
    {
        await Connection.ExecuteAsync(@"
            DO $$
            DECLARE 
                r RECORD;
            BEGIN
                FOR r IN (
                    SELECT tablename 
                    FROM pg_tables 
                    WHERE schemaname = 'public' 
                    AND tablename NOT IN ('migration_runs', 'spatial_ref_sys')
                ) LOOP
                    EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE';
                END LOOP;
            END $$;
        ");
    }
}