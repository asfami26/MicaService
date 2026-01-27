namespace MicaService.Application.DTOs;

public sealed record RefreshStatusDto(
    bool IsRefreshing,
    DateTimeOffset? StartedAt
);
