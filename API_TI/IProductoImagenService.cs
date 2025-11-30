using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ProductoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IProductoImagenService
    {
        Task<ApiResponse<List<ProductoImagenDto>>> GetProductImagesAsync(int productoId);
        Task<ApiResponse<ProductoImagenDto>> UploadImageAsync(int productoId, IFormFile file, int usuarioId);
        Task<ApiResponse<object>> DeleteImageAsync(int imagenId, int usuarioId);
        Task<ApiResponse<object>> SetPrimaryImageAsync(int imagenId, int usuarioId);

    }
}
