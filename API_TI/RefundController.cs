using API_TI.Models.DTOs.ReembolsoDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RefundController : BaseApiController
    {
        private readonly IReembolsoService _reembolsoService;

        public RefundController(IReembolsoService reembolsoService)
        {
            _reembolsoService = reembolsoService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost]
        public async Task<IActionResult> Request([FromBody] RefundRequest request)
        {
            var response = await _reembolsoService.RequestRefundAsync(request, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPost("{id}/process")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Process(int id)
        {
            var response = await _reembolsoService.ProcessRefundAsync(id, GetUserId());
            return FromApiResponse(response);
        }
    }
}
