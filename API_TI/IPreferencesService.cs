using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ConfGlobalDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IPreferencesService
    {
        Task<ApiResponse<PreferencesDto>> GetPreferencesAsync(int usuarioId);
        Task<ApiResponse<PreferencesDto>> UpdatePreferencesAsync(int usuarioId, UpdatePreferencesRequest request);
        Task<ApiResponse<PreferencesDto>> ResetPreferencesAsync(int usuarioId);
    }
}
