namespace Web.API.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public string? ErrorCode { get; init; }
        public int StatusCode { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public static ApiResponse<T> Ok(T data, int statusCode = 200)
            => new()
            {
                Success = true,
                Data = data,
                StatusCode = statusCode
            };

        public static ApiResponse<T> Fail(
            string error,
            string? errorCode = null,
            int statusCode = 400)
            => new()
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode,
                StatusCode = statusCode
            };
    }

    // Non-generic for commands that return no data
    public class ApiResponse
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public string? ErrorCode { get; init; }
        public int StatusCode { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public static ApiResponse Ok(int statusCode = 200)
            => new() { Success = true, StatusCode = statusCode };

        public static ApiResponse Fail(
            string error,
            string? errorCode = null,
            int statusCode = 400)
            => new()
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode,
                StatusCode = statusCode
            };
    }

}
