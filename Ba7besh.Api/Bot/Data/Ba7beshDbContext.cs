using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Ba7besh.Api.Bot.Data;

public class Ba7beshDbContext : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly ILogger<Ba7beshDbContext> _logger;
    private bool _disposed;

    public Ba7beshDbContext(IConfiguration configuration, ILogger<Ba7beshDbContext> logger)
    {
        _logger = logger;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new NpgsqlConnection(connectionString);
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}