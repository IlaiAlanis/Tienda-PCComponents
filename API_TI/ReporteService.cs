using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ReporteDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;


namespace API_TI.Services.Implementations
{
    public class ReporteService : BaseService, IReporteService
    {
        private readonly TiPcComponentsContext _context;

        public ReporteService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<VentasReporteDto>> GetSalesReportAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ordenes = await _context.Ordens
                    .Include(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                    .Where(o => o.FechaOrden >= fechaInicio && o.FechaOrden <= fechaFin)
                    .Where(o => o.EstatusVentaId >= 4) // Paid orders
                    .ToListAsync();

                var ventasPorDia = ordenes
                    .GroupBy(o => o.FechaOrden.Date)
                    .Select(g => new VentaPorDiaDto
                    {
                        Fecha = g.Key,
                        Ordenes = g.Count(),
                        Total = g.Sum(o => o.Total)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList();

                var productosMasVendidos = ordenes
                    .SelectMany(o => o.OrdenItems)
                    .GroupBy(oi => new { oi.ProductoId, oi.Producto.NombreProducto })
                    .Select(g => new ProductoMasVendidoDto
                    {
                        ProductoId = g.Key.ProductoId,
                        NombreProducto = g.Key.NombreProducto,
                        CantidadVendida = g.Sum(oi => oi.Cantidad),
                        TotalVentas = g.Sum(oi => oi.PrecioUnitario * oi.Cantidad)
                    })
                    .OrderByDescending(p => p.TotalVentas)
                    .Take(10)
                    .ToList();

                var reporte = new VentasReporteDto
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalOrdenes = ordenes.Count,
                    VentasTotales = ordenes.Sum(o => o.Total),
                    PromedioOrden = ordenes.Any() ? ordenes.Average(o => o.Total) : 0,
                    VentasPorDia = ventasPorDia,
                    ProductosMasVendidos = productosMasVendidos
                };

                return ApiResponse<VentasReporteDto>.Ok(reporte);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<VentasReporteDto>(9000);
            }
        }

        public async Task<ApiResponse<InventarioReporteDto>> GetInventoryReportAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.EstaActivo)
                    .ToListAsync();

                var stockMinimo = 10;
                var productosBajoStock = productos
                    .Where(p => p.StockTotal < stockMinimo && p.StockTotal > 0)
                    .Select(p => new ProductoBajoStockDto
                    {
                        ProductoId = p.IdProducto,
                        NombreProducto = p.NombreProducto,
                        StockActual = p.StockTotal,
                        StockMinimo = stockMinimo
                    })
                    .ToList();

                var reporte = new InventarioReporteDto
                {
                    TotalProductos = productos.Count,
                    ProductosBajoStock = productosBajoStock.Count,
                    ProductosAgotados = productos.Count(p => p.StockTotal == 0),
                    ValorInventarioTotal = productos.Sum(p => p.PrecioBase * p.StockTotal),
                    ProductosCriticos = productosBajoStock
                };

                return ApiResponse<InventarioReporteDto>.Ok(reporte);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<InventarioReporteDto>(9000);
            }
        }

        public async Task<ApiResponse<byte[]>> ExportSalesReportAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var reporteResponse = await GetSalesReportAsync(fechaInicio, fechaFin);
                if (!reporteResponse.Success) return await ReturnErrorAsync<byte[]>(reporteResponse.Error.Code);

                var reporte = reporteResponse.Data;

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(50);

                        page.Header().Text($"Reporte de Ventas").FontSize(20).SemiBold();
                        page.Content().Column(column =>
                        {
                            column.Spacing(10);
                            column.Item().Text($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}");
                            column.Item().Text($"Total Órdenes: {reporte.TotalOrdenes}");
                            column.Item().Text($"Ventas Totales: ${reporte.VentasTotales:N2}");
                            column.Item().Text($"Promedio por Orden: ${reporte.PromedioOrden:N2}");

                            column.Item().PaddingTop(20).Text("Productos Más Vendidos").FontSize(16).SemiBold();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Producto");
                                    header.Cell().AlignRight().Text("Cantidad");
                                    header.Cell().AlignRight().Text("Total");
                                });

                                foreach (var producto in reporte.ProductosMasVendidos)
                                {
                                    table.Cell().Text(producto.NombreProducto);
                                    table.Cell().AlignRight().Text(producto.CantidadVendida.ToString());
                                    table.Cell().AlignRight().Text($"${producto.TotalVentas:N2}");
                                }
                            });
                        });
                    });
                }).GeneratePdf();

                return ApiResponse<byte[]>.Ok(pdf);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<byte[]>(9000);
            }
        }
    }
}
