using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.NotificacionDTOs;

namespace API_TI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<IList<NotificacionDto>>> GetUserNotificationsAsync(int usuarioId);
        Task<ApiResponse<object>> MarkAsReadAsync(int notificationId, int usuarioId);
        Task<ApiResponse<object>> MarkAllAsReadAsync(int usuarioId);
        Task<ApiResponse<object>> DeleteNotificationAsync(int notificationId, int usuarioId);
        Task SendNotificationAsync(int usuarioId, string titulo, string mensaje, string tipo = "GENERAL");
    }
}
