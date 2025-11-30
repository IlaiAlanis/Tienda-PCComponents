using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.ReviewDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<PagedResult<ReviewDto>>> GetProductReviewsAsync(int productoId, int page, int pageSize);
        Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewRequest request, int usuarioId);
        Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, CreateReviewRequest request, int usuarioId);
        Task<ApiResponse<object>> DeleteReviewAsync(int reviewId, int usuarioId);
    }
}
