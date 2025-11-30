namespace API_TI.Models.DTOs.ProveedorDTOs
{
    public class ProveedorDto
    {
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string? NombreContacto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string? Direccion { get; set; }
        public bool EstaActivo { get; set; }

    }
}
