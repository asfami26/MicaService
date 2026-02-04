namespace MicaService.Application.DTOs;

public sealed record EmployeeExperienceResponseDto(
    string EmployeeId,
    string? Company,
    string? Position,
    DateTime? StartDate,
    DateTime? EndDate
);
