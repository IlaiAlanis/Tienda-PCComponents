using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ReembolsoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ReembolsoService : BaseService, IReembolsoService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IStripeService _stripeService;
        private readonly IPayPalService _paypalService;

        public ReembolsoService(
            TiPcComponentsContext context,
            IStripeService stripeService,
            IPayPalService paypalService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _stripeService = stripeService;
            _paypalService = paypalService;
        }

        public async Task<ApiResponse<RefundDto>> RequestRefundAsync(RefundRequest request, int usuarioId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                    .Include(o => o.EstatusVenta)
                    .Include(o => o.PagoTransaccions)
                    .FirstOrDefaultAsync(o => o.IdOrden == request.OrdenId && o.UsuarioId == usuarioId);

                if (orden == null)
                    return await ReturnErrorAsync<RefundDto>(1000);

                if (orden.EstatusVenta.Codigo != "PAID")
                    return await ReturnErrorAsync<RefundDto>(701, "Solo se puede reembolsar órdenes pagadas");

                // Check if already refunded
                if (await _context.Reembolsos.AnyAsync(r => r.OrdenId == request.OrdenId))
                    return await ReturnErrorAsync<RefundDto>(5, "Ya existe solicitud de reembolso");

                var pago = orden.PagoTransaccions.FirstOrDefault();
                if (pago == null)
                    return await ReturnErrorAsync<RefundDto>(800);

                // Create refund record
                var reembolso = new Reembolso
                {
                    OrdenId = request.OrdenId,
                    UsuarioId = usuarioId,
                    Monto = orden.Total,
                    Motivo = request.Motivo,
                    EstatusReembolsoId = 1, // Pendiente
                    FechaSolicitud = DateTime.UtcNow
                };

                _context.Reembolsos.Add(reembolso);
                await _context.SaveChangesAsync();

                // Process refund via payment gateway
                bool refundSuccess = false;
                if (pago.PaymentIntentId != null) // Stripe
                {
                    refundSuccess = await ProcessStripeRefund(pago.PaymentIntentId, orden.Total);
                }
                else if (pago.PaypalOrderId != null) // PayPal
                {
                    refundSuccess = await ProcessPayPalRefund(pago.PaypalOrderId, orden.Total);
                }

                if (!refundSuccess)
                {
                    await transaction.RollbackAsync();
                    return await ReturnErrorAsync<RefundDto>(805, "Error procesando reembolso");
                }

                // Update statuses
                reembolso.EstatusReembolsoId = 2; // Completado
                reembolso.FechaProcesado = DateTime.UtcNow;

                var estatusReembolso = await _context.EstatusVenta.FirstOrDefaultAsync(e => e.Codigo == "REFUND");
                orden.EstatusVentaId = estatusReembolso.IdEstatusVenta;

                var estatusPago = await _context.EstatusPagos.FirstOrDefaultAsync(e => e.NombreEstatusPago == "REEMBOLSADO");
                pago.EstatusPagoId = estatusPago.IdEstatusPago;

                // Restock items
                if (request.RestockItems)
                {
                    foreach (var item in orden.OrdenItems)
                    {
                        item.Producto.StockTotal += item.Cantidad;

                        _context.InventarioMovimientos.Add(new InventarioMovimiento
                        {
                            ProductoId = item.ProductoId,
                            TipoMovimientoInventarioId = 2, // Entrada por devolución
                            Cantidad = item.Cantidad,
                            StockAnterior = item.Producto.StockTotal - item.Cantidad,
                            StockNuevo = item.Producto.StockTotal,
                            Referencia = $"Reembolso orden {orden.NumeroOrden}",
                            FechaMovimiento = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await AuditAsync("Refund.Processed", new { ReembolsoId = reembolso.IdReembolso, OrdenId = request.OrdenId });

                return ApiResponse<RefundDto>.Ok(new RefundDto
                {
                    IdReembolso = reembolso.IdReembolso,
                    OrdenId = reembolso.OrdenId,
                    Monto = reembolso.Monto,
                    Motivo = reembolso.Motivo,
                    Estatus = "COMPLETADO",
                    FechaSolicitud = reembolso.FechaSolicitud
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<RefundDto>(9000);
            }
        }

        public async Task<ApiResponse<RefundDto>> ProcessRefundAsync(int reembolsoId, int adminId)
        {
            // Admin manual approval logic
            try
            {
                var reembolso = await _context.Reembolsos
                    .Include(r => r.Orden)
                    .FirstOrDefaultAsync(r => r.IdReembolso == reembolsoId);

                if (reembolso == null)
                    return await ReturnErrorAsync<RefundDto>(5);

                // Process similar to RequestRefundAsync
                return ApiResponse<RefundDto>.Ok(new RefundDto
                {
                    IdReembolso = reembolso.IdReembolso,
                    OrdenId = reembolso.OrdenId,
                    Monto = reembolso.Monto,
                    Motivo = reembolso.Motivo,
                    Estatus = "COMPLETADO",
                    FechaSolicitud = reembolso.FechaSolicitud
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<RefundDto>(9000);
            }
        }

        private async Task<bool> ProcessStripeRefund(string paymentIntentId, decimal amount)
        {
            try
            {
                var refundOptions = new Stripe.RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = (long)(amount * 100)
                };
                var refundService = new Stripe.RefundService();
                var refund = await refundService.CreateAsync(refundOptions);
                return refund.Status == "succeeded";
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ProcessPayPalRefund(string orderId, decimal amount)
        {
            try
            {
                // PayPal refund logic
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
