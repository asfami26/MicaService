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
    string? Education1,
    DateTime? Edu1GradYear,
    string? Edu1Majoring,
    string? Education2,
    DateTime? Edu2GradYear,
    string? Edu2Majoring,
    int Yos,
    int AllExp
);
