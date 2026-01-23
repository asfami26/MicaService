namespace MicaService.Application.DTOs;

public sealed record SectionResponseDto(
    string SectId,
    string? SectName,
    string? DeptId,
    string? MgrEmpId
);
