using Dapper;
using MicaService.Application.Constants;
using MicaService.Application.DTOs;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class EmployeeExperienceService(IEmployeeProfileDbContext db)
    : IEmployeeExperienceService
{
    private readonly IEmployeeProfileDbContext _db = db;

    public async Task<ApiResponseDto<EmployeeExperienceResponseDto>> GetByEmployeeIdAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                EMP_ID AS EmployeeId,
                COMPANY AS Company,
                POS AS Position,
                STARTDATE AS StartDate,
                ENDDATE AS EndDate
            FROM PFNATT.dbo.TBL_JOB_EX
            WHERE EMP_ID = @EmployeeId
            ORDER BY STARTDATE DESC, ENDDATE DESC
            """;

        using var connection = _db.CreateConnection();
        var payload = await connection.QueryAsync<EmployeeExperienceResponseDto>(
            new CommandDefinition(
                sql,
                new { EmployeeId = employeeId },
                cancellationToken: cancellationToken));

        return new ApiResponseDto<EmployeeExperienceResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.EmployeeExperiencesLoaded,
            payload.ToList()
        );
    }
}
