using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface IEmployeeExperienceHttpService
{
    Task<ApiResponseDto<EmployeeExperienceResponseDto>> GetByEmployeeIdAsync(string employeeId);
}
