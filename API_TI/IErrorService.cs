using API_TI.Models.DTOs.ErrorDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IErrorService
    {
        Task<ErrorInfoDto> GetErrorByCodeInfoAsync(int codigo);
        Task LogErrorAsync(int codigo, string technicalDetail, string? endpoint = null, int? usuario = null, string? email = null);
        Task<IEnumerable<ErrorInfoDto>> GetAllAsync();
    }
}
