using API_TI.Models.DTOs.ConfGlobalDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PreferencesController : BaseApiController
    {
        private readonly IPreferencesService _service;

        public PreferencesController(IPreferencesService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetPreferences()
        {
            var response = await _service.GetPreferencesAsync(GetUserId());
            return FromApiResponse(response);
        } 

        [HttpPut]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
        {
            var response = await _service.UpdatePreferencesAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPreferences()
        {
            var response = await _service.ResetPreferencesAsync(GetUserId());
            return FromApiResponse(response);
        }
    }
}