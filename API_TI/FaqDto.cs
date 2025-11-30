namespace API_TI.Models.DTOs.FaqDTOs
{
    public class FaqDto
    {
        public int IdFaq { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public string Categoria { get; set; }
        public int Orden { get; set; }
        public bool EstaActivo { get; set; }
    }
}
