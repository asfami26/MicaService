using MicaService.Application.Services.Interfaces;
using Quartz;

namespace MicaService.Api.Jobs;

public sealed class EmployeeProfileCacheRefreshJob(
    IEmployeeProfileService service,
    ILogger<EmployeeProfileCacheRefreshJob> logger) : IJob
{
    private readonly IEmployeeProfileService _service = service;
    private readonly ILogger<EmployeeProfileCacheRefreshJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var response = await _service.RefreshAsync(context.CancellationToken);
        _logger.LogInformation(
            "Employee profile cache refreshed. Code={Code} Count={Count}",
            response.Code,
            response.Data.Count
        );
    }
}
