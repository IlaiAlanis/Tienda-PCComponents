using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Azure;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;
using QuestPDF.Helpers;

namespace API_TI.Services.Implementations
{
    public class OrdenService : BaseService, IOrdenService
    {
        private readonly TiPcComponentsContext _context;

        public OrdenService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        #region User Methods
        public async Task<ApiResponse<PagedResult<OrdenDto>>> GetUserOrdersAsync(int usuarioId, int page, int pageSize)
        {
            try
            {
                var query = _context.Ordens
                    .Include(o => o.EstatusVenta)
                    .Include(o => o.DireccionEnvio)
                    .Include(o => o.Usuario)
                    .Where(o => o.UsuarioId == usuarioId)
                    .OrderByDescending(o => o.FechaOrden);

                var total = await query.CountAsync();
                var ordenes = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = ordenes.Select(o => new OrdenDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden,
                    FechaOrden = o.FechaOrden,
                    Estado = o.EstatusVenta.NombreEstatusVenta,
                    NombreCliente = $"{o.Usuario.NombreUsuario} {o.Usuario.ApellidoPaterno}".Trim(),
                    EmailCliente = o.Usuario.Correo,
                    TelefonoCliente = o.DireccionEnvio.Telefono,
                    Total = o.Total
                }).ToList();


                var pagedResult = new PagedResult<OrdenDto>(
                    items: dtos,
                    totalCount: total,
                    page: page,
                    pageSize: pageSize
                );

                return ApiResponse<PagedResult<OrdenDto>>.Ok(pagedResult);

            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagedResult<OrdenDto>>(9000);
            }
        }

        public async Task<ApiResponse<OrdenDto>> GetOrderByIdAsync(int usuarioId, int ordenId)
        {
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.Usuario)
                    .Include(o => o.DireccionEnvio)
                    .Include(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                            .ThenInclude(p => p.ProductoImagens)
                    .Include(o => o.OrdenDescuentos)
                        .ThenInclude(od => od.Descuento)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId && o.UsuarioId == usuarioId);

                if (orden == null)
                    return await ReturnErrorAsync<OrdenDto>(1000);

                var dto = Mapper.ToOrdenDto(orden);

                return ApiResponse<OrdenDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<OrdenDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> CancelOrderAsync(int ordenId, int usuarioId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId && o.UsuarioId == usuarioId);

                if (orden == null)
                    return await ReturnErrorAsync<object>(1000);

                // Only pending/processing orders can be cancelled
                if (orden.EstatusVentaId > 3)
                    return await ReturnErrorAsync<object>(5, "No se puede cancelar esta orden");

                // Restock
                foreach (var item in orden.OrdenItems)
                {
                    item.Producto.StockTotal += item.Cantidad;
                }

                var cancelledStatus = await _context.EstatusVenta
                    .FirstOrDefaultAsync(e => e.Codigo == "CANCELLED" || e.NombreEstatusVenta.ToLower() == "cancelado");

                if (cancelledStatus != null)
                {
                    orden.EstatusVentaId = cancelledStatus.IdEstatusVenta;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await AuditAsync("Order.Cancelled", new { OrdenId = ordenId }, usuarioId);

                return ApiResponse<object>.Ok(null, "Orden cancelada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        #endregion

        #region Admin Methods
        /// <summary>
        /// Get all orders with server-side filtering for admin
        /// </summary>
        public async Task<ApiResponse<PagedResult<OrdenDto>>> GetAllOrdersAsync(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Ordens
                    .Include(o => o.EstatusVenta)
                    .Include(o => o.Usuario)
                    .Include(o => o.DireccionEnvio)
                    .AsQueryable();

                // Search filter (order ID or customer name/email)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(o =>
                        o.IdOrden.ToString().Contains(search) ||
                        o.NumeroOrden.Contains(search) ||
                        o.Usuario.NombreUsuario.ToLower().Contains(searchLower) ||
                        o.Usuario.ApellidoPaterno.ToLower().Contains(searchLower) ||
                        o.Usuario.Correo.ToLower().Contains(searchLower)
                    );
                }

                // Status filter
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var statusLower = status.ToLower();
                    query = query.Where(o => o.EstatusVenta.NombreEstatusVenta.ToLower() == statusLower);
                }

