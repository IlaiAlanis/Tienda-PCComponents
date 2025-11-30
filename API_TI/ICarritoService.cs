using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CheckoutDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Interfaces
{
    public interface ICarritoService
    {
        Task<ApiResponse<CarritoDto>> GetCartAsync(int usuarioId);
        Task<ApiResponse<CarritoDto>> AddItemAsync(int usuarioId, AddToCartRequest request);
        Task<ApiResponse<CarritoDto>> UpdateItemAsync(int usuarioId, int itemId, UpdateCartItemRequest request);
        Task<ApiResponse<CarritoDto>> RemoveItemAsync(int usuarioId, int itemId);
        Task<ApiResponse<CarritoDto>> ApplyCouponAsync(int usuarioId, string codigoCupon);
        Task<ApiResponse<CarritoDto>> RemoveCouponAsync(int usuarioId);
        Task<ApiResponse<object>> ClearCartAsync(int usuarioId);

    }
}
