using System.Data;

namespace MicaService.Application.Repositories;

public interface IEmployeeProfileDbContext
{
    IDbConnection CreateConnection();
}
