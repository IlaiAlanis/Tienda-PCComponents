using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.WishlistDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<ApiResponse<WishlistDto>> GetWishlistAsync(int usuarioId);
        Task<ApiResponse<WishlistDto>> AddItemAsync(int usuarioId, int productoId);
        Task<ApiResponse<WishlistDto>> RemoveItemAsync(int usuarioId, int productoId);

    }
}
