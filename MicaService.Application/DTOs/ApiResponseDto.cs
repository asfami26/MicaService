namespace MicaService.Application.DTOs;

public sealed record ApiResponseDto<T>(string Code, string Message, List<T> Data);
