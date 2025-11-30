using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ReembolsoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class DevolucionService : BaseService, IDevolucionService
    {
        private readonly TiPcComponentsContext _context;
        private readonly INotificationService _notificationService;

        public DevolucionService(
            TiPcComponentsContext context,
            INotificationService notificationService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<DevolucionDto>> RequestReturnAsync(ReturnRequest request, int usuarioId)
        {
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.OrdenItems)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == request.OrdenId && o.UsuarioId == usuarioId);

                if (orden == null)
                    return await ReturnErrorAsync<DevolucionDto>(1000);

                if (orden.EstatusVenta.Codigo != "DELIVERED")
                    return await ReturnErrorAsync<DevolucionDto>(5, "Solo se pueden devolver órdenes entregadas");

                // Check return window (e.g., 30 days)
                if ((DateTime.UtcNow - orden.FechaEntrega.Value).TotalDays > 30)
                    return await ReturnErrorAsync<DevolucionDto>(5, "Periodo de devolución expirado");

                var devolucion = new Devolucion
                {
                    OrdenId = request.OrdenId,
                    UsuarioId = usuarioId,
                    TipoDevolucion = request.TipoDevolucion, // RETURN or EXCHANGE
                    Motivo = request.Motivo,
                    EstatusDevolucionId = 1, // Pending
                    FechaSolicitud = DateTime.UtcNow
                };

                _context.Devolucions.Add(devolucion);

                // Add items
                foreach (var itemRequest in request.Items)
                {
                    var ordenItem = orden.OrdenItems.FirstOrDefault(oi => oi.IdOrdenItem == itemRequest.OrdenItemId);
                    if (ordenItem == null) continue;

                    _context.DevolucionItems.Add(new DevolucionItem
                    {
                        DevolucionId = devolucion.IdDevolucion,
                        OrdenItemId = itemRequest.OrdenItemId,
                        Cantidad = itemRequest.Cantidad,
                        ProductoIntercambioId = itemRequest.ProductoIntercambioId
                    });
                }

                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync(
                    usuarioId,
                    "Solicitud de devolución recibida",
                    $"Tu solicitud de devolución #{devolucion.IdDevolucion} está siendo procesada",
                    "RETURN"
                );

                await AuditAsync("Return.Requested", new { DevolucionId = devolucion.IdDevolucion }, usuarioId);

                return ApiResponse<DevolucionDto>.Ok(new DevolucionDto
                {
                    IdDevolucion = devolucion.IdDevolucion,
                    OrdenId = devolucion.OrdenId,
                    TipoDevolucion = devolucion.TipoDevolucion,
                    Estatus = "PENDIENTE"
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DevolucionDto>(9000);
            }
        }

        public async Task<ApiResponse<DevolucionDto>> ProcessReturnAsync(int devolucionId, ProcessReturnRequest request, int adminId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var devolucion = await _context.Devolucions
                    .Include(d => d.Orden)
                        .ThenInclude(o => o.OrdenItems)
                    .Include(d => d.DevolucionItems)
                    .FirstOrDefaultAsync(d => d.IdDevolucion == devolucionId);

                if (devolucion == null)
                    return await ReturnErrorAsync<DevolucionDto>(5);

                devolucion.EstatusDevolucionId = request.Aprobar ? 2 : 3; // Approved/Rejected
                devolucion.NotasAdmin = request.Notas;
                devolucion.FechaProcesado = DateTime.UtcNow;

                if (request.Aprobar)
                {
                    if (devolucion.TipoDevolucion == "RETURN")
                    {
                        // Restock items
                        foreach (var item in devolucion.DevolucionItems)
                        {
                            var ordenItem = devolucion.Orden.OrdenItems.First(oi => oi.IdOrdenItem == item.OrdenItemId);
                            var producto = await _context.Productos.FindAsync(ordenItem.ProductoId);
                            producto.StockTotal += item.Cantidad;
                        }
                    }
                    else if (devolucion.TipoDevolucion == "EXCHANGE")
                    {
                        // Create exchange order items
                        foreach (var item in devolucion.DevolucionItems.Where(i => i.ProductoIntercambioId.HasValue))
                        {
                            var newProduct = await _context.Productos.FindAsync(item.ProductoIntercambioId.Value);
                            if (newProduct.StockTotal < item.Cantidad)
                                return await ReturnErrorAsync<DevolucionDto>(303, "Stock insuficiente para intercambio");

                            newProduct.StockTotal -= item.Cantidad;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.SendNotificationAsync(
                    devolucion.UsuarioId,
                    request.Aprobar ? "Devolución aprobada" : "Devolución rechazada",
                    request.Aprobar ? "Tu devolución ha sido aprobada" : $"Tu devolución fue rechazada: {request.Notas}",
                    "RETURN"
                );

                return ApiResponse<DevolucionDto>.Ok(new DevolucionDto
                {
                    IdDevolucion = devolucion.IdDevolucion,
                    OrdenId = devolucion.OrdenId,
                    TipoDevolucion = devolucion.TipoDevolucion,
                    Estatus = request.Aprobar ? "APROBADO" : "RECHAZADO"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DevolucionDto>(9000);
            }
        }
    }
}
