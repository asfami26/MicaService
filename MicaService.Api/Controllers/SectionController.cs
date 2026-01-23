using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicaService.Application.Services.Interfaces;
using MicaService.Application.DTOs;

namespace MicaService.Api.Controllers;

[ApiController]
[Route("api/sections")]
// [Authorize]
public sealed class SectionController(ISectionHttpService service) : ControllerBase
{
    private readonly ISectionHttpService _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<SectionResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<SectionResponseDto>> GetAll() => _service.GetAllAsync();

    [HttpGet("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<SectionResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<SectionResponseDto>> Refresh() => _service.RefreshAsync();
}

