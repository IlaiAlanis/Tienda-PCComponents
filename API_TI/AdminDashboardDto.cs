namespace API_TI.Models.DTOs.AdminDTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosHoy { get; set; }
        public int OrdenesPendientes { get; set; }
        public int OrdenesHoy { get; set; }
        public decimal VentasHoy { get; set; }
        public decimal VentasMes { get; set; }
    }
}
