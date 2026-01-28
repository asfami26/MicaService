using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicaService.Application.Services.Interfaces;
using MicaService.Application.DTOs;
using System.IO;
using System.Text.RegularExpressions;

namespace MicaService.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/employees")]
// [Authorize]
public sealed class EmployeeProfileController(
    IEmployeeProfileHttpService service,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IEmployeeProfileHttpService _service = service;
    private readonly IConfiguration _configuration = configuration;

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

    [HttpGet("refresh/status")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeRefreshStatusDto>), StatusCodes.Status200OK)]
    public ApiResponseDto<EmployeeRefreshStatusDto> RefreshStatus() => _service.GetRefreshStatus();

    [HttpGet("{employeeId}/photo")]
    [Produces("image/jpeg")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult GetPhoto(string employeeId)
    {
        var basePath = _configuration["EmployeePhoto:BasePath"];
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return NotFound();
        }

        var safeId = Regex.Replace(employeeId ?? string.Empty, @"[^0-9A-Za-z_-]", "");
        if (string.IsNullOrWhiteSpace(safeId))
        {
            return NotFound();
        }

        var extensions = new[] { ".jpg", ".JPG", ".jpeg", ".JPEG" };
        foreach (var ext in extensions)
        {
            var filePath = Path.Combine(basePath, $"{safeId}{ext}");
            if (System.IO.File.Exists(filePath))
            {
                return PhysicalFile(filePath, "image/jpeg");
            }
        }

        return NotFound();
    }
}
