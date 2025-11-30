using API_TI.Models.dbModels;
using API_TI.Models.DTOs.UsuarioDTOs;

namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class ReglaDescuentoDto
    {
        public int IdRegla { get; set; }
        public int DescuentoId { get; set; }
        public string CodigoCupon { get; set; }
        public bool RequiereAutenticacion { get; set; }
        public bool SoloNuevosUsuarios { get; set; }
        public int? CantidadMinProductos { get; set; }
        public decimal? MontoMinimoCompra { get; set; }
        public int? LimiteUsosTotal { get; set; }
        public int? LimiteUsosPorUsuario { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