                // Get total count
                var total = await query.CountAsync();

                // Get paginated results
                var ordenes = await query
                    .OrderByDescending(o => o.FechaOrden)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = ordenes.Select(o => new OrdenDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden,
                    FechaOrden = o.FechaOrden,
                    Estado = o.EstatusVenta.NombreEstatusVenta,
                    NombreCliente = $"{o.Usuario.NombreUsuario} {o.Usuario.ApellidoPaterno ?? ""}".Trim(),
                    EmailCliente = o.Usuario.Correo,
                    TelefonoCliente = o.DireccionEnvio.Telefono,
                    Total = o.Total,
                    Subtotal = o.Subtotal,
                    Descuento = o.DescuentoTotal,
                    Impuestos = o.ImpuestoTotal,
                    CostoEnvio = o.CostoEnvio
                }).ToList();

                var pagedResult = new PagedResult<OrdenDto>(
                    items: dtos,
                    totalCount: total,
                    page: page,
                    pageSize: pageSize
                );

                return ApiResponse<PagedResult<OrdenDto>>.Ok(pagedResult);

            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagedResult<OrdenDto>>(9000);
            }
        }

        /// <summary>
        /// Get order by ID (admin version - any order)
        /// </summary>
        public async Task<ApiResponse<OrdenDto>> GetOrderByIdAdminAsync(int ordenId)
        {
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.Usuario)
                    .Include(o => o.DireccionEnvio)
                    .Include(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                            .ThenInclude(p => p.ProductoImagens)
                    .Include(o => o.OrdenDescuentos)
                        .ThenInclude(od => od.Descuento)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId);

                if (orden == null)
                    return await ReturnErrorAsync<OrdenDto>(1000);

                var dto = Mapper.ToOrdenDto(orden);

                return ApiResponse<OrdenDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<OrdenDto>(9000);
            }
        }

        /// <summary>
        /// Update order status (admin only)
        /// </summary>
        public async Task<ApiResponse<object>> UpdateOrderStatusAsync(int ordenId, string nuevoEstado, int adminId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId);

                if (orden == null)
                    return await ReturnErrorAsync<object>(1000);

                // Find status by name
                var nuevoEstatusVenta = await _context.EstatusVenta
                    .FirstOrDefaultAsync(e => e.NombreEstatusVenta.ToLower() == nuevoEstado.ToLower());

                if (nuevoEstatusVenta == null)
                    return await ReturnErrorAsync<object>(5, "Estado inválido");

                var estadoAnterior = orden.EstatusVenta.NombreEstatusVenta;
                orden.EstatusVentaId = nuevoEstatusVenta.IdEstatusVenta;
                orden.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await AuditAsync("Order.StatusUpdated", new
                {
                    OrdenId = ordenId,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = nuevoEstado,
                    AdminId = adminId
                }, adminId);

                return ApiResponse<object>.Ok(null, "Estado actualizado correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        #endregion


        //public async Task<ApiResponse<PagedResult<OrdenDto>>> GetUserOrdersAsync(int usuarioId, int page, int pageSize)
        //{
        //    try
        //    {
        //        var query = _context.Ordens
        //            .Include(o => o.EstatusVenta)
        //            .Include(o => o.DireccionEnvio)
        //            .Where(o => o.UsuarioId == usuarioId)
        //            .OrderByDescending(o => o.FechaOrden);

        //        var total = await query.CountAsync();
        //        var ordenes = await query
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        var dtos = ordenes.Select(o => new OrdenDto
        //        {
        //            IdOrden = o.IdOrden,
        //            NumeroOrden = o.NumeroOrden,
        //            FechaOrden = o.FechaOrden,
        //            EstatusVenta = o.EstatusVenta.NombreEstatusVenta,
        //            Total = o.Total
        //        }).ToList();

        //        return ApiResponse<PagedResult<OrdenDto>>.Ok(new PagedResult<OrdenDto>
        //        {
        //            Items = dtos,
        //            TotalItems = total,
        //            Page = page,
        //            PageSize = pageSize
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogTechnicalErrorAsync(9000, ex);
        //        return await ReturnErrorAsync<PagedResult<OrdenDto>>(9000);
        //    }
        //}

        //public async Task<ApiResponse<OrdenDto>> GetOrderByIdAsync(int usuarioId, int ordenId)
        //{
        //    try
        //    {
        //        var orden = await _context.Ordens
        //            .Include(o => o.DireccionEnvio)
        //            .Include(o => o.OrdenItems)
        //                .ThenInclude(oi => oi.Producto)
        //                    .ThenInclude(p => p.ProductoImagens)
        //            .Include(o => o.OrdenDescuentos)
        //                .ThenInclude(od => od.Descuento)
        //            .Include(o => o.EstatusVenta)
        //            .FirstOrDefaultAsync(o => o.IdOrden == ordenId && o.UsuarioId == usuarioId);

        //        if (orden == null)
        //            return await ReturnErrorAsync<OrdenDto>(1000);

        //        var dto = new OrdenDto
        //        {
        //            IdOrden = orden.IdOrden,
        //            NumeroOrden = orden.NumeroOrden,
        //            FechaOrden = orden.FechaOrden,
        //            EstatusVenta = orden.EstatusVenta.NombreEstatusVenta,
        //            DireccionEnvio = new DireccionDto
        //            {
        //                Calle = orden.DireccionEnvio.Calle,
        //                NumeroExterior = orden.DireccionEnvio.NumeroExterior,
        //                CodigoPostal = orden.DireccionEnvio.CodigoPostal,
        //                DireccionCompleta = $"{orden.DireccionEnvio.Calle} {orden.DireccionEnvio.NumeroExterior}, " +
        //                   $"{orden.DireccionEnvio.CodigoPostal}"
        //            },
        //            Items = orden.OrdenItems.Select(oi => new OrdenItemDto
        //            {
        //                IdOrdenItem = oi.IdOrdenItem,
        //                ProductoId = oi.ProductoId,
        //                NombreProducto = oi.Producto.NombreProducto,
        //                ImagenUrl = oi.Producto.ProductoImagens.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
        //                Cantidad = oi.Cantidad,
        //                PrecioUnitario = oi.PrecioUnitario,
        //                DescuentoAplicado = oi.DescuentoAplicado,
        //                Subtotal = (oi.PrecioUnitario - oi.DescuentoAplicado) * oi.Cantidad
        //            }).ToList(),
        //            Descuentos = orden.OrdenDescuentos.Select(od => new OrdenDescuentoDto
        //            {
        //                NombreDescuento = od.Descuento.NombreDescuento,
        //                MontoDescuento = od.MontoAplicado
        //            }).ToList(),
        //            Subtotal = orden.Subtotal,
        //            DescuentoTotal = orden.DescuentoTotal,
        //            ImpuestoTotal = orden.ImpuestoTotal,
        //            Total = orden.Total
        //        };

        //        return ApiResponse<OrdenDto>.Ok(dto);
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogTechnicalErrorAsync(9000, ex);
        //        return await ReturnErrorAsync<OrdenDto>(9000);
        //    }
        //}


        //public async Task<ApiResponse<object>> CancelOrderAsync(int ordenId, int usuarioId)
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var orden = await _context.Ordens
        //            .Include(o => o.OrdenItems)
        //                .ThenInclude(oi => oi.Producto)
        //            .Include(o => o.EstatusVenta)
        //            .FirstOrDefaultAsync(o => o.IdOrden == ordenId && o.UsuarioId == usuarioId);

        //        if (orden == null)
        //            return await ReturnErrorAsync<object>(1000);

        //        // Only pending/processing orders can be cancelled
        //        if (orden.EstatusVentaId > 3)
        //            return await ReturnErrorAsync<object>(5, "No se puede cancelar esta orden");

        //        // Restock
        //        foreach (var item in orden.OrdenItems)
        //        {
        //            item.Producto.StockTotal += item.Cantidad;
        //        }

        //        var cancelledStatus = await _context.EstatusVenta.FirstOrDefaultAsync(e => e.Codigo == "CANCELLED");
        //        orden.EstatusVentaId = cancelledStatus.IdEstatusVenta;

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        await AuditAsync("Order.Cancelled", new { OrdenId = ordenId }, usuarioId);

        //        return ApiResponse<object>.Ok(null, "Orden cancelada");
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        await LogTechnicalErrorAsync(9000, ex);
        //        return await ReturnErrorAsync<object>(9000);
        //    }
        //}
    }
}
