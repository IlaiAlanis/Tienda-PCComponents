using API_TI.Models.DTOs.FacturaDTOs;
using API_TI.Services.Implementations;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FacturaController : BaseApiController
    {
        private readonly IFacturaService _facturaService;
        private readonly ICfdiService _cfdiService;

        public FacturaController(
            IFacturaService facturaService,
            ICfdiService cfdiService
        )
        {
            _facturaService = facturaService;
            _cfdiService = cfdiService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] CreateFacturaRequest request)
        {
            var response = await _facturaService.GenerateInvoiceAsync(request, GetUserId());
            return FromApiResponse(response);
        }

        [HttpGet("order/{ordenId}")]
        public async Task<IActionResult> GetByOrder(int ordenId)
        {
            var response = await _facturaService.GetInvoiceByOrderAsync(ordenId, GetUserId());
            return FromApiResponse(response);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var response = await _facturaService.DownloadInvoicePdfAsync(id, GetUserId());
            if (!response.Success) return FromApiResponse(response);
            return File(response.Data, "application/pdf", $"factura_{id}.pdf");
        }

        [HttpGet("{id}/xml")]
        public async Task<IActionResult> DownloadXml(int id)
        {
            var xml = await _cfdiService.GenerateCfdiXmlAsync(id);
            if (xml == null) return NotFound();
            return File(Encoding.UTF8.GetBytes(xml), "text/xml", $"factura_{id}.xml");
        }
    }
}
