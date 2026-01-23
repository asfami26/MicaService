using System.Data;

namespace MicaService.Application.Repositories;

public interface ILocationDbContext
{
    IDbConnection CreateConnection();
}
