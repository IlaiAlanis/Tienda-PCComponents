using API_TI.Models.DTOs.PagoDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodsController : BaseApiController
    {
        private readonly IPaymentMethodsService _service;

        public PaymentMethodsController(IPaymentMethodsService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetUserPaymentMethodsAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentMethodRequest request)
        {
            var response = await _service.CreatePaymentMethodAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentMethodRequest request)
        {
            var response = await _service.UpdatePaymentMethodAsync(GetUserId(), id, request);
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeletePaymentMethodAsync(GetUserId(), id);
            return FromApiResponse(response);
        }

        [HttpPut("{id}/predeterminado")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var response = await _service.SetDefaultPaymentMethodAsync(GetUserId(), id);
            return FromApiResponse(response);
        }
    }
}