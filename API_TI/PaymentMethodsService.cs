using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.PagoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace API_TI.Services.Implementations
{
    public class PaymentMethodsService : BaseService, IPaymentMethodsService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<PaymentMethodsService> _logger;

        public PaymentMethodsService(
            TiPcComponentsContext context,
            ILogger<PaymentMethodsService> logger,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<IList<PaymentMethodDto>>> GetUserPaymentMethodsAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Getting payment methods for user {UsuarioId}", usuarioId);

                var methods = await _context.UsuarioMetodoPagos
                    .Where(m => m.UsuarioId == usuarioId)
                    .OrderByDescending(m => m.EsPrincipal)
                    .ThenByDescending(m => m.FechaCreacion)
                    .ToListAsync();

                var dtos = methods.Select(m => MapToDto(m)).ToList();

                return ApiResponse<IList<PaymentMethodDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods for user {UsuarioId}", usuarioId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<PaymentMethodDto>>(9000);
            }
        }

        public async Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByIdAsync(int usuarioId, int metodoPagoId)
        {
            try
            {
                var method = await _context.UsuarioMetodoPagos
                    .FirstOrDefaultAsync(m => m.IdMetodoPago == metodoPagoId && m.UsuarioId == usuarioId);

                if (method == null)
                {
                    _logger.LogWarning("Payment method {MetodoPagoId} not found for user {UsuarioId}", metodoPagoId, usuarioId);
                    return await ReturnErrorAsync<PaymentMethodDto>(802, "Método de pago no encontrado");
                }

                return ApiResponse<PaymentMethodDto>.Ok(MapToDto(method));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment method {MetodoPagoId}", metodoPagoId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PaymentMethodDto>(9000);
            }
        }

        public async Task<ApiResponse<PaymentMethodDto>> CreatePaymentMethodAsync(
            int usuarioId,
            CreatePaymentMethodRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating payment method for user {UsuarioId}", usuarioId);

                // Verify user exists
                var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == usuarioId);
                if (!userExists)
                {
                    _logger.LogWarning("User {UsuarioId} not found", usuarioId);
                    return await ReturnErrorAsync<PaymentMethodDto>(203); // Usuario no encontrado
                }

                // Validate request
                var validationError = ValidateCreateRequest(request);
                if (validationError != null)
                {
                    return await ReturnErrorAsync<PaymentMethodDto>(5, validationError);
                }

                UsuarioMetodoPago newMethod;

                // Check if using Stripe token (recommended)
                if (!string.IsNullOrWhiteSpace(request.StripeToken))
                {
                    _logger.LogInformation("Creating payment method with Stripe token");

                    // TODO: Verify token with Stripe API
                    // For now, extract last 4 from token metadata or require separate field

                    newMethod = new UsuarioMetodoPago
                    {
                        UsuarioId = usuarioId,
                        StripePaymentMethodId = request.StripeToken,
                        Last4 = ExtractLast4FromToken(request.StripeToken), // Simplified
                        Marca = "card", // Get from Stripe API
                        Titular = request.Titular,
                        ExpMonth = 12, // Get from Stripe API
                        ExpYear = DateTime.Now.Year + 2, // Get from Stripe API
                        EsPrincipal = request.EsPrincipal,
                        FechaCreacion = DateTime.UtcNow
                    };
                }
                else if (!string.IsNullOrWhiteSpace(request.NumeroTarjeta))
                {
                    // DEPRECATED: Raw card storage (for backward compatibility only)
                    _logger.LogWarning("Creating payment method with raw card data - NOT RECOMMENDED");

                    var cardInfo = ProcessRawCard(request.NumeroTarjeta, request.FechaVencimiento);

                    newMethod = new UsuarioMetodoPago
                    {
                        UsuarioId = usuarioId,
                        Last4 = cardInfo.Last4,
                        Marca = cardInfo.Brand,
                        Titular = request.Titular,
                        ExpMonth = cardInfo.ExpMonth,
                        ExpYear = cardInfo.ExpYear,
                        EsPrincipal = request.EsPrincipal,
                        FechaCreacion = DateTime.UtcNow
                    };

                    // Store encrypted card number (IMPORTANT: Use proper encryption!)
                    // For now, storing only last 4 - DO NOT store full number in production
                    // newMethod.EncryptedCardNumber = Encrypt(request.NumeroTarjeta);
                }
                else
                {
                    return await ReturnErrorAsync<PaymentMethodDto>(5,
                        "Debe proporcionar stripeToken o numeroTarjeta");
                }

                // If setting as primary, unset other primary methods
                if (newMethod.EsPrincipal)
                {
                    await UnsetPrimaryMethodsAsync(usuarioId);
                }

                _context.UsuarioMetodoPagos.Add(newMethod);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("PaymentMethod.Created", new
                {
                    UsuarioId = usuarioId,
                    MetodoPagoId = newMethod.IdMetodoPago,
                    Last4 = newMethod.Last4,
                    UsedToken = !string.IsNullOrWhiteSpace(request.StripeToken)
                }, usuarioId);

                _logger.LogInformation("Successfully created payment method {MetodoPagoId} for user {UsuarioId}",
                    newMethod.IdMetodoPago, usuarioId);

                return ApiResponse<PaymentMethodDto>.Ok(MapToDto(newMethod), "Método de pago agregado");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating payment method for user {UsuarioId}", usuarioId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PaymentMethodDto>(9000);
            }
        }

        public async Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(
            int usuarioId,
            int metodoPagoId,
            UpdatePaymentMethodRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating payment method {MetodoPagoId} for user {UsuarioId}",
                    metodoPagoId, usuarioId);

                var method = await _context.UsuarioMetodoPagos
                    .FirstOrDefaultAsync(m => m.IdMetodoPago == metodoPagoId && m.UsuarioId == usuarioId);

                if (method == null)
                {
                    _logger.LogWarning("Payment method {MetodoPagoId} not found", metodoPagoId);
                    return await ReturnErrorAsync<PaymentMethodDto>(802, "Método de pago no encontrado");
                }

                // Update fields
                if (!string.IsNullOrWhiteSpace(request.Titular))
                {
                    method.Titular = request.Titular;
                }

                if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
                {
                    var expiry = ParseExpiry(request.FechaVencimiento);
                    if (expiry.HasValue)
                    {
                        method.ExpMonth = expiry.Value.Month;
                        method.ExpYear = expiry.Value.Year;
                    }
                }

                if (request.EsPrincipal.HasValue)
                {
                    if (request.EsPrincipal.Value && !method.EsPrincipal)
                    {
                        // Setting as primary - unset others
                        await UnsetPrimaryMethodsAsync(usuarioId);
                        method.EsPrincipal = true;
                    }
                    else if (!request.EsPrincipal.Value && method.EsPrincipal)
                    {
                        // Cannot unset primary if it's the only method
                        var methodCount = await _context.UsuarioMetodoPagos
                            .CountAsync(m => m.UsuarioId == usuarioId);

                        if (methodCount > 1)
                        {
                            method.EsPrincipal = false;
                        }
                        else
                        {
                            return await ReturnErrorAsync<PaymentMethodDto>(5,
                                "No se puede desactivar el único método de pago principal");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("PaymentMethod.Updated", new
                {
                    UsuarioId = usuarioId,
                    MetodoPagoId = metodoPagoId,
                    UpdatedFields = request
                }, usuarioId);

                _logger.LogInformation("Successfully updated payment method {MetodoPagoId}", metodoPagoId);

                return ApiResponse<PaymentMethodDto>.Ok(MapToDto(method), "Método de pago actualizado");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating payment method {MetodoPagoId}", metodoPagoId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PaymentMethodDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeletePaymentMethodAsync(int usuarioId, int metodoPagoId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Deleting payment method {MetodoPagoId} for user {UsuarioId}",
                    metodoPagoId, usuarioId);

                var method = await _context.UsuarioMetodoPagos
                    .FirstOrDefaultAsync(m => m.IdMetodoPago == metodoPagoId && m.UsuarioId == usuarioId);

                if (method == null)
                {
                    _logger.LogWarning("Payment method {MetodoPagoId} not found", metodoPagoId);
                    return await ReturnErrorAsync<object>(802, "Método de pago no encontrado");
                }

                // If deleting primary method and others exist, set another as primary
                if (method.EsPrincipal)
                {
                    var otherMethod = await _context.UsuarioMetodoPagos
                        .Where(m => m.UsuarioId == usuarioId && m.IdMetodoPago != metodoPagoId)
                        .OrderByDescending(m => m.FechaCreacion)
                        .FirstOrDefaultAsync();

                    if (otherMethod != null)
                    {
                        otherMethod.EsPrincipal = true;
                    }
                }

                _context.UsuarioMetodoPagos.Remove(method);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("PaymentMethod.Deleted", new
                {
                    UsuarioId = usuarioId,
                    MetodoPagoId = metodoPagoId,
                    Last4 = method.Last4
                }, usuarioId);

                _logger.LogInformation("Successfully deleted payment method {MetodoPagoId}", metodoPagoId);

                return ApiResponse<object>.Ok(null, "Método de pago eliminado");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting payment method {MetodoPagoId}", metodoPagoId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<PaymentMethodDto>> SetDefaultPaymentMethodAsync(
            int usuarioId,
            int metodoPagoId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Setting payment method {MetodoPagoId} as default for user {UsuarioId}",
                    metodoPagoId, usuarioId);

                var method = await _context.UsuarioMetodoPagos
                    .FirstOrDefaultAsync(m => m.IdMetodoPago == metodoPagoId && m.UsuarioId == usuarioId);

                if (method == null)
                {
                    _logger.LogWarning("Payment method {MetodoPagoId} not found", metodoPagoId);
                    return await ReturnErrorAsync<PaymentMethodDto>(802, "Método de pago no encontrado");
                }

                // Already primary?
                if (method.EsPrincipal)
                {
                    return ApiResponse<PaymentMethodDto>.Ok(MapToDto(method),
                        "Este método de pago ya es el principal");
                }

                // Unset other primary methods
                await UnsetPrimaryMethodsAsync(usuarioId);

                // Set as primary
                method.EsPrincipal = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("PaymentMethod.SetDefault", new
                {
                    UsuarioId = usuarioId,
                    MetodoPagoId = metodoPagoId
                }, usuarioId);

                _logger.LogInformation("Successfully set payment method {MetodoPagoId} as default", metodoPagoId);

                return ApiResponse<PaymentMethodDto>.Ok(MapToDto(method),
                    "Método de pago predeterminado actualizado");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error setting default payment method {MetodoPagoId}", metodoPagoId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PaymentMethodDto>(9000);
            }
        }

        #region Private Helper Methods

        private PaymentMethodDto MapToDto(UsuarioMetodoPago method)
        {
            return new PaymentMethodDto
            {
                IdMetodoPago = method.IdMetodoPago,
                NumeroTarjeta = $"**** **** **** {method.Last4}",
                Ultimos4 = method.Last4,
                Marca = method.Marca,
                Titular = method.Titular,
                FechaVencimiento = $"{method.ExpMonth:00}/{method.ExpYear % 100:00}",
                EsPrincipal = method.EsPrincipal,
                StripePaymentMethodId = method.StripePaymentMethodId,
                FechaCreacion = method.FechaCreacion
            };
        }

        private async Task UnsetPrimaryMethodsAsync(int usuarioId)
        {
            var primaryMethods = await _context.UsuarioMetodoPagos
                .Where(m => m.UsuarioId == usuarioId && m.EsPrincipal)
                .ToListAsync();

            foreach (var method in primaryMethods)
            {
                method.EsPrincipal = false;
            }
        }


        private string ValidateCreateRequest(CreatePaymentMethodRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titular))
            {
                return "El titular es requerido";
            }

            if (string.IsNullOrWhiteSpace(request.StripeToken) &&
                string.IsNullOrWhiteSpace(request.NumeroTarjeta))
            {
                return "Debe proporcionar stripeToken o numeroTarjeta";
            }

            if (!string.IsNullOrWhiteSpace(request.NumeroTarjeta))
            {
                // Validate card number format
                var cleanCard = Regex.Replace(request.NumeroTarjeta, @"\s+", "");
                if (!Regex.IsMatch(cleanCard, @"^\d{13,19}$"))
                {
                    return "Número de tarjeta inválido";
                }

                // Validate expiry
                if (string.IsNullOrWhiteSpace(request.FechaVencimiento))
                {
                    return "Fecha de vencimiento es requerida";
                }

                var expiry = ParseExpiry(request.FechaVencimiento);
                if (!expiry.HasValue)
                {
                    return "Fecha de vencimiento inválida (use MM/YY)";
                }

                if (expiry.Value < DateTime.Now)
                {
                    return "La tarjeta ha expirado";
                }
            }

            return null;
        }

        private (string Last4, string Brand, int ExpMonth, int ExpYear) ProcessRawCard(
            string numeroTarjeta,
            string fechaVencimiento)
        {
            var cleanCard = Regex.Replace(numeroTarjeta, @"\s+", "");
            var last4 = cleanCard.Substring(cleanCard.Length - 4);
            var brand = DetectCardBrand(cleanCard);

            var expiry = ParseExpiry(fechaVencimiento);

            return (last4, brand, expiry?.Month ?? 12, expiry?.Year ?? DateTime.Now.Year + 2);
        }

        private string DetectCardBrand(string cardNumber)
        {
            if (cardNumber.StartsWith("4")) return "visa";
            if (cardNumber.StartsWith("5")) return "mastercard";
            if (cardNumber.StartsWith("3")) return "amex";
            if (cardNumber.StartsWith("6")) return "discover";
            return "card";
        }

        private DateTime? ParseExpiry(string fechaVencimiento)
        {
            if (string.IsNullOrWhiteSpace(fechaVencimiento)) return null;

            var parts = fechaVencimiento.Split('/');
            if (parts.Length != 2) return null;

            if (!int.TryParse(parts[0], out int month) || month < 1 || month > 12)
                return null;

            if (!int.TryParse(parts[1], out int year))
                return null;

            // Handle 2-digit year
            if (year < 100)
            {
                year += 2000;
            }

            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }


        private string ExtractLast4FromToken(string token)
        {
            // TODO: Call Stripe API to get actual last 4
            // For now, return placeholder
            return "0000";
        }



        #endregion
    }
}