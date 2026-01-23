using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface ILocationHttpService
{
    Task<ApiResponseDto<LocationResponseDto>> GetAllAsync();
    Task<ApiResponseDto<LocationResponseDto>> RefreshAsync();
}
