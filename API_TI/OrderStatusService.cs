using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Models.DTOs.OrderDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class OrderStatusService : BaseService, IOrderStatusService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public OrderStatusService(
            TiPcComponentsContext context,
            IEmailService emailService,
            INotificationService notificationService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<OrdenDto>> UpdateOrderStatusAsync(int ordenId, UpdateOrderStatusRequest request, int adminId)
        {
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.Usuario)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == ordenId);

                if (orden == null)
                    return await ReturnErrorAsync<OrdenDto>(1000);

                var newStatus = await _context.EstatusVenta
                    .FirstOrDefaultAsync(e => e.IdEstatusVenta == request.NuevoEstatusId);

                if (newStatus == null)
                    return await ReturnErrorAsync<OrdenDto>(5, "Estado inválido");

                // Validate transition
                if (!IsValidTransition(orden.EstatusVentaId, request.NuevoEstatusId))
                    return await ReturnErrorAsync<OrdenDto>(5, "Transición de estado inválida");

                var oldStatusId = orden.EstatusVentaId;
                orden.EstatusVentaId = request.NuevoEstatusId;

                //Add tracking info if shipping
                //if (newStatus.Codigo == "SHIPPED" && !string.IsNullOrWhiteSpace(request.NumeroSeguimiento))
                //{
                //    orden.NumeroSeguimiento = request.NumeroSeguimiento;
                //    orden.OperadorEnvioId = request.OperadorEnvioId;
                //    orden.FechaEnvio = DateTime.UtcNow;
                //}

                if (newStatus.Codigo == "DELIVERED")
                    orden.FechaEntrega = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send notifications
                await SendStatusChangeNotifications(orden, newStatus);

                await AuditAsync("Order.StatusChanged", new
                {
                    OrdenId = ordenId,
                    OldStatus = oldStatusId,
                    NewStatus = request.NuevoEstatusId
                }, adminId);

                return ApiResponse<OrdenDto>.Ok(null, "Estado actualizado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<OrdenDto>(9000);
            }
        }

        private bool IsValidTransition(int currentStatusId, int newStatusId)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["PENDING"] = new() { "PROCESSING", "CANCELLED" },
                ["PROCESSING"] = new() { "SHIPPED", "CANCELLED" },
                ["SHIPPED"] = new() { "DELIVERED", "RETURNED" },
                ["DELIVERED"] = new() { "RETURNED" },
                ["PAID"] = new() { "PROCESSING", "REFUND" }
            };

            // Implementation depends on your status codes
            return true; // Simplified
        }

        private async Task SendStatusChangeNotifications(Orden orden, EstatusVentum newStatus)
        {
            var userEmail = orden.Usuario.Correo;
            var orderNumber = orden.NumeroOrden;

            switch (newStatus.Codigo)
            {
                case "PROCESSING":
                    await _emailService.SendOrderProcessingEmailAsync(userEmail, orderNumber);
                    await _notificationService.SendNotificationAsync(
                        orden.UsuarioId,
                        "Orden en proceso",
                        $"Tu orden {orderNumber} está siendo preparada",
                        "ORDER_UPDATE"
                    );
                    break;

                case "SHIPPED":
                    await _emailService.SendOrderShippedEmailAsync(
                        userEmail,
                        orderNumber,
                        orden.NumeroSeguimiento
                    );
                    await _notificationService.SendNotificationAsync(
                        orden.UsuarioId,
                        "Orden enviada",
                        $"Tu orden {orderNumber} ha sido enviada. Seguimiento: {orden.NumeroSeguimiento}",
                        "ORDER_SHIPPED"
                    );
                    break;

                case "DELIVERED":
                    await _emailService.SendOrderDeliveredEmailAsync(userEmail, orderNumber);
                    await _notificationService.SendNotificationAsync(
                        orden.UsuarioId,
                        "Orden entregada",
                        $"Tu orden {orderNumber} fue entregada",
                        "ORDER_DELIVERED"
                    );
                    break;

                case "CANCELLED":
                    await _emailService.SendOrderCancelledEmailAsync(userEmail, orderNumber);
                    await _notificationService.SendNotificationAsync(
                        orden.UsuarioId,
                        "Orden cancelada",
                        $"Tu orden {orderNumber} fue cancelada",
                        "ORDER_CANCELLED"
                    );
                    break;
            }
        }
    }
}
