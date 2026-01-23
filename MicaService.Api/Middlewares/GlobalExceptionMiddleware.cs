using System.Net;
using System.Text.Json;
using MicaService.Application.Constants;
using MicaService.Application.DTOs;

namespace MicaService.Api.Middlewares;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await Write(
                context,
                HttpStatusCode.InternalServerError,
                ResponseCodes.Error,
                ResponseMessages.InternalServerError
            );
        }
    }

    private static Task Write(HttpContext context, HttpStatusCode status, string code, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var payload = new ApiResponseDto<object>(code, message, new List<object>());
        var json = JsonSerializer.Serialize(payload);
        return context.Response.WriteAsync(json);
    }
}
