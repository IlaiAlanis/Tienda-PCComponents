using API_TI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly TiPcComponentsContext _context;
        private readonly string _webhookSecret;

        public WebhookController(
            ILogger<WebhookController> logger,
            TiPcComponentsContext context,
            IConfiguration config
        )
        {
            _logger = logger;
            _context = context;
            _webhookSecret = config["Payment:Stripe:WebhookSecret"];
        }

        [HttpPost("stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var succeededIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogInformation("Payment succeeded: {PaymentIntentId}", succeededIntent.Id);

                        break;

                    case "payment_intent.payment_failed":
                        var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogWarning("Payment failed: {PaymentIntentId}", failedIntent.Id);
                        break;

                    case "payment_intent.canceled":
                        var canceledIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogWarning("Payment canceled: {PaymentIntentId}", canceledIntent.Id);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Webhook error");
                return BadRequest();
            }
        }

    }
}
