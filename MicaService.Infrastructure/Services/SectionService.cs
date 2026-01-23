using Dapper;
using Microsoft.Extensions.Caching.Memory;
using MicaService.Application.Constants;
using MicaService.Application.DTOs;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;
using MicaService.Domain.Policies;

namespace MicaService.Infrastructure.Services;

public sealed class SectionService(ISectionDbContext db, IMemoryCache cache) : ISectionService
{
    private const string CacheKey = "mica.sections";
    private readonly ISectionDbContext _db = db;
    private readonly IMemoryCache _cache = cache;

    public async Task<ApiResponseDto<SectionResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out List<SectionResponseDto>? cached) && cached is not null)
        {
            return new ApiResponseDto<SectionResponseDto>(
                ResponseCodes.Ok,
                ResponseMessages.SectionsLoadedFromCache,
                cached
            );
        }

        var refreshed = await RefreshAsync(cancellationToken);
        return new ApiResponseDto<SectionResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.SectionsLoaded,
            refreshed.Data
        );
    }

    public async Task<ApiResponseDto<SectionResponseDto>> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                SECT_ID AS SectId,
                SECT_NAME AS SectName,
                DEPT_ID AS DeptId,
                MGR_EMP_ID AS MgrEmpId
            FROM PFNATT.dbo.TBL_SECTION
            """;

        using var connection = _db.CreateConnection();
        var items = await connection.QueryAsync<SectionResponseDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        var payload = items.ToList();

        _cache.Remove(CacheKey);
        var expiration = SectionCachePolicy.GetNextSchedule(DateTimeOffset.Now);
        _cache.Set(CacheKey, payload, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        });

        return new ApiResponseDto<SectionResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.SectionsRefreshed,
            payload
        );
    }

}
