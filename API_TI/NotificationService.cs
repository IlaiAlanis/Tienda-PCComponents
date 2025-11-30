using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.NotificacionDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class NotificationService : BaseService, INotificationService
    {
        private readonly TiPcComponentsContext _context;

        public NotificationService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<IList<NotificacionDto>>> GetUserNotificationsAsync(int usuarioId)
        {
            try
            {
                var notifications = await _context.NotificacionUsuarios
                    .Where(n => n.UsuarioId == usuarioId)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(50)
                    .ToListAsync();

                var dtoList = Mapper.ToNotificacionDto(notifications);

                return ApiResponse<IList<NotificacionDto>>.Ok(dtoList);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<NotificacionDto>>(9000);
            }
        }

        public async Task<ApiResponse<object>> MarkAsReadAsync(int notificationId, int usuarioId)
        {
            try
            {
                var notification = await _context.NotificacionUsuarios
                    .FirstOrDefaultAsync(n => n.IdNotificacion == notificationId && n.UsuarioId == usuarioId);

                if (notification == null)
                    return await ReturnErrorAsync<object>(5, "Notificación no encontrada");

                notification.Leido = true;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.Ok(null, "Notificación marcada como leída");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> MarkAllAsReadAsync(int usuarioId)
        {
            try
            {
                var unreadNotifications = await _context.NotificacionUsuarios
                    .Where(n => n.UsuarioId == usuarioId && !n.Leido)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                    notification.Leido = true;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.Ok(null, $"{unreadNotifications.Count} notificaciones marcadas como leídas");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteNotificationAsync(int notificationId, int usuarioId)
        {
            try
            {
                var notification = await _context.NotificacionUsuarios
                    .FirstOrDefaultAsync(n => n.IdNotificacion == notificationId && n.UsuarioId == usuarioId);

                if (notification == null)
                    return await ReturnErrorAsync<object>(5, "Notificación no encontrada");

                _context.NotificacionUsuarios.Remove(notification);
                await _context.SaveChangesAsync();

                return ApiResponse<object>.Ok(null, "Notificación eliminada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task SendNotificationAsync(int usuarioId, string titulo, string mensaje, string tipo = "GENERAL")
        {
            try
            {
                _context.NotificacionUsuarios.Add(new NotificacionUsuario
                {
                    UsuarioId = usuarioId,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = tipo,
                    Leido = false,
                    FechaCreacion = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Don't throw - notification failure shouldn't break business logic
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        
    }
}