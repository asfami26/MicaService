using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface ISectionService
{
    Task<ApiResponseDto<SectionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseDto<SectionResponseDto>> RefreshAsync(CancellationToken cancellationToken = default);
}
