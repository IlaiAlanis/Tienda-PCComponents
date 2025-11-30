using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : BaseApiController
    {
        private readonly IOrdenService _ordenService;

        public OrderController(IOrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _ordenService.GetUserOrdersAsync(GetUserId(), page, pageSize);
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var response = await _ordenService.GetOrderByIdAsync(GetUserId(), id);
            return FromApiResponse(response);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var response = await _ordenService.CancelOrderAsync(id, GetUserId());
            return FromApiResponse(response);
        }
    }
}
