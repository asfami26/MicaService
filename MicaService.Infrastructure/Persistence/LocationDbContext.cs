using System.Data;
using MicaService.Application.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MicaService.Infrastructure.Persistence;

public sealed class LocationDbContext(IConfiguration config) : ILocationDbContext
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
