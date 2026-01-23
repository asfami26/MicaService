using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class LocationHttpService(ILocationService service) : ILocationHttpService
{
    private readonly ILocationService _service = service;

    public Task<ApiResponseDto<LocationResponseDto>> GetAllAsync() => _service.GetAllAsync();

    public Task<ApiResponseDto<LocationResponseDto>> RefreshAsync() => _service.RefreshAsync();
}
