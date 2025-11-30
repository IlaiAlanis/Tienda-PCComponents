namespace API_TI.Models.ApiResponse
{
    public class ApiError
    {
        public int Code { get; set; }
        public string Message { get; set; } = null!;
        public string? Detail { get; set; }
        public int Severity { get; set; } = 3;
    }
}
