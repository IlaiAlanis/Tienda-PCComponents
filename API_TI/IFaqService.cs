using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.FaqDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IFaqService
    {
        Task<ApiResponse<List<FaqDto>>> GetAllFaqsAsync();
        Task<ApiResponse<FaqDto>> GetFaqByIdAsync(int id);
        Task<ApiResponse<FaqDto>> CreateFaqAsync(CreateFaqRequest request);
        Task<ApiResponse<FaqDto>> UpdateFaqAsync(int id, UpdateFaqRequest request);
        Task<ApiResponse<object>> DeleteFaqAsync(int id);
    }
}
