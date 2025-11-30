using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class UpdateDescuentoRequest
    {
        [Required]
        public string NombreDescuento { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public string TipoDescuento { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public int? LimiteUsosTotal { get; set; }

        public int? LimiteUsosPorUsuario { get; set; }

        public decimal? MontoMinimo { get; set; }

        public bool EstaActivo { get; set; }
    }
}
