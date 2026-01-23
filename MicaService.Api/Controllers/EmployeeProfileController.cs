using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicaService.Application.Services.Interfaces;
using MicaService.Application.DTOs;

namespace MicaService.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public sealed class EmployeeProfileController(IEmployeeProfileHttpService service) : ControllerBase
{
    private readonly IEmployeeProfileHttpService _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeProfileResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<EmployeeProfileResponseDto>> GetAll([FromQuery] string? deptId)
        => _service.GetAllAsync(deptId);

    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeProfileResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<EmployeeProfileResponseDto>> GetById(string employeeId)
        => _service.GetByIdAsync(employeeId);

    [HttpGet("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeProfileResponseDto>), StatusCodes.Status200OK)]
    public Task<ApiResponseDto<EmployeeProfileResponseDto>> Refresh() => _service.RefreshAsync();
}
