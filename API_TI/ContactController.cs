using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ContactoDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : BaseApiController
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Send contact form message
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] ContactRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<object>.Fail(2, "Datos inválidos", 2));

            var response = await _contactService.SendContactMessageAsync(request);
            return FromApiResponse(response);
        }
    }
}