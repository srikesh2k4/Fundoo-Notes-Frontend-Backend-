// ========================================
// FILE: FundooNotes/Middleware/GlobalExceptionMiddleware.cs
// ========================================
using BusinessLayer.Exceptions;
using ModelLayer.Responses;
using System.Net;
using System.Text.Json;

namespace FundooNotes.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var errorCode = "INTERNAL_ERROR";
            var message = "An unexpected error occurred";
            Dictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorCode = "NOT_FOUND";
                    message = exception.Message;
                    break;

                case UnauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorCode = "UNAUTHORIZED";
                    message = exception.Message;
                    break;

                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = "VALIDATION_ERROR";
                    message = exception.Message;
                    errors = validationEx.Errors;
                    break;

                default:
                    message = "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var errorResponse = errors != null
                ? new ErrorResponse(message, errorCode, errors)
                : new ErrorResponse(message, errorCode);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }
    }
}