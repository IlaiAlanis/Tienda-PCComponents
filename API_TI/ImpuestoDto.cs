namespace API_TI.Models.DTOs.ImpuestoDTOs
{
    public class ImpuestoDto
    {
        public int IdImpuesto { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; }
    }
}
