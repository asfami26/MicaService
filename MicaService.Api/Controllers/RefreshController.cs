using MicaService.Application.Constants;
using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MicaService.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/refresh")]
[Authorize]
public sealed class RefreshController(
    ISectionHttpService sectionService,
    ILocationHttpService locationService,
    IEmployeeProfileHttpService employeeService,
    IMemoryCache cache) : ControllerBase
{
    private const string RefreshAllKey = "mica.refresh.all";
    private const string RefreshAllStartedKey = "mica.refresh.all.started";
    private readonly ISectionHttpService _sectionService = sectionService;
    private readonly ILocationHttpService _locationService = locationService;
    private readonly IEmployeeProfileHttpService _employeeService = employeeService;
    private readonly IMemoryCache _cache = cache;

    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponseDto<RefreshStatusDto>), StatusCodes.Status200OK)]
    public async Task<ApiResponseDto<RefreshStatusDto>> RefreshAll()
    {
        if (_cache.TryGetValue(RefreshAllKey, out bool inProgress) && inProgress)
        {
            return new ApiResponseDto<RefreshStatusDto>(
                ResponseCodes.Ok,
                ResponseMessages.RefreshAllInProgress,
                new List<RefreshStatusDto> { new(true, GetStartedAt()) });
        }

        var startedAt = DateTimeOffset.UtcNow;
        _cache.Set(RefreshAllKey, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
        _cache.Set(RefreshAllStartedKey, startedAt, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        try
        {
            await _sectionService.RefreshAsync();
            await _locationService.RefreshAsync();
            await _employeeService.RefreshAsync();
        }
        finally
        {
            _cache.Remove(RefreshAllKey);
        }

        return new ApiResponseDto<RefreshStatusDto>(
            ResponseCodes.Ok,
            ResponseMessages.RefreshAllCompleted,
            new List<RefreshStatusDto> { new(false, startedAt) });
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponseDto<RefreshStatusDto>), StatusCodes.Status200OK)]
    public ApiResponseDto<RefreshStatusDto> Status()
    {
        var inProgress = _cache.TryGetValue(RefreshAllKey, out bool status) && status;
        return new ApiResponseDto<RefreshStatusDto>(
            ResponseCodes.Ok,
            ResponseMessages.RefreshAllStatusLoaded,
            new List<RefreshStatusDto> { new(inProgress, GetStartedAt()) });
    }

    private DateTimeOffset? GetStartedAt()
        => _cache.TryGetValue(RefreshAllStartedKey, out DateTimeOffset started)
            ? started
            : null;
}
