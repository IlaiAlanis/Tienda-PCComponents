using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ErrorDTOs;
using API_TI.Services.Interfaces;
using System.Text.Json;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Aquí los servicios scoped se obtienen correctamente
        var errorService = context.RequestServices.GetRequiredService<IErrorService>();
        var auditService = context.RequestServices.GetRequiredService<IAuditService>();

        var correlationId = context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var endpoint = context.Request.Path;
            var ip = context.Connection?.RemoteIpAddress?.ToString();
            var ua = context.Request.Headers["User-Agent"].ToString();
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            int? userIdParsed = null;
            if (userId != null && int.TryParse(userId, out var parsed))
                userIdParsed = parsed;

            _logger.LogError(ex, "Unhandled exception {@Meta}", new
            {
                CorrelationId = correlationId,
                Endpoint = endpoint,
                Ip = ip,
                UserAgent = ua,
                UserId = userId
            });

            // Guardar en DB
            try
            {
                var technicalDetail = $"Exception: {ex.Message}\n{ex.StackTrace}";
                await errorService.LogErrorAsync(9000, technicalDetail, endpoint, userIdParsed);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write error log to DB");
            }

            try
            {
                await auditService.LogAsync("System.Exception", new
                {
                    CorrelationId = correlationId,
                    Endpoint = endpoint,
                    Error = ex.Message
                }, userIdParsed);
            }
            catch { }

            var errorInfo = await errorService.GetErrorByCodeInfoAsync(9000)
                ?? new ErrorInfoDto { Codigo = 9000, Mensaje = "Error interno del servidor.", Severidad = 3 };

            var apiResponse = ApiResponse<object>.Fail(errorInfo.Codigo, errorInfo.Mensaje, errorInfo.Severidad);
            apiResponse.CorrelationId = correlationId;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(apiResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
