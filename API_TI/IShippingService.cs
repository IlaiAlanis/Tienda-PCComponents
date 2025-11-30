using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CotizacionDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IShippingService
    {
        Task<ApiResponse<CotizacionesEnvioResponse>> GetShippingQuotesAsync(int userId, CotizarEnvioRequest request);
        Task<ApiResponse<CotizacionEnvioDto>> GetLocalShippingRateAsync(int direccionId, decimal pesoKg);
    }
}
