using API_TI.Data;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class AuditService : BaseService, IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly TiPcComponentsContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuditService(
            ILogger<AuditService> logger,
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor
        ) : base(errorService, httpContextAccessor, null)
        {
            _logger = logger;
            _context = context;
            _contextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string eventType, object? data = null, int? userId = null)
        {
            var http = _contextAccessor.HttpContext;
            var ip = http?.Connection?.RemoteIpAddress?.ToString();
            var ua = http?.Request?.Headers["User-Agent"].ToString();

            var entry = new AuditLog
            {
                EventType = eventType,
                EventData = data == null ? null : JsonSerializer.Serialize(data),
                UsuarioId = userId,
                IpAddress = ip,
                UserAgent = ua,
                FechaCreacion = DateTime.UtcNow
            };

            try
            {
                _context.AuditLogs.Add(entry);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for {EventType}", eventType);
            }
        }
    }
}
