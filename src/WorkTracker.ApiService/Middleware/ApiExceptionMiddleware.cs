using Microsoft.AspNetCore.Mvc;
using System.Net;
using WorkTracker.Common.Exceptions;

namespace WorkTracker.ApiService.Middleware
{
    public sealed class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                ApiException ex => ((int)ex.StatusCode, ex.Message),
                _ => ((int)HttpStatusCode.InternalServerError, exception.Message)
            };

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Unhandled exception occurred");
            }
            else
            {
                _logger.LogWarning(exception, "Request failed with status code {StatusCode}", statusCode);
            }

            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Type = $"https://httpstatuses.io/{statusCode}",
                Instance = context.Request.Path
            };

            if (exception is ApiException apiException)
            {
                if (!string.IsNullOrWhiteSpace(apiException.ErrorCode))
                {
                    problemDetails.Extensions["errorCode"] = apiException.ErrorCode;
                }

                if (apiException.Errors?.Count > 0)
                {
                    problemDetails.Extensions["errors"] = apiException.Errors;
                }
            }

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
