using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ContactoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;

namespace API_TI.Services.Implementations
{
    public class ContactService : BaseService, IContactService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IEmailService _emailService;

        public ContactService(
            TiPcComponentsContext context,
            IEmailService emailService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<ApiResponse<object>> SendContactMessageAsync(ContactRequest request)
        {
            try
            {
                // Validate email format
                if (!IsValidEmail(request.Email))
                    return await ReturnErrorAsync<object>(3, "Formato de email inválido");

                // Create contact message record
                var contacto = new Contacto
                {
                    Nombre = request.Nombre,
                    Email = request.Email,
                    Motivo = request.Motivo,
                    Mensaje = request.Mensaje,
                    FechaCreacion = DateTime.UtcNow,
                    Leido = false
                };

                _context.Contactos.Add(contacto);
                await _context.SaveChangesAsync();

                // Send email notification to admin
                await _emailService.SendContactNotificationToAdminAsync(
                    request.Nombre,
                    request.Email,
                    request.Motivo,
                    request.Mensaje
                );

                // Send confirmation email to user
                await _emailService.SendContactConfirmationAsync(
                    request.Email,
                    request.Nombre
                );

                return ApiResponse<object>.Ok(new
                {
                    Message = "Mensaje enviado correctamente. Te contactaremos pronto."
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000, "Error al enviar mensaje");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
