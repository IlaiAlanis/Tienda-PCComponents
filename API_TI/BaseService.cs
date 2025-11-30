using API_TI.Models.ApiResponse;
using API_TI.Services.Interfaces;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Abstract
{
    public abstract class BaseService
    {
        protected readonly IErrorService _errorService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IAuditService _auditService;

        protected BaseService(
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService)
        {
            _errorService = errorService;
            _httpContextAccessor = httpContextAccessor;
            _auditService = auditService;

        }
        protected async Task AuditAsync(string eventType, object eventData)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _auditService.LogAsync(
                    eventType,
                    eventData,
                    userId != null ? int.Parse(userId) : (int?)null
                );
            }
            catch (Exception ex)
            {
                // Never let audit failure break business logic
                System.Diagnostics.Debug.WriteLine($"Audit failed: {ex.Message}");
            }
        }
        protected async Task AuditAsync(string eventType, object eventData, int usuarioId)
        {
            try
            {
                await _auditService.LogAsync(eventType, eventData, usuarioId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audit failed: {ex.Message}");
            }
        }

        protected string GetCorrelationId() =>
        _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();

        /// <summary>
        /// Returns a business error response without logging to error_log table.
        /// Use this for expected business validation errors (user not found, invalid input, etc.)
        /// </summary>
        protected async Task<ApiResponse<T>> ReturnErrorAsync<T>(int codigo, string? extraMessage = null)
        {         
            var error = await _errorService.GetErrorByCodeInfoAsync(codigo);         
            var message = extraMessage != null ? $"{error.Mensaje} {extraMessage}" : error.Mensaje;
            return ApiResponse<T>.Fail(error.Codigo, message, error.Severidad, correlationId: GetCorrelationId());
        }

        /// <summary>
        /// Logs a technical exception to the error_log table for debugging.
        /// Use this when catching unexpected exceptions in try-catch blocks.
        /// </summary>
        protected async Task LogTechnicalErrorAsync(int codigo, Exception ex, string? additionalDetail = null)
        {
            try
            {
                var endpoint = _httpContextAccessor.HttpContext?.Request.Path;
                var userId = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var technicalDetail = $"Exception: {ex.Message}\n{ex.StackTrace}";
                if (!string.IsNullOrEmpty(additionalDetail))
                {
                    technicalDetail += $"\nAdditional Context: {additionalDetail}";
                }

                await _errorService.LogErrorAsync(
                    codigo,
                    technicalDetail,
                    endpoint,
                    userId != null ? int.Parse(userId) : (int?)null
                );
            }
            catch (Exception logEx)
            {
                // At minimum, write to debug output so developers can see it
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Failed to log error {codigo}: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs a custom technical detail to error_log without an exception.
        /// Use for edge cases where you need to log technical info without an actual exception.
        /// </summary>
        protected async Task LogCustomTechnicalDetailAsync(int codigo, string detail)
        {
            try
            {
                var endpoint = _httpContextAccessor.HttpContext?.Request.Path;
                var userId = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _errorService.LogErrorAsync(
                    codigo,
                    detail,
                    endpoint,
                    userId != null ? int.Parse(userId) : (int?)null
                );
            }
            catch (Exception logEx)
            {
                // At minimum, write to debug output so developers can see it
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Failed to log error {codigo}: {logEx.Message}");
            }
        }
    }
}
