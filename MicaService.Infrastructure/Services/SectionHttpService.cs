using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Infrastructure.Services;

public sealed class SectionHttpService(ISectionService service) : ISectionHttpService
{
    private readonly ISectionService _service = service;

    public Task<ApiResponseDto<SectionResponseDto>> GetAllAsync() => _service.GetAllAsync();

    public Task<ApiResponseDto<SectionResponseDto>> RefreshAsync() => _service.RefreshAsync();
}
