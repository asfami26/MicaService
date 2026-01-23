using MicaService.Application.Services.Interfaces;
using Quartz;

namespace MicaService.Api.Jobs;

public sealed class SectionCacheRefreshJob(
    ISectionService service,
    ILogger<SectionCacheRefreshJob> logger) : IJob
{
    private readonly ISectionService _service = service;
    private readonly ILogger<SectionCacheRefreshJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var response = await _service.RefreshAsync(context.CancellationToken);
        _logger.LogInformation(
            "Section cache refreshed. Code={Code} Count={Count}",
            response.Code,
            response.Data.Count
        );
    }
}
