using API_TI.Models.dbModels;

namespace API_TI.Models.DTOs.CouponDTOs
{
    public class CuponUsadoDto
    {
        public int IdCouponUsado { get; set; }
        public int UsuarioId { get; set; }
        public int OrdenId { get; set; }
        public int ReglaDescuentoId { get; set; }
        public int DescuentoId { get; set; }
        public string? CodigoCupon { get; set; } = null!;
        public bool Aplicado { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public decimal TotalFinal { get; set; }
        public string? NombreDescuento { get; set; }
        public string? TipoValor { get; set; }
        public decimal Valor { get; set; }
        public DateTime FechaUsado { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Orden Orden { get; set; } = null!;
        public ReglaDescuento ReglaDescuento { get; set; } = null!;

    }
}
