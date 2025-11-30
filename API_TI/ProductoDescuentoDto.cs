using API_TI.Models.DTOs.ProductoDTOs;
namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class ProductoDescuentoDto
    {
        public int ProductoId { get; set; }
        public int DescuentoId { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
