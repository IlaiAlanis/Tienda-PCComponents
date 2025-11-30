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
    public class ReturnController : BaseApiController
    {
        private readonly IDevolucionService _devolucionService;

        public ReturnController(IDevolucionService devolucionService)
        {
            _devolucionService = devolucionService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost]
        public async Task<IActionResult> RequestReturn([FromBody] ReturnRequest request)
        {
            var response = await _devolucionService.RequestReturnAsync(request, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPost("{id}/process")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ProcessReturn(int id, [FromBody] ProcessReturnRequest request)
        {
            var response = await _devolucionService.ProcessReturnAsync(id, request, GetUserId());
            return FromApiResponse(response);
        }
    }
}
