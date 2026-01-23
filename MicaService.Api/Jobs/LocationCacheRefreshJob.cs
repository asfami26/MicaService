using MicaService.Application.Services.Interfaces;
using Quartz;

namespace MicaService.Api.Jobs;

public sealed class LocationCacheRefreshJob(
    ILocationService service,
    ILogger<LocationCacheRefreshJob> logger) : IJob
{
    private readonly ILocationService _service = service;
    private readonly ILogger<LocationCacheRefreshJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var response = await _service.RefreshAsync(context.CancellationToken);
        _logger.LogInformation(
            "Location cache refreshed. Code={Code} Count={Count}",
            response.Code,
            response.Data.Count
        );
    }
}
