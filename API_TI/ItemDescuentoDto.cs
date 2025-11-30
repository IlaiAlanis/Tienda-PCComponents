namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class ItemDescuentoDto
    {
        public int IdCarritoItem { get; set; }
        public int ProductoId { get; set; }
        public decimal MontoDescuentoPorUnidad { get; set; }
        public decimal MontoDescuentoTotal => MontoDescuentoPorUnidad;
    }
}
