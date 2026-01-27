using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class EmployeeProfileHttpService(IEmployeeProfileService service)
    : IEmployeeProfileHttpService
{
    private readonly IEmployeeProfileService _service = service;

    public Task<ApiResponseDto<EmployeeProfileResponseDto>> GetAllAsync(string? deptId = null)
        => _service.GetAllAsync(deptId);

    public Task<ApiResponseDto<EmployeeProfileResponseDto>> GetByIdAsync(string employeeId)
        => _service.GetByIdAsync(employeeId);

    public Task<ApiResponseDto<EmployeeProfileResponseDto>> RefreshAsync()
        => _service.RefreshAsync();

    public ApiResponseDto<EmployeeRefreshStatusDto> GetRefreshStatus()
        => _service.GetRefreshStatus();
}
