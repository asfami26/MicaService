using System.Data;
using MicaService.Application.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MicaService.Infrastructure.Persistence;

public sealed class SectionDbContext(IConfiguration config) : ISectionDbContext
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
