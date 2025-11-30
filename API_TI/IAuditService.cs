namespace API_TI.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string eventType, object? data = null, int? userId = null);
    }
}
