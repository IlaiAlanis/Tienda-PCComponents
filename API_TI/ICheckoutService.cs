using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Models.DTOs.OrdenDTOs;

namespace API_TI.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<ApiResponse<CheckoutSummaryDto>> GetCheckoutSummaryAsync(int usuarioId, int direccionEnvioId);
        Task<ApiResponse<OrdenDto>> ConfirmCheckoutAsync(int usuarioId, ConfirmCheckoutRequest request);
    }
}

