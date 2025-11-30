using API_TI.Models.ApiResponse;

namespace API_TI.Services.Interfaces
{
    public interface INewsletterService
    {
        Task<ApiResponse<object>> SubscribeAsync(string email);
        Task<ApiResponse<object>> UnsubscribeAsync(string email);
    }
}
