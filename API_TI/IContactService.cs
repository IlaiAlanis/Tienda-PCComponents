using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ContactoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IContactService
    {
        Task<ApiResponse<object>> SendContactMessageAsync(ContactRequest request);
    }
}
