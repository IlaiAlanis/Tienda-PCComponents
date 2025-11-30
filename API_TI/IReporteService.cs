using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ReporteDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IReporteService
    {
        Task<ApiResponse<VentasReporteDto>> GetSalesReportAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<ApiResponse<InventarioReporteDto>> GetInventoryReportAsync();
        Task<ApiResponse<byte[]>> ExportSalesReportAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
