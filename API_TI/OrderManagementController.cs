using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")]
    public class OrderManagementController : BaseApiController
    {
        private readonly IOrdenService _ordenService;

        public OrderManagementController(IOrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        private int GetAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        /// <summary>
        /// Get all orders with filtering (Admin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _ordenService.GetAllOrdersAsync(search, status, page, pageSize);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Get order by ID (Admin - any order)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var response = await _ordenService.GetOrderByIdAdminAsync(id);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// Accepts { estado: "string" } in request body
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Estado))
            {
                return BadRequest(new
                {
                    success = false,
                    error = new
                    {
                        code = 400,
                        message = "El campo 'estado' es requerido"
                    }
                });
            }

            var response = await _ordenService.UpdateOrderStatusAsync(id, request.Estado, GetAdminId());
            return FromApiResponse(response);
        }
    }

    /// <summary>
    /// DTO for update order status request
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        public string Estado { get; set; } = string.Empty;
    }
}
