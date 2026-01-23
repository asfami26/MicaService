using Dapper;
using Microsoft.Extensions.Caching.Memory;
using MicaService.Application.Constants;
using MicaService.Application.DTOs;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;
using MicaService.Domain.Policies;

namespace MicaService.Infrastructure.Services;

public sealed class LocationService(ILocationDbContext db, IMemoryCache cache) : ILocationService
{
    private const string CacheKey = "mica.locations";
    private readonly ILocationDbContext _db = db;
    private readonly IMemoryCache _cache = cache;

    public async Task<ApiResponseDto<LocationResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out List<LocationResponseDto>? cached) && cached is not null)
        {
            return new ApiResponseDto<LocationResponseDto>(
                ResponseCodes.Ok,
                ResponseMessages.LocationsLoadedFromCache,
                cached
            );
        }

        var refreshed = await RefreshAsync(cancellationToken);
        return new ApiResponseDto<LocationResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.LocationsLoaded,
            refreshed.Data
        );
    }

    public async Task<ApiResponseDto<LocationResponseDto>> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                LOC_ID AS LocId,
                LOC_NAME AS LocName,
                COUNTRY AS Country
            FROM PFNATT.dbo.TBL_LOCATION
            """;

        using var connection = _db.CreateConnection();
        var items = await connection.QueryAsync<LocationResponseDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        var payload = items.ToList();

        _cache.Remove(CacheKey);
        var expiration = LocationCachePolicy.GetNextSchedule(DateTimeOffset.Now);
        _cache.Set(CacheKey, payload, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        });

        return new ApiResponseDto<LocationResponseDto>(
            ResponseCodes.Ok,
            ResponseMessages.LocationsRefreshed,
            payload
        );
    }
}
