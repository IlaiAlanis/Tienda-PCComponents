using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class CreateDescuentoRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string NombreDescuento { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El tipo de descuento es requerido")]
        public string TipoDescuento { get; set; } // "Porcentaje" o "MontoFijo"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
        public decimal Valor { get; set; }

        [StringLength(50)]
        public string? CodigoCupon { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public int? LimiteUsosTotal { get; set; }

        public int? LimiteUsosPorUsuario { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MontoMinimo { get; set; }

        public bool SoloNuevosUsuarios { get; set; } = false;

        public bool EstaActivo { get; set; } = true;

        public int? Prioridad { get; set; } = 1;

        // Scope
        public string? TipoEntidad { get; set; } = "GLOBAL"; // GLOBAL, PRODUCTO, CATEGORIA, MARCA
        public int? EntidadId { get; set; }
    }
}
