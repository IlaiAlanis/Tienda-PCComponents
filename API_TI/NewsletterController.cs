using API_TI.Models.DTOs.NewsletterDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : BaseApiController
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            var response = await _newsletterService.SubscribeAsync(request.Email);
            return FromApiResponse(response);
        }

        [HttpPost("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Unsubscribe([FromBody] SubscribeRequest request)
        {
            var response = await _newsletterService.UnsubscribeAsync(request.Email);
            return FromApiResponse(response);
        }
    }

   
}
