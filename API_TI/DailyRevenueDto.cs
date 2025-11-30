namespace API_TI.Models.DTOs.AdminDTOs
{
    public class DailyRevenueDto
    {
        public DateTime Fecha { get; set; }
        public decimal Ventas { get; set; }
        public int Ordenes { get; set; }
    }
}
