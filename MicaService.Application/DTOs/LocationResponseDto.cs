namespace MicaService.Application.DTOs;

public sealed record LocationResponseDto(
    string LocId,
    string? LocName,
    string? Country
);
