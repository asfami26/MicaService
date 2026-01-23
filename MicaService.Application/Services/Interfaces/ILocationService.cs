using MicaService.Application.DTOs;

namespace MicaService.Application.Services.Interfaces;

public interface ILocationService
{
    Task<ApiResponseDto<LocationResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseDto<LocationResponseDto>> RefreshAsync(CancellationToken cancellationToken = default);
}
