namespace API_TI.Services.Implementations
{
    using API_TI.Data;
    using API_TI.Models.ApiResponse;
    using API_TI.Models.dbModels;
    using API_TI.Models.DTOs.PagoDTOs;
    using API_TI.Services.Abstract;
    using API_TI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    // Services/Implementations/StripeService.cs
    using Stripe;

    public class StripeService : BaseService, IStripeService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<StripeService> _logger;
        private readonly string _secretKey;
        private readonly string _publicKey;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public StripeService(
            TiPcComponentsContext context,
            ILogger<StripeService> logger,
            IConfiguration config,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService,
            IEmailService emailService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
            _secretKey = config["Payment:Stripe:SecretKey"];
            _publicKey = config["Payment:Stripe:PublicKey"];
            StripeConfiguration.ApiKey = _secretKey;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<ApiResponse<PaymentIntentResponse>> CreatePaymentIntentAsync(int ordenId, decimal monto)
        {
            try
            {
                var orden = await _context.Ordens.FindAsync(ordenId);
                if (orden == null)
                    return await ReturnErrorAsync<PaymentIntentResponse>(1000);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(monto * 100), // Convert to cents
                    Currency = "mxn",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "orden_id", ordenId.ToString() },
                        { "numero_orden", orden.NumeroOrden }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                await AuditAsync("Payment.Stripe.IntentCreated", new
                {
                    OrdenId = ordenId,
                    PaymentIntentId = paymentIntent.Id,
                    Monto = monto
                });

                return ApiResponse<PaymentIntentResponse>.Ok(new PaymentIntentResponse
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id,
                    PublicKey = _publicKey
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                await LogTechnicalErrorAsync(806, ex);
                return await ReturnErrorAsync<PaymentIntentResponse>(806);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PaymentIntentResponse>(9000);
            }
        }

        public async Task<ApiResponse<PagoTransaccionDto>> ConfirmPaymentAsync(int ordenId, string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                if (paymentIntent.Status != "succeeded")
                    return await ReturnErrorAsync<PagoTransaccionDto>(801, "Pago no completado");

                var orden = await _context.Ordens
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId);
                
                if (orden == null)
                    return await ReturnErrorAsync<PagoTransaccionDto>(1000);

                // Get Stripe payment method ID
                var metodoPago = await _context.MetodoPagos
                    .FirstOrDefaultAsync(m => m.Tipo == "CARD");

                // Get payment status
                var estatusPago = await _context.EstatusPagos
                    .FirstOrDefaultAsync(e => e.NombreEstatusPago == "COMPLETADO");

                var transaccion = new PagoTransaccion
                {
                    OrdenId = ordenId,
                    MetodoPagoId = metodoPago.IdMetodoPago,
                    EstatusPagoId = estatusPago.IdEstatusPago,
                    Monto = paymentIntent.Amount / 100m,
                    ReferenciaGateway = paymentIntentId,
                    TransaccionGatewayId = paymentIntent.Id,
                    PaymentIntentId = paymentIntentId,
                    ClienteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    RespuestaGateway = paymentIntent.ToJson(),
                    FechaTransaccion = DateTime.UtcNow
                };

                _context.PagoTransaccions.Add(transaccion);
                await _emailService.SendPaymentConfirmationEmailAsync(
                    orden.Usuario.Correo,
                    orden.NumeroOrden,
                    transaccion.Monto
                );
                // Update order status
                var estatusVenta = await _context.EstatusVenta
                    .FirstOrDefaultAsync(e => e.Codigo == "PAID");
                orden.EstatusVentaId = estatusVenta.IdEstatusVenta;

                await _context.SaveChangesAsync();

                await AuditAsync("Payment.Stripe.Confirmed", new
                {
                    OrdenId = ordenId,
                    TransaccionId = transaccion.IdPago,
                    Monto = transaccion.Monto
                });

                return ApiResponse<PagoTransaccionDto>.Ok(new PagoTransaccionDto
                {
                    IdPago = transaccion.IdPago,
                    OrdenId = ordenId,
                    MetodoPago = "Stripe",
                    Estatus = "COMPLETADO",
                    Monto = transaccion.Monto,
                    ReferenciaGateway = paymentIntentId,
                    FechaTransaccion = transaccion.FechaTransaccion
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagoTransaccionDto>(9000);
            }
        }
    }
}
