using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CotizacionDTOs;
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
    public class ShippingController : BaseApiController
    {
        private readonly IShippingService _shippingService;

        public ShippingController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost("quote")]
        public async Task<IActionResult> GetQuotes([FromBody] CotizarEnvioRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<CotizacionesEnvioResponse>.Fail(2, "Datos inválidos", 2));

            var response = await _shippingService.GetShippingQuotesAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpGet("local-rate")]
        public async Task<IActionResult> GetLocalRate([FromQuery] int direccionId, [FromQuery] decimal pesoKg)
        {
            var response = await _shippingService.GetLocalShippingRateAsync(direccionId, pesoKg);
            return FromApiResponse(response);
        }
    }
}
