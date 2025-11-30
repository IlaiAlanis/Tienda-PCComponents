using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.PagoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IPayPalService
    {
        Task<ApiResponse<PayPalOrderResponse>> CreateOrderAsync(int ordenId, decimal monto);
        Task<ApiResponse<PagoTransaccionDto>> CaptureOrderAsync(int ordenId, string paypalOrderId);
    }
}
