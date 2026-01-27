using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface IEmployeeProfileService
{
    Task<ApiResponseDto<EmployeeProfileResponseDto>> GetAllAsync(
        string? deptId = null,
        CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeProfileResponseDto>> GetByIdAsync(
        string employeeId,
        CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeProfileResponseDto>> RefreshAsync(CancellationToken cancellationToken = default);
    ApiResponseDto<EmployeeRefreshStatusDto> GetRefreshStatus();
}
