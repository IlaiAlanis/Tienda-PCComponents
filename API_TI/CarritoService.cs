using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;

namespace API_TI.Services.Implementations
{
    public class CarritoService : BaseService, ICarritoService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IDescuentoService _descuentoService;
        private readonly IImpuestoService _impuestoService;

        public CarritoService(
            TiPcComponentsContext context,
            IDescuentoService descuentoService,
            IImpuestoService impuestoService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _descuentoService = descuentoService;
            _impuestoService = impuestoService;
        }

        public async Task<ApiResponse<CarritoDto>> GetCartAsync(int usuarioId)
        {
            try
            {
                var carrito = await GetOrCreateCartAsync(usuarioId);
                var dto = await MapToCarritoDto(carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<CarritoDto>> AddItemAsync(int usuarioId, AddToCartRequest request)
        {
            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == request.ProductoId && p.EstaActivo);

                if (producto == null)
                    return await ReturnErrorAsync<CarritoDto>(300);

                if (producto.StockTotal < request.Cantidad)
                    return await ReturnErrorAsync<CarritoDto>(303, "Stock insuficiente");

                var carrito = await GetOrCreateCartAsync(usuarioId);


                var existingItem = await _context.CarritoItems
                    .FirstOrDefaultAsync(i => i.CarritoId == carrito.IdCarrito && i.ProductoId == request.ProductoId);

                if (existingItem != null)
                {
                    var newQuantity = existingItem.Cantidad + request.Cantidad;

                    if (producto.StockTotal < newQuantity)
                    {
                        if (producto.PermitePreorden)
                        {
                            existingItem.Cantidad = newQuantity;
                            existingItem.EsPreorden = true;
                            existingItem.FechaEstimadaEntrega = producto.FechaRestock;
                        }
                        else
                            return await ReturnErrorAsync<CarritoDto>(303, "Stock insuficiente");
                    }
                    else
                    {
                        existingItem.Cantidad = newQuantity;
                    }
                }
                else
                {
                    // New item
                    if (producto.StockTotal < request.Cantidad)
                    {
                        if (producto.PermitePreorden)
                        {
                            _context.CarritoItems.Add(new CarritoItem
                            {
                                CarritoId = carrito.IdCarrito,
                                ProductoId = request.ProductoId,
                                Cantidad = request.Cantidad,
                                PrecioUnitario = producto.PrecioPromocional ?? producto.PrecioBase,
                                EsPreorden = true,
                                FechaEstimadaEntrega = producto.FechaRestock,
                                FechaReserva = DateTime.UtcNow
                            });
                        }
                        else
                            return await ReturnErrorAsync<CarritoDto>(303, "Stock insuficiente");
                    }
                    else
                    {
                        _context.CarritoItems.Add(new CarritoItem
                        {
                            CarritoId = carrito.IdCarrito,
                            ProductoId = request.ProductoId,
                            Cantidad = request.Cantidad,
                            PrecioUnitario = producto.PrecioPromocional ?? producto.PrecioBase,
                            EsPreorden = false,
                            FechaReserva = DateTime.UtcNow
                        });
                    }
                }

                carrito.UltimaActividad = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await RecalculateCartAsync(carrito.IdCarrito);

                var dto = await MapToCarritoDto(carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<CarritoDto>> UpdateItemAsync(int usuarioId, int itemId, UpdateCartItemRequest request)
        {
            try
            {
                var item = await _context.CarritoItems
                    .Include(i => i.Carrito)
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.IdCarritoItem == itemId && i.Carrito.UsuarioId == usuarioId);

                if (item == null)
                    return await ReturnErrorAsync<CarritoDto>(602);

                if (item.Producto.StockTotal < request.Cantidad)
                    return await ReturnErrorAsync<CarritoDto>(303);

                item.Cantidad = request.Cantidad;
                await _context.SaveChangesAsync();
                await RecalculateCartAsync(item.CarritoId);

                var dto = await MapToCarritoDto(item.Carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<CarritoDto>> RemoveItemAsync(int usuarioId, int itemId)
        {
            try
            {
                var item = await _context.CarritoItems
                    .Include(i => i.Carrito)
                    .FirstOrDefaultAsync(i => i.IdCarritoItem == itemId && i.Carrito.UsuarioId == usuarioId);

                if (item == null)
                    return await ReturnErrorAsync<CarritoDto>(602);

                _context.CarritoItems.Remove(item);
                await _context.SaveChangesAsync();
                await RecalculateCartAsync(item.CarritoId);

                var dto = await MapToCarritoDto(item.Carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<CarritoDto>> ApplyCouponAsync(int usuarioId, string codigoCupon)
        {
            try
            {
                var validation = await _descuentoService.ValidateCouponAsync(codigoCupon, usuarioId);
                if (!validation.Success)
                    return await ReturnErrorAsync<CarritoDto>(validation.Error.Code, validation.Error.Message);

                var carrito = await GetOrCreateCartAsync(usuarioId);
                await RecalculateCartAsync(carrito.IdCarrito, codigoCupon);

                var dto = await MapToCarritoDto(carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<CarritoDto>> RemoveCouponAsync(int usuarioId)
        {
            try
            {
                var carrito = await GetOrCreateCartAsync(usuarioId);

                // Remove coupon discounts
                var couponDiscounts = await _context.CarritoDescuentos
                    .Where(cd => cd.CarritoId == carrito.IdCarrito && cd.ReglaDescuentoId != null)
                    .ToListAsync();

                _context.CarritoDescuentos.RemoveRange(couponDiscounts);
                await _context.SaveChangesAsync();

                await RecalculateCartAsync(carrito.IdCarrito);

                var dto = await MapToCarritoDto(carrito);
                return ApiResponse<CarritoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CarritoDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> ClearCartAsync(int usuarioId)
        {
            try
            {
                var carrito = await _context.Carritos
                    .Include(c => c.CarritoItems)
                    .Include(c => c.CarritoDescuentos)
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (carrito != null)
                {
                    _context.CarritoItems.RemoveRange(carrito.CarritoItems);
                    _context.CarritoDescuentos.RemoveRange(carrito.CarritoDescuentos);
                    await _context.SaveChangesAsync();
                }

                return ApiResponse<object>.Ok(null);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        // Helper methods
        private async Task<Carrito> GetOrCreateCartAsync(int usuarioId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritoItems)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.ProductoImagens)
                .Include(c => c.CarritoDescuentos)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.EstatusVentaId == 1);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    UsuarioId = usuarioId,
                    EstatusVentaId = 1, // Active cart
                    FechaCreacion = DateTime.UtcNow
                };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return carrito;
        }

        private async Task RecalculateCartAsync(int carritoId, string codigoCupon = null)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritoItems)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(c => c.CarritoItems)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Marca)
                .Include(c => c.CarritoDescuentos)
                .FirstOrDefaultAsync(c => c.IdCarrito == carritoId);

            if (carrito == null || !carrito.CarritoItems.Any()) return;

            // Clear existing discounts
            _context.CarritoDescuentos.RemoveRange(carrito.CarritoDescuentos);
            await _context.SaveChangesAsync();

            // Calculate subtotal
            decimal subtotal = carrito.CarritoItems.Sum(i => i.PrecioUnitario * i.Cantidad);

            // Get applicable discounts
            var descuentos = await _descuentoService.GetApplicableDiscountsAsync(carritoId, codigoCupon);

            decimal totalDescuento = 0;
            var descuentosAplicados = new List<CarritoDescuento>();

            foreach (var descuento in descuentos)
            {
                var regla = descuento.ReglaDescuentos.FirstOrDefault();
                if (regla == null) continue;

                // Validate rules
                if (regla.MontoMinimoCompra.HasValue && subtotal < regla.MontoMinimoCompra) continue;
                if (regla.CantidadMinProductos.HasValue && carrito.CarritoItems.Sum(i => i.Cantidad) < regla.CantidadMinProductos) continue;

                decimal montoDescuento = CalculateDiscountAmount(descuento, subtotal - totalDescuento, carrito);

                if (montoDescuento > 0)
                {
                    descuentosAplicados.Add(new CarritoDescuento
                    {
                        CarritoId = carritoId,
                        DescuentoId = descuento.IdDescuento,
                        ReglaDescuentoId = regla.IdRegla,
                        MontoAplicado = montoDescuento
                    });

                    totalDescuento += montoDescuento;

                    // If not stackable, break
                    if (!descuento.EsAcumulable) break;
                }
            }

            _context.CarritoDescuentos.AddRange(descuentosAplicados);

            carrito.Subtotal = subtotal;
            carrito.DescuentoTotal = totalDescuento;

            await _context.SaveChangesAsync();
        }

        private decimal CalculateDiscountAmount(Descuento descuento, decimal subtotal, Carrito carrito)
        {
            decimal monto = 0;

            switch (descuento.TipoDescuento)
            {
                case "PORCENTAJE":
                    monto = subtotal * (descuento.Valor / 100);
                    break;
                case "MONTO_FIJO":
                    monto = descuento.Valor;
                    break;
                case "ENVIO_GRATIS":
                    // Handled in checkout
                    break;
                case "BOGO":
                    // Buy one get one - complex logic
                    var alcances = descuento.DescuentoAlcances.ToList();
                    foreach (var item in carrito.CarritoItems)
                    {
                        if (alcances.Any(a =>
                            (a.TipoEntidad == "PRODUCTO" && a.EntidadId == item.ProductoId) ||
                            (a.TipoEntidad == "CATEGORIA" && a.EntidadId == item.Producto.CategoriaId)))
                        {
                            int freeItems = item.Cantidad / 2;
                            monto += freeItems * item.PrecioUnitario;
                        }
                    }
                    break;
            }

            // Apply max value cap
            if (descuento.ValorMaximo.HasValue && monto > descuento.ValorMaximo)
                monto = descuento.ValorMaximo.Value;

            return monto;
        }

        private async Task<CarritoDto> MapToCarritoDto(Carrito carrito)
        {
            var items = carrito.CarritoItems.Select(i => new CarritoItemDto
            {
                IdCarritoItem = i.IdCarritoItem,
                ProductoId = i.ProductoId,
                NombreProducto = i.Producto.NombreProducto,
                ImagenUrl = i.Producto.ProductoImagens.FirstOrDefault(img => img.EsPrincipal)?.UrlImagen,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                DescuentoAplicado = i.DescuentoAplicado,
                Subtotal = (i.PrecioUnitario - i.DescuentoAplicado) * i.Cantidad,
                StockDisponible = i.Producto.StockTotal
            }).ToList();

            var descuentos = carrito.CarritoDescuentos.Select(cd => new CarritoDescuentoDto
            {
                IdDescuento = cd.DescuentoId,
                NombreDescuento = cd.Descuento?.NombreDescuento,
                TipoDescuento = cd.Descuento?.TipoDescuento,
                MontoDescuento = cd.MontoAplicado,
                CodigoCupon = cd.ReglaDescuento?.CodigoCupon
            }).ToList();

            return new CarritoDto
            {
                IdCarrito = carrito.IdCarrito,
                UsuarioId = carrito.UsuarioId,
                Items = items,
                DescuentosAplicados = descuentos,
                Subtotal = carrito.Subtotal,
                DescuentoTotal = carrito.DescuentoTotal,
                ImpuestoTotal = 0, // Calculated at checkout
                EnvioTotal = 0, // Calculated at checkout
                Total = carrito.Subtotal - carrito.DescuentoTotal,
                TotalItems = items.Sum(i => i.Cantidad)
            };
        }
    }
}
