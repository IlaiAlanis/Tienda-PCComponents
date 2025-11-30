using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Models.DTOs.MarcaDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CarritoController : BaseApiController
    {
        private readonly ICarritoService _carritoService;

        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var response = await _carritoService.GetCartAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
        {
            var response = await _carritoService.AddItemAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var response = await _carritoService.UpdateItemAsync(GetUserId(), itemId, request);
            return FromApiResponse(response);
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var response = await _carritoService.RemoveItemAsync(GetUserId(), itemId);
            return FromApiResponse(response);
        }

        [HttpPost("coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto request)
        {
            var response = await _carritoService.ApplyCouponAsync(GetUserId(), request.CodigoCupon);
            return FromApiResponse(response);
        }

        [HttpDelete("coupon")]
        public async Task<IActionResult> RemoveCoupon()
        {
            var response = await _carritoService.RemoveCouponAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var response = await _carritoService.ClearCartAsync(GetUserId());
            return FromApiResponse(response);
        }
    }
}
