using System.Data;
using MicaService.Application.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MicaService.Infrastructure.Persistence;

public sealed class EmployeeProfileDbContext(IConfiguration config) : IEmployeeProfileDbContext
{
    private readonly string _connectionString = config.GetConnectionString("OdpConnection")
        ?? throw new InvalidOperationException("Connection string 'OdpConnection' not found.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
