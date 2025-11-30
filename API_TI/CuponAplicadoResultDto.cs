using API_TI.Models.DTOs.DescuentoDTOs;

namespace API_TI.Models.DTOs.CouponDTOs
{
    public class CuponAplicadoResultDto
    {
        public int DescuentoId { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public List<ItemDescuentoDto> Items { get; set; } = new();
    }
}
