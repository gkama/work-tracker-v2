using System.ComponentModel.DataAnnotations;
using System.Net;

namespace WorkTracker.Common.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ErrorCode { get; }
        public List<string>? Errors { get; }

        public ApiException(HttpStatusCode statusCode)
            : base($"HTTP {(int)statusCode} - {statusCode}")
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, string message, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public ApiException(HttpStatusCode statusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, List<string> errors)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
