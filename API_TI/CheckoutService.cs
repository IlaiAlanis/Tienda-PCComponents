using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Models.DTOs.CotizacionDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Models.DTOs.ImpuestoDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Models.DTOs.PagoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace API_TI.Services.Implementations
{
    public class CheckoutService : BaseService, ICheckoutService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ICarritoService _carritoService;
        private readonly IShippingService _shippingService;
        private readonly IImpuestoService _impuestoService;
        private readonly IStripeService _stripeService;
        private readonly IPayPalService _paypalService;
        private readonly IEmailService _emailService;

        public CheckoutService(
            TiPcComponentsContext context,
            ICarritoService carritoService,
            IShippingService shippingService,
            IImpuestoService impuestoService,
            IStripeService stripeService,
            IPayPalService paypalService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService,
            IEmailService emailService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _carritoService = carritoService;
            _shippingService = shippingService;
            _impuestoService = impuestoService;
            _stripeService = stripeService;
            _paypalService = paypalService;
            _emailService = emailService; 
        }

        public async Task<ApiResponse<CheckoutSummaryDto>> GetCheckoutSummaryAsync(int usuarioId, int direccionEnvioId)
        {
            try
            {
                // Get cart
                var carritoResponse = await _carritoService.GetCartAsync(usuarioId);
                if (!carritoResponse.Success || !carritoResponse.Data.Items.Any())
                    return await ReturnErrorAsync<CheckoutSummaryDto>(1301, "Carrito vacío");

                // Validate address
                var direccion = await _context.Direccions
                    .FirstOrDefaultAsync(d => d.IdDireccion == direccionEnvioId && d.UsuarioId == usuarioId);

                if (direccion == null)
                    return await ReturnErrorAsync<CheckoutSummaryDto>(902);

                // Get shipping options
                var pesoTotal = await CalculateTotalWeightAsync(carritoResponse.Data.Items);
                var shippingResponse = await _shippingService.GetShippingQuotesAsync(usuarioId, new CotizarEnvioRequest
                {
                    DireccionId = direccionEnvioId,
                    PesoKg = pesoTotal
                });

                // Get applicable tax
                var impuesto = await _impuestoService.GetApplicableTaxAsync(direccionEnvioId);
                var subtotalConDescuento = carritoResponse.Data.Subtotal - carritoResponse.Data.DescuentoTotal;
                var impuestoTotal = await _impuestoService.CalculateTaxAsync(subtotalConDescuento, direccionEnvioId);

                // Select cheapest shipping
                var envioMasBarato = shippingResponse.Data?.Cotizaciones?.OrderBy(c => c.Costo).FirstOrDefault();
                var costoEnvio = envioMasBarato?.Costo ?? 0;

                var summary = new CheckoutSummaryDto
                {
                    Carrito = carritoResponse.Data,
                    DireccionEnvio = MapToDireccionDto(direccion),
                    OpcionesEnvio = shippingResponse.Data?.Cotizaciones,
                    Impuesto = impuesto != null ? new ImpuestoDto
                    {
                        IdImpuesto = impuesto.IdImpuesto,
                        Nombre = impuesto.Nombre,
                        Codigo = impuesto.Codigo,
                        Valor = impuesto.Valor,
                        Tipo = impuesto.Tipo
                    } : null,
                    Costos = new ResumenCostosDto
                    {
                        Subtotal = carritoResponse.Data.Subtotal,
                        DescuentoTotal = carritoResponse.Data.DescuentoTotal,
                        SubtotalConDescuento = subtotalConDescuento,
                        Impuesto = impuestoTotal,
                        Envio = costoEnvio,
                        Total = subtotalConDescuento + impuestoTotal + costoEnvio
                    }
                };

                return ApiResponse<CheckoutSummaryDto>.Ok(summary);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CheckoutSummaryDto>(9000);
            }
        }

        public async Task<ApiResponse<OrdenDto>> ConfirmCheckoutAsync(int usuarioId, ConfirmCheckoutRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validate cart
                var carrito = await _context.Carritos
                    .Include(c => c.CarritoItems)
                        .ThenInclude(i => i.Producto)
                    .Include(c => c.CarritoDescuentos)
                        .ThenInclude(cd => cd.Descuento)
                    .Include(c => c.CarritoDescuentos)
                        .ThenInclude(cd => cd.ReglaDescuento)
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.EstatusVentaId == 1);

                if (carrito == null || !carrito.CarritoItems.Any())
                    return await ReturnErrorAsync<OrdenDto>(1301);

                // 2. Validate stock
                foreach (var item in carrito.CarritoItems)
                {
                    if (item.Producto.StockTotal < item.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return await ReturnErrorAsync<OrdenDto>(2007, $"Stock insuficiente para {item.Producto.NombreProducto}");
                    }
                }

                // 3. Validate address
                var direccion = await _context.Direccions
                    .FirstOrDefaultAsync(d => d.IdDireccion == request.DireccionEnvioId && d.UsuarioId == usuarioId);

                if (direccion == null)
                {
                    await transaction.RollbackAsync();
                    return await ReturnErrorAsync<OrdenDto>(902);
                }

                // 4. Calculate totals
                var subtotal = carrito.CarritoItems.Sum(i => i.PrecioUnitario * i.Cantidad);
                var descuentoTotal = carrito.CarritoDescuentos.Sum(cd => cd.MontoAplicado);
                var subtotalConDescuento = subtotal - descuentoTotal;

                var impuesto = await _impuestoService.GetApplicableTaxAsync(request.DireccionEnvioId);
                var impuestoTotal = await _impuestoService.CalculateTaxAsync(subtotalConDescuento, request.DireccionEnvioId);

                var pesoTotal = await CalculateTotalWeightAsync(carrito.CarritoItems);
                var shippingResponse = await _shippingService.GetShippingQuotesAsync(usuarioId, new CotizarEnvioRequest
                {
                    DireccionId = request.DireccionEnvioId,
                    PesoKg = pesoTotal
                });
                var costoEnvio = shippingResponse.Data?.Cotizaciones?.OrderBy(c => c.Costo).FirstOrDefault()?.Costo ?? 0;

                var total = subtotalConDescuento + impuestoTotal + costoEnvio;

                // 5. Create order
                var numeroOrden = GenerateOrderNumber();
                var orden = new Orden
                {
                    UsuarioId = usuarioId,
                    ImpuestoId = impuesto?.IdImpuesto ?? 1,
                    EstatusVentaId = 2, // Pending payment
                    DireccionEnvioId = request.DireccionEnvioId,
                    NumeroOrden = numeroOrden,
                    FechaOrden = DateTime.UtcNow,
                    Subtotal = subtotal,
                    CostoEnvio = costoEnvio,
                    DescuentoTotal = descuentoTotal,
                    ImpuestoTotal = impuestoTotal,
                    Total = total
                };

                _context.Ordens.Add(orden);
                await _context.SaveChangesAsync();

                var usuario = await _context.Usuarios.FindAsync(usuarioId);

                await _emailService.SendOrderConfirmationEmailAsync(
                    usuario.Correo,
                    orden.NumeroOrden,
                    orden.Total
                );

                // 6. Create order items
                foreach (var cartItem in carrito.CarritoItems)
                {
                    var ordenItem = new OrdenItem
                    {
                        OrdenId = orden.IdOrden,
                        ProductoId = cartItem.ProductoId,
                        Cantidad = cartItem.Cantidad,
                        PrecioUnitario = cartItem.PrecioUnitario,
                        DescuentoAplicado = cartItem.DescuentoAplicado
                    };
                    _context.OrdenItems.Add(ordenItem);
                }

                // 7. Copy discounts
                foreach (var cartDiscount in carrito.CarritoDescuentos)
                {
                    var ordenDescuento = new OrdenDescuento
                    {
                        OrdenId = orden.IdOrden,
                        DescuentoId = cartDiscount.DescuentoId,
                        MontoAplicado = cartDiscount.MontoAplicado
                    };
                    _context.OrdenDescuentos.Add(ordenDescuento);

                    // Record discount usage
                    var descuentoUso = new DescuentoUso
                    {
                        DescuentoId = cartDiscount.DescuentoId,
                        UsuarioId = usuarioId,
                        OrdenId = orden.IdOrden,
                        CodigoCupon = cartDiscount.ReglaDescuento?.CodigoCupon,
                        MontoDescuento = cartDiscount.MontoAplicado,
                        FechaUso = DateTime.UtcNow
                    };
                    _context.DescuentoUsos.Add(descuentoUso);

                    // Increment usage counter
                    if (cartDiscount.ReglaDescuento != null)
                    {
                        cartDiscount.ReglaDescuento.UsosActuales++;
                    }
                }

                await _context.SaveChangesAsync();

                // 8. Process payment
                ApiResponse<PagoTransaccionDto> paymentResponse;
                if (request.MetodoPago.ToLower() == "stripe")
                {
                    if (string.IsNullOrWhiteSpace(request.PaymentIntentId))
                    {
                        await transaction.RollbackAsync();
                        return await ReturnErrorAsync<OrdenDto>(801, "PaymentIntentId requerido");
                    }
                    paymentResponse = await _stripeService.ConfirmPaymentAsync(orden.IdOrden, request.PaymentIntentId);
                }
                else if (request.MetodoPago.ToLower() == "paypal")
                {
                    if (string.IsNullOrWhiteSpace(request.PayPalOrderId))
                    {
                        await transaction.RollbackAsync();
                        return await ReturnErrorAsync<OrdenDto>(801, "PayPalOrderId requerido");
                    }
                    paymentResponse = await _paypalService.CaptureOrderAsync(orden.IdOrden, request.PayPalOrderId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    return await ReturnErrorAsync<OrdenDto>(802, "Método de pago inválido");
                }

                if (!paymentResponse.Success)
                {
                    await transaction.RollbackAsync();
                    return await ReturnErrorAsync<OrdenDto>(paymentResponse.Error.Code, paymentResponse.Error.Message);
                }

                // 9. Update order status
                var estatusPagado = await _context.EstatusVenta.FirstOrDefaultAsync(e => e.Codigo == "PAID");
                orden.EstatusVentaId = estatusPagado?.IdEstatusVenta ?? 4;
                orden.ReferenciaMetodoPago = paymentResponse.Data.ReferenciaGateway;

                // 10. Update inventory
                foreach (var item in carrito.CarritoItems)
                {
                    item.Producto.StockTotal -= item.Cantidad;
                    
                    // Record inventory movement
                    var movimiento = new InventarioMovimiento
                    {
                        ProductoId = item.ProductoId,
                        TipoMovimientoInventarioId = 1,
                        Cantidad = -item.Cantidad,
                        StockAnterior = item.Producto.StockTotal + item.Cantidad,
                        StockNuevo = item.Producto.StockTotal,
                        Referencia = $"Orden {numeroOrden}",
                        FechaMovimiento = DateTime.UtcNow
                    };
                    _context.InventarioMovimientos.Add(movimiento);
                }

                // 11. Clear cart
                _context.CarritoItems.RemoveRange(carrito.CarritoItems);
                _context.CarritoDescuentos.RemoveRange(carrito.CarritoDescuentos);
                carrito.EstatusVentaId = 5; // Completed

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 12. Audit
                await AuditAsync("Order.Created", new
                {
                    OrdenId = orden.IdOrden,
                    NumeroOrden = numeroOrden,
                    Total = total
                });

                // 13. Return order DTO
                var ordenDto = await GetOrderDtoAsync(orden.IdOrden);
                return ApiResponse<OrdenDto>.Ok(ordenDto, "Orden creada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<OrdenDto>(9000);
            }
        }

        private async Task<decimal> CalculateTotalWeightAsync(List<CarritoItemDto> items)
        {
            var productIds = items.Select(i => i.ProductoId).ToList();
            var productos = await _context.Productos
                .Where(p => productIds.Contains(p.IdProducto))
                .Select(p => new { p.IdProducto, p.Peso })
                .ToListAsync();

            decimal total = 0;
            foreach (var item in items)
            {
                var producto = productos.FirstOrDefault(p => p.IdProducto == item.ProductoId);
                total += (producto?.Peso ?? 1) * item.Cantidad;
            }
            return total;
        }

        private async Task<decimal> CalculateTotalWeightAsync(ICollection<CarritoItem> items)
        {
            return items.Sum(i => (i.Producto.Peso ?? 1) * i.Cantidad);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private async Task<OrdenDto> GetOrderDtoAsync(int ordenId)
        {
            var orden = await _context.Ordens
                .Include(o => o.DireccionEnvio)
                .Include(o => o.OrdenItems)
                    .ThenInclude(oi => oi.Producto)
                        .ThenInclude(p => p.ProductoImagens)
                .Include(o => o.OrdenDescuentos)
                    .ThenInclude(od => od.Descuento)
                .Include(o => o.EstatusVenta)
                .FirstOrDefaultAsync(o => o.IdOrden == ordenId);

            return new OrdenDto
            {
                IdOrden = orden.IdOrden,
                NumeroOrden = orden.NumeroOrden,
                FechaOrden = orden.FechaOrden,
                //EstatusVenta = orden.EstatusVenta.NombreEstatusVenta,
                DireccionEnvio = MapToDireccionDto(orden.DireccionEnvio),
                Items = orden.OrdenItems.Select(oi => new OrdenItemDto
                {
                    IdOrdenItem = oi.IdOrdenItem,
                    ProductoId = oi.ProductoId,
                    NombreProducto = oi.Producto.NombreProducto,
                    ImagenUrl = oi.Producto.ProductoImagens.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    Cantidad = oi.Cantidad,
                    PrecioUnitario = oi.PrecioUnitario,
                    DescuentoAplicado = oi.DescuentoAplicado,
                    Subtotal = (oi.PrecioUnitario - oi.DescuentoAplicado) * oi.Cantidad
                }).ToList(),
                Descuentos = orden.OrdenDescuentos.Select(od => new OrdenDescuentoDto
                {
                    NombreDescuento = od.Descuento.NombreDescuento,
                    TipoDescuento = od.Descuento.TipoDescuento,
                    MontoDescuento = od.MontoAplicado,
                    CodigoCupon = od.Descuento.ReglaDescuentos.FirstOrDefault(x => x.DescuentoId == od.DescuentoId)?.CodigoCupon,
                }).ToList(),
                Subtotal = orden.Subtotal,
                //DescuentoTotal = orden.DescuentoTotal,
                //ImpuestoTotal = orden.ImpuestoTotal,
                //EnvioTotal = 0,  Add shipping tracking if needed
                Total = orden.Total
            };
        }

        private DireccionDto MapToDireccionDto(Direccion d)
        {
            return new DireccionDto
            {
                IdDireccion = d.IdDireccion,
                PaisNombre = d.PaisNombre,
                EstadoNombre = d.Estado.NombreEstado,
                CiudadNombre = d.CiudadNombre,
                CodigoPostal = d.CodigoPostal,
                Colonia = d.Colonia,
                Calle = d.Calle,
                NumeroExterior = d.NumeroExterior,
                NumeroInterior = d.NumeroInterior,
                DireccionCompleta = d.DireccionCompleta,
                Referencia = d.Referencia
            };
        }


    }
}