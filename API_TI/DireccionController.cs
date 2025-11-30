using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Services.Interfaces;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DireccionController : BaseApiController
    {
        private readonly IDireccionService _direccionService;

        public DireccionController(IDireccionService direccionService)
        {
            _direccionService = direccionService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var response = await _direccionService.GetUserAddressesAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddress(int id)
        {
            var response = await _direccionService.GetAddressByIdAsync(GetUserId(), id);
            return FromApiResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateDireccionRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<DireccionDto>.Fail(2, "Datos inválidos", 2));

            var response = await _direccionService.CreateAddressAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateDireccionRequest request)
        {
            var response = await _direccionService.UpdateAddressAsync(GetUserId(), id, request);
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var response = await _direccionService.DeleteAddressAsync(GetUserId(), id);
            return FromApiResponse(response);
        }

        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var response = await _direccionService.SetDefaultAddressAsync(GetUserId(), id);
            return FromApiResponse(response);
        }
    }
}
