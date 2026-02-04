using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface IEmployeeExperienceService
{
    Task<ApiResponseDto<EmployeeExperienceResponseDto>> GetByEmployeeIdAsync(
        string employeeId,
        CancellationToken cancellationToken = default);
}
