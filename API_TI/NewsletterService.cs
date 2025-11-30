using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class NewsletterService : BaseService, INewsletterService
    {
        private readonly TiPcComponentsContext _context;

        public NewsletterService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> SubscribeAsync(string email)
        {
            try
            {
                var existing = await _context.SuscripcionNewsletters
                    .FirstOrDefaultAsync(n => n.Correo == email);

                if (existing != null)
                {
                    if (existing.EstaActivo)
                        return await ReturnErrorAsync<object>(6, "Ya está suscrito al newsletter");

                    // Reactivate
                    existing.EstaActivo = true;
                    existing.FechaAlta = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return ApiResponse<object>.Ok(null, "Suscripción reactivada");
                }

                _context.SuscripcionNewsletters.Add(new SuscripcionNewsletter
                {
                    Correo = email,
                    EstaActivo = true,
                    FechaAlta = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await AuditAsync("Newsletter.Subscribed", new { Email = email });

                return ApiResponse<object>.Ok(null, "¡Gracias por suscribirte!");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> UnsubscribeAsync(string email)
        {
            try
            {
                var subscription = await _context.SuscripcionNewsletters
                    .FirstOrDefaultAsync(n => n.Correo == email);

                if (subscription == null || !subscription.EstaActivo)
                    return await ReturnErrorAsync<object>(5, "No hay suscripción activa");

                subscription.EstaActivo = false;
                await _context.SaveChangesAsync();

                await AuditAsync("Newsletter.Unsubscribed", new { Email = email });

                return ApiResponse<object>.Ok(null, "Te has desuscrito exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }
    }
}
