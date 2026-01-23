using Dapper;
using MicaService.Application.Constants;
using MicaService.Application.DTOs;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class EmployeeProfileService(IEmployeeProfileDbContext db)
    : IEmployeeProfileService
{
    private readonly IEmployeeProfileDbContext _db = db;

    public async Task<ApiResponseDto<EmployeeProfileResponseDto>> GetAllAsync(
        string? deptId = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _db.CreateConnection();
        var payload = await connection.QueryAsync<EmployeeProfileResponseDto>(
            new CommandDefinition(
                GetCacheSelectSql(deptId),
                new { DeptId = deptId },
                cancellationToken: cancellationToken));

        return new ApiResponseDto<EmployeeProfileResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.EmployeesLoadedFromCacheTable,
            payload.ToList()
        );
    }

    public async Task<ApiResponseDto<EmployeeProfileResponseDto>> GetByIdAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _db.CreateConnection();
        var payload = await connection.QueryAsync<EmployeeProfileResponseDto>(
            new CommandDefinition(
                CacheSelectByEmployeeIdSql,
                new { EmployeeId = employeeId },
                cancellationToken: cancellationToken));

        return new ApiResponseDto<EmployeeProfileResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.EmployeeLoadedFromCacheTable,
            payload.ToList()
        );
    }

    public async Task<ApiResponseDto<EmployeeProfileResponseDto>> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        const int RefreshTimeoutSeconds = 300;
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            new CommandDefinition(
                "TRUNCATE TABLE EMP.EmployeeProfileCache;",
                commandTimeout: RefreshTimeoutSeconds,
                transaction: transaction,
                cancellationToken: cancellationToken));
        await connection.ExecuteAsync(
            new CommandDefinition(
                InsertCacheSql,
                commandTimeout: RefreshTimeoutSeconds,
                transaction: transaction,
                cancellationToken: cancellationToken));
        transaction.Commit();

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "SELECT COUNT(*) FROM EMP.EmployeeProfileCache;",
                commandTimeout: RefreshTimeoutSeconds,
                cancellationToken: cancellationToken));

        return new ApiResponseDto<EmployeeProfileResponseDto>(
            ResponseCodes.Ok,
            string.Format(ResponseMessages.EmployeesRefreshedCountFormat, count),
            new List<EmployeeProfileResponseDto>()
        );
    }

    private static string GetCacheSelectSql(string? deptId)
        => string.IsNullOrWhiteSpace(deptId)
            ? CacheSelectAllSql
            : CacheSelectByDeptSql;

    private const string CacheSelectAllSql = """
        SELECT
            EmployeeId,
            DepartmentId,
            DivisionId,
            FullName,
            Age,
            Status,
            PositionName,
            JgcGrade,
            JoinDate,
            NoKtp,
            CurrentAddress,
            Email,
            PersonalEmail,
            Tax,
            Education1,
            Edu1GradYear,
            Edu1Majoring,
            Education2,
            Edu2GradYear,
            Edu2Majoring,
            Yos,
            AllExp
        FROM EMP.EmployeeProfileCache
        """;

    private const string CacheSelectByDeptSql = """
        SELECT
            EmployeeId,
            DepartmentId,
            DivisionId,
            FullName,
            Age,
            Status,
            PositionName,
            JgcGrade,
            JoinDate,
            NoKtp,
            CurrentAddress,
            Email,
            PersonalEmail,
            Tax,
            Education1,
            Edu1GradYear,
            Edu1Majoring,
            Education2,
            Edu2GradYear,
            Edu2Majoring,
            Yos,
            AllExp
        FROM EMP.EmployeeProfileCache
        WHERE DepartmentId = @DeptId
        """;

    private const string CacheSelectByEmployeeIdSql = """
        SELECT
            EmployeeId,
            DepartmentId,
            DivisionId,
            FullName,
            Age,
            Status,
            PositionName,
            JgcGrade,
            JoinDate,
            NoKtp,
            CurrentAddress,
            Email,
            PersonalEmail,
            Tax,
            Education1,
            Edu1GradYear,
            Edu1Majoring,
            Education2,
            Edu2GradYear,
            Edu2Majoring,
            Yos,
            AllExp
        FROM EMP.EmployeeProfileCache
        WHERE EmployeeId = @EmployeeId
        """;

    private const string InsertCacheSql = """
        INSERT INTO EMP.EmployeeProfileCache (
            EmployeeId,
            DepartmentId,
            DivisionId,
            FullName,
            Age,
            Status,
            PositionName,
            JgcGrade,
            JoinDate,
            NoKtp,
            CurrentAddress,
            Email,
            PersonalEmail,
            Tax,
            Education1,
            Edu1GradYear,
            Edu1Majoring,
            Education2,
            Edu2GradYear,
            Edu2Majoring,
            Yos,
            AllExp
        )
        SELECT
            te.EMP_ID AS EmployeeId,
            te.SECT_ID AS DepartmentId,
            ts.DEPT_ID AS DivisionId,
            te.FULL_NAME AS FullName,
            COALESCE(
                DATEDIFF(YEAR, te.BIRTHDATE, GETDATE())
                  - CASE
                        WHEN DATEADD(YEAR, DATEDIFF(YEAR, te.BIRTHDATE, GETDATE()), te.BIRTHDATE) > GETDATE()
                        THEN 1 ELSE 0
                    END,
                0
            ) AS Age,
            te.EMP_TYPE_ID AS Status,
            tp.POSITION_NAME AS PositionName,
            te.JGC_GRADE AS JgcGrade,
            te.JOIN_DATE AS JoinDate,
            te.IDNUMBER AS NoKtp,
            CONCAT(
                COALESCE(te.PADDRESS, '') COLLATE DATABASE_DEFAULT,
                ' ',
                COALESCE(te.PCITY, '') COLLATE DATABASE_DEFAULT
            ) AS CurrentAddress,
            em.Email AS Email,
            te.EMAIL AS PersonalEmail,
            te.TAX_STATUS_ID AS Tax,
            edu.EDUCATION_1 AS Education1,
            edu.EDU1_GRAD_YEAR AS Edu1GradYear,
            edu.EDU1_MAJORING AS Edu1Majoring,
            edu.EDUCATION_2 AS Education2,
            edu.EDU2_GRAD_YEAR AS Edu2GradYear,
            edu.EDU2_MAJORING AS Edu2Majoring,
            CASE
                WHEN exp.CurrentExp IS NULL THEN 0
                WHEN PATINDEX('%[^0-9]%', exp.CurrentExp) = 0 THEN CAST(exp.CurrentExp AS INT)
                WHEN UPPER(exp.CurrentExp) LIKE '%YEAR%' THEN CAST(LEFT(
                    exp.CurrentExp,
                    PATINDEX('%[^0-9]%', exp.CurrentExp) - 1
                ) AS INT)
                ELSE 0
            END AS Yos,
            CASE
                WHEN exp.TotalExp IS NULL THEN 0
                WHEN PATINDEX('%[^0-9]%', exp.TotalExp) = 0 THEN CAST(exp.TotalExp AS INT)
                WHEN PATINDEX('%[^0-9]%', exp.TotalExp) > 1 THEN CAST(LEFT(
                    exp.TotalExp,
                    PATINDEX('%[^0-9]%', exp.TotalExp) - 1
                ) AS INT)
                ELSE 0
            END AS AllExp
        FROM PFNATT.dbo.TBL_EMP te
        LEFT JOIN PFNATT.dbo.TBL_POSITION tp ON te.POSITION_ID = tp.POSITION_ID
        LEFT JOIN PFNATT.dbo.TBL_SECTION ts ON te.SECT_ID = ts.SECT_ID
        OUTER APPLY (
            SELECT TOP (1) e.EMAIL AS Email
            FROM PFNATT.dbo.TBL_EMAIL e
            WHERE e.EMP_ID = te.EMP_ID
            ORDER BY e.EMAIL
        ) em
        OUTER APPLY (
            SELECT
                PFNATT.dbo.GetCurrentExperience(te.EMP_ID) COLLATE DATABASE_DEFAULT AS CurrentExp,
                PFNATT.dbo.GetTotalExperience(te.EMP_ID) COLLATE DATABASE_DEFAULT AS TotalExp
        ) exp
        OUTER APPLY (
            SELECT
                MAX(CASE WHEN rn = 1 THEN INSTITUTE END) AS EDUCATION_1,
                MAX(CASE WHEN rn = 1 THEN GRAD_YEAR END) AS EDU1_GRAD_YEAR,
                MAX(CASE WHEN rn = 1 THEN MAJORING END) AS EDU1_MAJORING,
                MAX(CASE WHEN rn = 2 THEN INSTITUTE END) AS EDUCATION_2,
                MAX(CASE WHEN rn = 2 THEN GRAD_YEAR END) AS EDU2_GRAD_YEAR,
                MAX(CASE WHEN rn = 2 THEN MAJORING END) AS EDU2_MAJORING
            FROM (
                SELECT TOP (2)
                    ed.INSTITUTE,
                    ed.GRAD_YEAR,
                    ed.MAJORING,
                    ROW_NUMBER() OVER (ORDER BY ed.GRAD_YEAR DESC) AS rn
                FROM PFNATT.dbo.TBL_EDUCATION ed
                WHERE ed.EMP_ID = te.EMP_ID
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%SD%'
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%ELEMENTARY%'
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%PRIMARY%'
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%SMP%'
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%JUNIOR%'
                  AND UPPER(ed.INSTITUTE) NOT LIKE '%MIDDLE%'
                ORDER BY ed.GRAD_YEAR DESC
            ) x
        ) edu
        """;
}
