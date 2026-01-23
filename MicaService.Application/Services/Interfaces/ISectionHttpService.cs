using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface ISectionHttpService
{
    Task<ApiResponseDto<SectionResponseDto>> GetAllAsync();
    Task<ApiResponseDto<SectionResponseDto>> RefreshAsync();
}
