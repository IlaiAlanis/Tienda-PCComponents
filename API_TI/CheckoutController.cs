using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckoutController : BaseApiController
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost("summary")]
        public async Task<IActionResult> GetSummary([FromBody] InitiateCheckoutRequest request)
        {
            var response = await _checkoutService.GetCheckoutSummaryAsync(GetUserId(), request.DireccionEnvioId);
            return FromApiResponse(response);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmCheckoutRequest request)
        {
            var response = await _checkoutService.ConfirmCheckoutAsync(GetUserId(), request);
            return FromApiResponse(response);
        }
    }
}
