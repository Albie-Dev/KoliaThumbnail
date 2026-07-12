using System.Text.Json;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models;

namespace Kolia.Thumbnail.API.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, ex.Message);

            await WriteResponseAsync(
                context,
                ex.StatusCode,
                ex.Code,
                ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var response = new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "Dữ liệu đầu vào không hợp lệ.",
                TraceId = context.TraceIdentifier,
                Errors = ex.Errors
                .Select(x => new ValidationError
                {
                    Property = char.ToLowerInvariant(x.PropertyName[0]) + x.PropertyName[1..],
                    Message = x.ErrorMessage,
                    ErrorCode = x.ErrorCode
                })
                .ToArray()
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            await WriteResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}