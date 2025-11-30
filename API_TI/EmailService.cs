using API_TI.Data;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace API_TI.Services.Implementations
{
    public class EmailService : BaseService, IEmailService
    {
        private readonly ILogger<EmailService> _logger;  
        private readonly TiPcComponentsContext _context;
        private readonly IConfiguration _config;

        public EmailService(
             ILogger<EmailService> logger,  
             TiPcComponentsContext context,
             IErrorService errorService,
             IHttpContextAccessor httpContextAccessor,
             IConfiguration config,
             IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

        public async Task SendEmailVerificationAsync(string email, string code)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SendGrid not configured, verification code: {Code}", code);
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"],
                                            _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);
                var subject = "Verificación de correo electrónico";
                var plainText = $"Tu código de verificación es: {code}. Expira en 1 hora.";
                var htmlContent = $@"
                    <h2>Verificación de correo</h2>
                    <p>Tu código de verificación es:</p>
                    <h1 style='color: #4CAF50;'>{code}</h1>
                    <p>Este código expira en 1 hora.</p>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError("SendGrid failed: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                // Don't throw - email failure shouldn't break registration
            }
        }

        public async Task SendPasswordResetAsync(string email, string code)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SendGrid not configured");
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"],
                                            _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);
                var subject = "Recuperación de contraseña";
                var plainText = $"Tu código de recuperación es: {code}. Expira en 15 minutos.";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif;'>
                        <h2>Recuperación de contraseña</h2>
                        <p>Has solicitado restablecer tu contraseña.</p>
                        <p>Tu código de recuperación es:</p>
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #2196F3; margin: 0;'>{code}</h1>
                        </div>
                        <p><strong>Este código expira en 15 minutos.</strong></p>
                        <p style='color: #666; font-size: 12px;'>
                            Si no solicitaste este cambio, ignora este correo.
                        </p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError("SendGrid failed for password reset: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }
        }

        public async Task SendOrderConfirmationEmailAsync(string email, string orderNumber, decimal total)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Confirmación de Orden #{orderNumber}";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4CAF50;'>¡Gracias por tu compra!</h2>
                        <p>Tu orden <strong>#{orderNumber}</strong> ha sido confirmada.</p>
                        <div style='background: #f5f5f5; padding: 20px; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Total:</strong> ${total:N2} MXN</p>
                        </div>
                        <p>Recibirás actualizaciones sobre el estado de tu orden.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email");
            }
        }

        public async Task SendPaymentConfirmationEmailAsync(string email, string orderNumber, decimal amount)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Pago Confirmado - Orden #{orderNumber}";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2196F3;'>Pago Recibido</h2>
                        <p>Hemos recibido tu pago de <strong>${amount:N2} MXN</strong></p>
                        <p>Orden: <strong>#{orderNumber}</strong></p>
                        <p>Tu orden será procesada en breve.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment confirmation email");
            }
        }

        public async Task SendOrderProcessingEmailAsync(string email, string orderNumber)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Orden #{orderNumber} en Proceso";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Tu orden está siendo preparada</h2>
                        <p>Orden: <strong>#{orderNumber}</strong></p>
                        <p>Estamos preparando tu pedido para el envío.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send processing email");
            }
        }

        public async Task SendOrderShippedEmailAsync(string email, string orderNumber, string trackingNumber)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Orden #{orderNumber} Enviada";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4CAF50;'>¡Tu orden va en camino!</h2>
                        <p>Orden: <strong>#{orderNumber}</strong></p>
                        <div style='background: #f5f5f5; padding: 20px; margin: 20px 0;'>
                            <p><strong>Número de seguimiento:</strong></p>
                            <p style='font-size: 18px; color: #2196F3;'>{trackingNumber}</p>
                        </div>
                        <p>Recibirás tu pedido pronto.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send shipping email");
            }
        }

        public async Task SendOrderDeliveredEmailAsync(string email, string orderNumber)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Orden #{orderNumber} Entregada";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4CAF50;'>Tu orden fue entregada</h2>
                        <p>Orden: <strong>#{orderNumber}</strong></p>
                        <p>Esperamos que disfrutes tu compra.</p>
                        <p>¿Te gustaría dejar una reseña?</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send delivery email");
            }
        }

        public async Task SendOrderCancelledEmailAsync(string email, string orderNumber)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Orden #{orderNumber} Cancelada";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Orden Cancelada</h2>
                        <p>Tu orden <strong>#{orderNumber}</strong> ha sido cancelada.</p>
                        <p>Si tienes preguntas, contáctanos.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation email");
            }
        }

        public async Task SendRefundConfirmationEmailAsync(string email, string orderNumber, decimal amount)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return;

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email);

                var subject = $"Reembolso Procesado - Orden #{orderNumber}";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Reembolso Confirmado</h2>
                        <p>Tu reembolso de <strong>${amount:N2} MXN</strong> ha sido procesado.</p>
                        <p>Orden: <strong>#{orderNumber}</strong></p>
                        <p>Los fondos deberían reflejarse en 5-10 días hábiles.</p>
                    </div>
                ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send refund email");
            }
        }
        /// <summary>
        /// Sends notification to admin when a contact form is submitted
        /// </summary>
        public async Task SendContactNotificationToAdminAsync(string userName, string userEmail, string motivo, string mensaje)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SendGrid not configured, contact message from: {Email}", userEmail);
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);

                // Send to admin email (configure in appsettings.json)
                var adminEmail = _config["Email:AdminEmail"] ?? _config["Email:SendGrid:FromEmail"];
                var to = new EmailAddress(adminEmail, "Admin");

                var subject = $"Nuevo mensaje de contacto - {motivo}";
                var plainText = $@"
            Nuevo mensaje de contacto recibido:
            
            De: {userName} ({userEmail})
            Motivo: {motivo}
            
            Mensaje:
            {mensaje}
        ";

                var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2196F3;'>Nuevo Mensaje de Contacto</h2>
                
                <div style='background: #f5f5f5; padding: 20px; margin: 20px 0; border-left: 4px solid #2196F3;'>
                    <p><strong>De:</strong> {userName}</p>
                    <p><strong>Email:</strong> <a href='mailto:{userEmail}'>{userEmail}</a></p>
                    <p><strong>Motivo:</strong> {motivo}</p>
                </div>
                
                <div style='background: #fff; padding: 20px; border: 1px solid #ddd;'>
                    <h3 style='margin-top: 0;'>Mensaje:</h3>
                    <p style='white-space: pre-wrap;'>{mensaje}</p>
                </div>
                
                <div style='margin-top: 20px; padding: 15px; background: #fff3cd; border: 1px solid #ffc107;'>
                    <p style='margin: 0;'><strong>⚠️ Acción requerida:</strong> Responde a este cliente lo antes posible.</p>
                </div>
            </div>
        ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError("SendGrid failed for contact notification: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact notification to admin");
                // Don't throw - email failure shouldn't break contact form submission
            }
        }

        /// <summary>
        /// Sends confirmation email to user after submitting contact form
        /// </summary>
        public async Task SendContactConfirmationAsync(string email, string userName)
        {
            try
            {
                var apiKey = _config["Email:SendGrid:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SendGrid not configured, cannot send contact confirmation to: {Email}", email);
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_config["Email:SendGrid:FromEmail"], _config["Email:SendGrid:FromName"]);
                var to = new EmailAddress(email, userName);

                var subject = "Hemos recibido tu mensaje - PC Components";
                var plainText = $@"
            Hola {userName},
            
            Hemos recibido tu mensaje y nuestro equipo lo revisará pronto.
            
            Te responderemos a la brevedad posible.
            
            Gracias por contactarnos,
            Equipo de PC Components
        ";

                var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>PC Components</h1>
                </div>
                
                <div style='padding: 30px; background: #fff;'>
                    <h2 style='color: #333;'>¡Gracias por contactarnos, {userName}!</h2>
                    
                    <p style='color: #555; line-height: 1.6;'>
                        Hemos recibido tu mensaje y nuestro equipo lo revisará pronto.
                    </p>
                    
                    <div style='background: #f0f7ff; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #2196F3;'>
                        <p style='margin: 0; color: #333;'>
                            <strong>📧 Te responderemos lo antes posible</strong>
                        </p>
                        <p style='margin: 10px 0 0 0; color: #666; font-size: 14px;'>
                            Normalmente respondemos en menos de 24 horas hábiles.
                        </p>
                    </div>
                    
                    <p style='color: #555;'>
                        Mientras tanto, puedes:
                    </p>
                    
                    <ul style='color: #555; line-height: 1.8;'>
                        <li>Revisar nuestras <a href='#' style='color: #2196F3;'>Preguntas Frecuentes</a></li>
                        <li>Explorar nuestro <a href='#' style='color: #2196F3;'>Catálogo de Productos</a></li>
                        <li>Ver el <a href='#' style='color: #2196F3;'>Estado de tu Pedido</a></li>
                    </ul>
                    
                    <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                        Este es un correo automático, por favor no respondas a este mensaje.
                    </p>
                </div>
                
                <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #999;'>
                    <p style='margin: 0;'>© 2024 PC Components. Todos los derechos reservados.</p>
                </div>
            </div>
        ";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError("SendGrid failed for contact confirmation: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact confirmation email to {Email}", email);
                // Don't throw - email failure shouldn't break contact form submission
            }
        }

    }
}
