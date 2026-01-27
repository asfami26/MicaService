namespace MicaService.Application.DTOs;

public sealed record EmployeeProfileResponseDto(
    string EmployeeId,
    string? DepartmentId,
    string? DivisionId,
    string? FullName,
    int Age,
    string? Status,
    string? PositionName,
    string? JgcGrade,
    DateTime? JoinDate,
    string? NoKtp,
    string? CurrentAddress,
    string? Email,
    string? PersonalEmail,
    string? Tax,
    string? Phone,
    string? MaritalStatus,
    string? Education1,
    DateTime? Edu1GradYear,
    string? Edu1Majoring,
    string? Edu1Institute,
    string? Education2,
    DateTime? Edu2GradYear,
    string? Edu2Majoring,
    string? Edu2Institute,
    int Yos,
    int AllExp
);

public sealed record EmployeeRefreshStatusDto(
    bool IsRefreshing,
    DateTimeOffset? StartedAt
);
