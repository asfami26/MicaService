using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicaService.Application.DTOs;
using MicaService.Application.Services.Interfaces;

namespace MicaService.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/locations")]
// [Authorize]
public sealed class LocationController(ILocationHttpService service) : ControllerBase
{
    private readonly ILocationHttpService _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<LocationResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<LocationResponseDto>> GetAll() => _service.GetAllAsync();

    [HttpGet("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<LocationResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<LocationResponseDto>> Refresh() => _service.RefreshAsync();
}
