namespace API_TI.Models.DTOs.ReviewDTOs
{
    public class ReviewDto
    {
        public int IdReview { get; set; }
        public int ProductoId { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Verificado { get; set; }
    }
}
