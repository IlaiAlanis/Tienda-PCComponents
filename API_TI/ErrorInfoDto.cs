namespace API_TI.Models.DTOs.ErrorDTOs
{
    public class ErrorInfoDto
    {
        public int Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Clave { get; set; }

        public string Mensaje { get; set; }
        public int Severidad { get; set; }
        public bool EstaActivo { get; set; }
    }
}
