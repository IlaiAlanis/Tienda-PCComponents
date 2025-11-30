namespace API_TI.Models.ApiResponse
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }
        public string? CorrelationId { get; set; }

        // Success factory
        public static ApiResponse<T> Ok(T data, string? correlationId = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Error = null,
                CorrelationId = correlationId // Let caller set it
            };
        }


        // Fail factory: use Error object (no technical details here)
        public static ApiResponse<T> Fail(int code, string message, int severity = 3,
            string? detail = null, string? correlationId = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    Detail = detail == null ? null : SanitizeClientDetail(detail),
                    Severity = severity
                },
                CorrelationId = correlationId
            };
        }

        // Ensure we don't leak sensitive text to clients
        private static string SanitizeClientDetail(string d)
        {
            // Very small sanitizer: remove newlines and stack traces style pieces.
            // Keep minimal, non-technical info. Advanced sanitizer can be injected.
            return d.Replace("\r", " ").Replace("\n", " ").Trim();
        }


    }
}
