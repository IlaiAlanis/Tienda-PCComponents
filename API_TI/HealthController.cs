using API_TI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            TiPcComponentsContext context,
            ILogger<HealthController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "TI_PC_Components_API"
            });
        }

        [HttpGet("ready")]
        [AllowAnonymous]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Check database
                await _context.Database.CanConnectAsync();

                return Ok(new
                {
                    status = "ready",
                    database = "connected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unavailable",
                    database = "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("detailed")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Detailed()
        {
            var checks = new Dictionary<string, object>();

            // Database
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                checks["database"] = new { status = canConnect ? "healthy" : "unhealthy" };
            }
            catch (Exception ex)
            {
                checks["database"] = new { status = "unhealthy", error = ex.Message };
            }

            // Memory
            var memoryMB = GC.GetTotalMemory(false) / 1024 / 1024;
            checks["memory"] = new { allocatedMB = memoryMB };

            // Uptime
            var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
            checks["uptime"] = new { hours = uptime.TotalHours };

            return Ok(new
            {
                status = "healthy",
                checks,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
