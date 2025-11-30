using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.InventarioDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace API_TI.Services.Interfaces
{
    public interface IProductoService
    {
        Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync();
        Task<ApiResponse<ProductoDto>> GetByIdAsync(int id);       
        Task<ApiResponse<ProductoDto>> CreateAsync(CreateProductoDto dto, int usuario);
        Task<ApiResponse<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto, int usuario);
        Task<ApiResponse<bool>> DeleteAsync(int id, int usuario);

        // Variations
        Task<ApiResponse<ProductoVariacionDto>> GetByIdVariationAsync(int idVariacion);
        Task<ApiResponse<ProductoVariacionDto>> CreateVariationAsync(ProductoVariacionCreateDto dto, int usuario);
        Task<ApiResponse<ProductoVariacionDto>> UpdateVariationAsync(int idVariacion, ProductoVariacionCreateDto dto, int usuario);
        Task<ApiResponse<bool>> DeleteVariationAsync(int idVariacion, int usuario);
        Task<ApiResponse<PagedResult<ProductoDto>>> SearchAsync(ProductoSearchRequest request);
        Task<ApiResponse<List<ProductoDto>>> GetFeaturedAsync();
        Task<ApiResponse<StockStatusDto>> CheckStockAsync(int productoId, int cantidad);
        Task RecalculateStockAsync(int productoId);
    }
}
