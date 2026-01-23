using System.Data;

namespace MicaService.Application.Repositories;

public interface ISectionDbContext
{
    IDbConnection CreateConnection();
}
