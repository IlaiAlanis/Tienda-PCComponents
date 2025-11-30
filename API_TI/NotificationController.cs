using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _notificationService.GetUserNotificationsAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var response = await _notificationService.MarkAsReadAsync(id, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var response = await _notificationService.MarkAllAsReadAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var response = await _notificationService.DeleteNotificationAsync(id, GetUserId());
            return FromApiResponse(response);
        }
    }
}
