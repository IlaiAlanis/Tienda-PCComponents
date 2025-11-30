using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.PagoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IStripeService
    {
        Task<ApiResponse<PaymentIntentResponse>> CreatePaymentIntentAsync(int ordenId, decimal monto);
        Task<ApiResponse<PagoTransaccionDto>> ConfirmPaymentAsync(int ordenId, string paymentIntentId);
    }
}
