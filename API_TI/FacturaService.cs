using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.FacturaDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class FacturaService : BaseService, IFacturaService
    {
        private readonly TiPcComponentsContext _context;

        public FacturaService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<FacturaDto>> GenerateInvoiceAsync(CreateFacturaRequest request, int usuarioId)
        {
            try
            {
                var orden = await _context.Ordens
                    .Include(o => o.OrdenItems)
                    .Include(o => o.EstatusVenta)
                    .FirstOrDefaultAsync(o => o.IdOrden == request.OrdenId && o.UsuarioId == usuarioId);

                if (orden == null)
                    return await ReturnErrorAsync<FacturaDto>(1000);

                if (orden.EstatusVenta.Codigo != "PAID")
                    return await ReturnErrorAsync<FacturaDto>(704, "Solo se puede facturar órdenes pagadas");

                // Check if already invoiced
                if (await _context.Facturas.AnyAsync(f => f.OrdenId == request.OrdenId))
                    return await ReturnErrorAsync<FacturaDto>(5, "Orden ya tiene factura");

                var nextFolio = await GetNextFolioAsync(request.TipoFactura);

                var factura = new Factura
                {
                    OrdenId = request.OrdenId,
                    TipoFactura = request.TipoFactura,
                    Serie = "A",
                    Folio = nextFolio,
                    Subtotal = orden.Subtotal,
                    Impuestos = orden.ImpuestoTotal,
                    Total = orden.Total,
                    Uuid = Guid.NewGuid().ToString(),
                    FechaEmision = DateTime.UtcNow
                };

                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                await AuditAsync("Invoice.Generated", new { FacturaId = factura.IdFactura, OrdenId = request.OrdenId });

                return ApiResponse<FacturaDto>.Ok(new FacturaDto
                {
                    IdFactura = factura.IdFactura,
                    OrdenId = factura.OrdenId,
                    NumeroOrden = orden.NumeroOrden,
                    TipoFactura = factura.TipoFactura,
                    Serie = factura.Serie,
                    Folio = factura.Folio,
                    Subtotal = factura.Subtotal,
                    Impuestos = factura.Impuestos,
                    Total = factura.Total,
                    Uuid = factura.Uuid,
                    FechaEmision = factura.FechaEmision
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<FacturaDto>(9000);
            }
        }

        public async Task<ApiResponse<FacturaDto>> GetInvoiceByOrderAsync(int ordenId, int usuarioId)
        {
            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Orden)
                    .FirstOrDefaultAsync(f => f.OrdenId == ordenId && f.Orden.UsuarioId == usuarioId);

                if (factura == null)
                    return await ReturnErrorAsync<FacturaDto>(5, "Factura no encontrada");

                return ApiResponse<FacturaDto>.Ok(new FacturaDto
                {
                    IdFactura = factura.IdFactura,
                    OrdenId = factura.OrdenId,
                    NumeroOrden = factura.Orden.NumeroOrden,
                    TipoFactura = factura.TipoFactura,
                    Serie = factura.Serie,
                    Folio = factura.Folio,
                    Subtotal = factura.Subtotal,
                    Impuestos = factura.Impuestos,
                    Total = factura.Total,
                    Uuid = factura.Uuid,
                    FechaEmision = factura.FechaEmision,
                    FechaCancelacion = factura.FechaCancelacion
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<FacturaDto>(9000);
            }
        }

        public async Task<ApiResponse<byte[]>> DownloadInvoicePdfAsync(int facturaId, int usuarioId)
        {
            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Orden)
                    .FirstOrDefaultAsync(f => f.IdFactura == facturaId && f.Orden.UsuarioId == usuarioId);

                if (factura == null)
                    return await ReturnErrorAsync<byte[]>(5);

                // Return stored PDF or generate on-demand
                if (factura.PdfFactura != null)
                    return ApiResponse<byte[]>.Ok(factura.PdfFactura);

                return await ReturnErrorAsync<byte[]>(5, "PDF no disponible");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<byte[]>(9000);
            }
        }

        private async Task<int> GetNextFolioAsync(string tipoFactura)
        {
            var lastFolio = await _context.Facturas
                .Where(f => f.TipoFactura == tipoFactura)
                .MaxAsync(f => (int?)f.Folio) ?? 0;

            return lastFolio + 1;
        }
    }
}
