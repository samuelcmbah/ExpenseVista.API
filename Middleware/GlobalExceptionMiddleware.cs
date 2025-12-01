using ExpenseVista.API.Common.Exceptions;
using ExpenseVista.API.Common.Models;
using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, errorCode, message) = MapException(ex);

        _logger.LogError(ex, "Unhandled exception: {Message} | TraceId: {TraceId}", ex.Message, traceId);

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            ErrorCode = errorCode,
            TraceId = traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);

        await context.Response.WriteAsync(json);
    }

    private static (int statusCode, string? errorCode, string message) MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException notFound =>
                ((int)HttpStatusCode.NotFound, "ERR_NOT_FOUND", notFound.Message),

            BadRequestException badRequest =>
                ((int)HttpStatusCode.BadRequest, "ERR_BAD_REQUEST", badRequest.Message),

            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized,
                    "ERR_UNAUTHORIZED",
                    "You are not authorized to perform this action."),

            _ => ((int)HttpStatusCode.InternalServerError,
                "ERR_UNEXPECTED",
                "An unexpected error occurred. Please try again later.")
        };
    }

}
