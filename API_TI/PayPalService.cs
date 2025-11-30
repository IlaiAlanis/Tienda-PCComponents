namespace API_TI.Services.Implementations
{
    using API_TI.Data;
    using API_TI.Models.ApiResponse;
    using API_TI.Models.dbModels;
    using API_TI.Models.DTOs.PagoDTOs;
    using API_TI.Services.Abstract;
    using API_TI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using PayPalCheckoutSdk.Core;
    using PayPalCheckoutSdk.Orders;

    public class PayPalService : BaseService, IPayPalService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<PayPalService> _logger;
        private readonly PayPalHttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public PayPalService(
            TiPcComponentsContext context,
            ILogger<PayPalService> logger,
            IConfiguration config,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService,
            IEmailService emailService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;

            var clientId = config["Payment:PayPal:ClientId"];
            var secret = config["Payment:PayPal:Secret"];
            var mode = config["Payment:PayPal:Mode"];

            PayPalEnvironment environment = mode == "sandbox"
                ? new SandboxEnvironment(clientId, secret)
                : new LiveEnvironment(clientId, secret);

            _client = new PayPalHttpClient(environment);
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<ApiResponse<PayPalOrderResponse>> CreateOrderAsync(int ordenId, decimal monto)
        {
            try
            {
                var orden = await _context.Ordens.FindAsync(ordenId);
                if (orden == null)
                    return await ReturnErrorAsync<PayPalOrderResponse>(1000);

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        ReferenceId = ordenId.ToString(),
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "MXN",
                            Value = monto.ToString("F2")
                        }
                    }
                },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = "https://yourwebsite.com/payment/success",
                        CancelUrl = "https://yourwebsite.com/payment/cancel"
                    }
                });

                var response = await _client.Execute(request);
                var result = response.Result<Order>();

                var approvalUrl = result.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

                await AuditAsync("Payment.PayPal.OrderCreated", new
                {
                    OrdenId = ordenId,
                    PayPalOrderId = result.Id,
                    Monto = monto
                });

                return ApiResponse<PayPalOrderResponse>.Ok(new PayPalOrderResponse
                {
                    OrderId = result.Id,
                    ApprovalUrl = approvalUrl
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PayPalOrderResponse>(9000);
            }
        }

        public async Task<ApiResponse<PagoTransaccionDto>> CaptureOrderAsync(int ordenId, string paypalOrderId)
        {
            try
            {
                var request = new OrdersCaptureRequest(paypalOrderId);
                request.RequestBody(new OrderActionRequest());

                var response = await _client.Execute(request);
                var result = response.Result<Order>();

                if (result.Status != "COMPLETED")
                    return await ReturnErrorAsync<PagoTransaccionDto>(801, "Pago no completado");

                var orden = await _context.Ordens.FindAsync(ordenId);
                if (orden == null)
                    return await ReturnErrorAsync<PagoTransaccionDto>(1000);

                var metodoPago = await _context.MetodoPagos
                    .FirstOrDefaultAsync(m => m.Tipo == "PAYPAL");

                var estatusPago = await _context.EstatusPagos
                    .FirstOrDefaultAsync(e => e.NombreEstatusPago == "COMPLETADO");

                var capture = result.PurchaseUnits[0].Payments.Captures[0];

                var transaccion = new PagoTransaccion
                {
                    OrdenId = ordenId,
                    MetodoPagoId = metodoPago.IdMetodoPago,
                    EstatusPagoId = estatusPago.IdEstatusPago,
                    Monto = decimal.Parse(capture.Amount.Value),
                    ReferenciaGateway = paypalOrderId,
                    TransaccionGatewayId = capture.Id,
                    PaypalOrderId = paypalOrderId,
                    ClienteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    FechaTransaccion = DateTime.UtcNow
                };

                _context.PagoTransaccions.Add(transaccion);
                await _emailService.SendPaymentConfirmationEmailAsync(
                    orden.Usuario.Correo,
                    orden.NumeroOrden,
                    transaccion.Monto
                );

                var estatusVenta = await _context.EstatusVenta
                    .FirstOrDefaultAsync(e => e.Codigo == "PAID");
                orden.EstatusVentaId = estatusVenta.IdEstatusVenta;

                await _context.SaveChangesAsync();

                await AuditAsync("Payment.PayPal.Captured", new
                {
                    OrdenId = ordenId,
                    TransaccionId = transaccion.IdPago,
                    Monto = transaccion.Monto
                });

                return ApiResponse<PagoTransaccionDto>.Ok(new PagoTransaccionDto
                {
                    IdPago = transaccion.IdPago,
                    OrdenId = ordenId,
                    MetodoPago = "PayPal",
                    Estatus = "COMPLETADO",
                    Monto = transaccion.Monto,
                    ReferenciaGateway = paypalOrderId,
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
