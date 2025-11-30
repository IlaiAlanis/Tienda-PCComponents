namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class DescuentoObjetivoDto
    {
        public int IdDescuentoObjetivo { get; set; }
        public int DescuentoId { get; set; }
        public string TipoObjetivo { get; set; } = null!;

        public int ObjetivoId { get; set; }
    }
}
