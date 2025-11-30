using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.PagoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IPaymentMethodsService
    {
        Task<ApiResponse<IList<PaymentMethodDto>>> GetUserPaymentMethodsAsync(int usuarioId);
        Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByIdAsync(int usuarioId, int metodoPagoId);
        Task<ApiResponse<PaymentMethodDto>> CreatePaymentMethodAsync(int usuarioId, CreatePaymentMethodRequest request);
        Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(int usuarioId, int metodoPagoId, UpdatePaymentMethodRequest request);
        Task<ApiResponse<object>> DeletePaymentMethodAsync(int usuarioId, int metodoPagoId);
        Task<ApiResponse<PaymentMethodDto>> SetDefaultPaymentMethodAsync(int usuarioId, int metodoPagoId);
    }
}
