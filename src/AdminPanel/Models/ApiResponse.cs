namespace AdminPanel.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public int StatusCode { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public int StatusCode { get; set; }
    }

}
