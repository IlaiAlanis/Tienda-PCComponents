using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CotizacionDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
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
    public class AddressController : BaseApiController
    {
        private readonly IDireccionService _direccionService;
        private readonly IGooglePlacesService _googlePlacesService;

        public AddressController(
            IDireccionService direccionService,
            IGooglePlacesService googlePlacesService
        )
        {
            _direccionService = direccionService;
            _googlePlacesService = googlePlacesService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost("validate-place")]
        public async Task<IActionResult> ValidatePlace([FromBody] ValidatePlaceRequest request)
        {
            var response = await _googlePlacesService.ValidatePlaceIdAsync(request.PlaceId);
            return FromApiResponse(response);
        }

        [HttpGet("place-details/{placeId}")]
        public async Task<IActionResult> GetPlaceDetails(string placeId)
        {
            var response = await _googlePlacesService.GetAddressDetailsAsync(placeId);
            return FromApiResponse(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var response = await _direccionService.GetUserAddressesAsync(GetUserId());
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

        [HttpGet("search-by-postal-code/{codigoPostal}")]
        public async Task<IActionResult> SearchByPostalCode(string codigoPostal)
        {
            var response = await _googlePlacesService.SearchByPostalCodeAsync(codigoPostal);
            return FromApiResponse(response);
        }
    }
}
