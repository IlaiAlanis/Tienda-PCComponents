using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")]
    public class ReportController : BaseApiController
    {
        private readonly IReporteService _reporteService;

        public ReportController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var response = await _reporteService.GetSalesReportAsync(fechaInicio, fechaFin);
            return FromApiResponse(response);
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var response = await _reporteService.GetInventoryReportAsync();
            return FromApiResponse(response);
        }

        [HttpGet("sales/export")]
        public async Task<IActionResult> ExportSales([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var response = await _reporteService.ExportSalesReportAsync(fechaInicio, fechaFin);
            if (!response.Success) return FromApiResponse(response);
            return File(response.Data, "application/pdf", $"ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf");
        }
    }
}
