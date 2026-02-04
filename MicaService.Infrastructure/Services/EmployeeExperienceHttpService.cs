using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class EmployeeExperienceHttpService(IEmployeeExperienceService service)
    : IEmployeeExperienceHttpService
{
    private readonly IEmployeeExperienceService _service = service;

    public Task<ApiResponseDto<EmployeeExperienceResponseDto>> GetByEmployeeIdAsync(string employeeId)
        => _service.GetByEmployeeIdAsync(employeeId);
}
