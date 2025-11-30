using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ReembolsoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IDevolucionService
    {

        Task<ApiResponse<DevolucionDto>> RequestReturnAsync(ReturnRequest request, int usuarioId);
        Task<ApiResponse<DevolucionDto>> ProcessReturnAsync(int devolucionId, ProcessReturnRequest request, int adminId);

    }
}
