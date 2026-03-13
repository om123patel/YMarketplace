using Shared.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using Web.API.Models;

namespace Web.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle auth responses AFTER pipeline completes
                // This catches 401/403 set by JwtBearer middleware
                // without any unhandled exception being thrown
                await HandleAuthResponsesAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        // ── Handles unhandled exceptions ────────────────────────
        private static async Task HandleExceptionAsync(
            HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, error, errorCode) = exception switch
            {
                DomainException domainEx =>
                    (HttpStatusCode.UnprocessableEntity,
                     domainEx.Message,
                     domainEx.Code),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized,
                     "Unauthorized access.",
                     "UNAUTHORIZED"),

                ArgumentNullException argEx =>
                    (HttpStatusCode.BadRequest,
                     argEx.Message,
                     "BAD_REQUEST"),

                ArgumentException argEx =>
                    (HttpStatusCode.BadRequest,
                     argEx.Message,
                     "BAD_REQUEST"),

                InvalidOperationException opEx =>
                    (HttpStatusCode.BadRequest,
                     opEx.Message,
                     "INVALID_OPERATION"),

                _ =>
                    (HttpStatusCode.InternalServerError,
                     "An unexpected error occurred. Please try again later.",
                     "INTERNAL_SERVER_ERROR")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse.Fail(
                error, errorCode, (int)statusCode);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, SerializerOptions));
        }

        // ── Handles 401 / 403 set by JwtBearer middleware ──────
        private static async Task HandleAuthResponsesAsync(HttpContext context)
        {
            // Only intercept if response has not started
            // and status is 401 or 403
            if (context.Response.HasStarted)
                return;

            if (context.Response.StatusCode == 401)
            {
                context.Response.ContentType = "application/json";

                // Check if token was expired specifically
                var isExpired = context.Response.Headers
                    .ContainsKey("Token-Expired");

                var response = ApiResponse.Fail(
                    error: isExpired
                        ? "Your session has expired. Please login again."
                        : "Unauthorized. Please provide a valid token.",
                    errorCode: isExpired ? "TOKEN_EXPIRED" : "UNAUTHORIZED",
                    statusCode: 401);

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, SerializerOptions));

                return;
            }

            if (context.Response.StatusCode == 403)
            {
                context.Response.ContentType = "application/json";

                var response = ApiResponse.Fail(
                    error: "Forbidden. You do not have permission to access this resource.",
                    errorCode: "FORBIDDEN",
                    statusCode: 403);

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, SerializerOptions));
            }
        }
    }

}
