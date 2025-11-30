using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.DireccionDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IGooglePlacesService
    {
        Task<ApiResponse<bool>> ValidatePlaceIdAsync(string placeId);
        Task<ApiResponse<DireccionDto>> GetAddressDetailsAsync(string placeId);
        Task<ApiResponse<DireccionDto>> SearchByPostalCodeAsync(string codigoPostal);

    }
}
