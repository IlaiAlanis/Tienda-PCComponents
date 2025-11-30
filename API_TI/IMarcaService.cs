using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.MarcaDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IMarcaService
    {
        Task<ApiResponse<List<MarcaDto>>> GetAllAsync();
        Task<ApiResponse<MarcaDto>> GetByIdAsync(int id);
        Task<ApiResponse<MarcaDto>> CreateAsync(CreateMarcaDto dto);
        Task<ApiResponse<MarcaDto>> UpdateAsync(UpdateMarcaDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);

    }
}
