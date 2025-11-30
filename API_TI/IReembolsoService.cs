using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ReembolsoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IReembolsoService
    {
        Task<ApiResponse<RefundDto>> RequestRefundAsync(RefundRequest request, int usuarioId);
        Task<ApiResponse<RefundDto>> ProcessRefundAsync(int reembolsoId, int adminId);
    }
}
