using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.InventarioDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IInventarioService
    {
        Task<ApiResponse<object>> AdjustStockAsync(AdjustStockRequest request, int usuarioId);
    }
}
