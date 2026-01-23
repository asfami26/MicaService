using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface IEmployeeProfileHttpService
{
    Task<ApiResponseDto<EmployeeProfileResponseDto>> GetAllAsync(string? deptId = null);
    Task<ApiResponseDto<EmployeeProfileResponseDto>> GetByIdAsync(string employeeId);
    Task<ApiResponseDto<EmployeeProfileResponseDto>> RefreshAsync();
}
