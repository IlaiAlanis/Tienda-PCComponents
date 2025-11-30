using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CouponDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")]
    public class DescuentoController : BaseApiController
    {
        private readonly IDescuentoAdminService _descuentoAdminService;

        public DescuentoController(IDescuentoAdminService descuentoAdminService)
        {
            _descuentoAdminService = descuentoAdminService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        /// <summary>
        /// Get all discounts with optional filters (Admin)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _descuentoAdminService.GetAllDiscountsAsync(search, status, page, pageSize);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Get discount by ID (Admin)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _descuentoAdminService.GetDiscountByIdAsync(id);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Create new discount (Admin)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDescuentoRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<DescuentoDto>.Fail(2, "Datos inválidos", 2));

            var response = await _descuentoAdminService.CreateDiscountAsync(request, GetUserId());
            return FromApiResponse(response);
        }

        /// <summary>
        /// Update discount (Admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDescuentoRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<DescuentoDto>.Fail(2, "Datos inválidos", 2));

            var response = await _descuentoAdminService.UpdateDiscountAsync(id, request, GetUserId());
            return FromApiResponse(response);
        }

        /// <summary>
        /// Delete discount (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _descuentoAdminService.DeleteDiscountAsync(id, GetUserId());
            return FromApiResponse(response);
        }

        /// <summary>
        /// Validate coupon code (Public - used during checkout)
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidarCuponDto request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Codigo))
                return FromApiResponse(ApiResponse<object>.Fail(2, "Código de cupón requerido", 2));

            var userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)
                : 0;

            var response = await _descuentoAdminService.ValidateCouponAsync(request.Codigo, userId);
            return FromApiResponse(response);
        }
    }
}