namespace ExpenseVista.API.Common.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; } 
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; } 
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
