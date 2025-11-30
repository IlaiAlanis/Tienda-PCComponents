using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.FacturaDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IFacturaService
    {
        Task<ApiResponse<FacturaDto>> GenerateInvoiceAsync(CreateFacturaRequest request, int usuarioId);
        Task<ApiResponse<FacturaDto>> GetInvoiceByOrderAsync(int ordenId, int usuarioId);
        Task<ApiResponse<byte[]>> DownloadInvoicePdfAsync(int facturaId, int usuarioId);
    }
}
