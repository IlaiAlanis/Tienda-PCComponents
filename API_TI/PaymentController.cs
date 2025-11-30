using API_TI.Models.DTOs.PagoDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : BaseApiController
    {
        private readonly IStripeService _stripeService;
        private readonly IPayPalService _paypalService;

        public PaymentController(
            IStripeService stripeService,
            IPayPalService paypalService
        )
        {
            _stripeService = stripeService;
            _paypalService = paypalService;
        }

        [HttpPost("stripe/create-intent")]
        public async Task<IActionResult> CreateStripeIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var response = await _stripeService.CreatePaymentIntentAsync(request.OrdenId, request.Monto);
            return FromApiResponse(response);
        }

        [HttpPost("stripe/confirm")]
        public async Task<IActionResult> ConfirmStripePayment([FromBody] ConfirmPaymentRequest request)
        {
            var response = await _stripeService.ConfirmPaymentAsync(request.OrdenId, request.PaymentIntentId);
            return FromApiResponse(response);
        }

        [HttpPost("paypal/create-order")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] CreatePayPalOrderRequest request)
        {
            var response = await _paypalService.CreateOrderAsync(request.OrdenId, request.Monto);
            return FromApiResponse(response);
        }

        [HttpPost("paypal/capture")]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] CapturePayPalOrderRequest request)
        {
            var response = await _paypalService.CaptureOrderAsync(request.OrdenId, request.PayPalOrderId);
            return FromApiResponse(response);
        }
    }
}
