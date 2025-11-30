namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class DescuentoDetailDto : DescuentoDto
    {
        public int? LimiteUsosPorUsuario { get; set; }
        public decimal? MontoMinimo { get; set; }
        public bool SoloNuevosUsuarios { get; set; }
        public List<AlcanceDto>? Alcances { get; set; }
    }
}
