using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CouponDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IDescuentoService
    {
        Task<List<Descuento>> GetApplicableDiscountsAsync(int carritoId, string codigoCupon = null);
        Task<ApiResponse<object>> ValidateCouponAsync(string codigoCupon, int usuarioId);
    }
}
