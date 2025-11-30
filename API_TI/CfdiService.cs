using API_TI.Data;
using API_TI.Models.dbModels;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Xml.Linq;

namespace API_TI.Services.Implementations
{
    public class CfdiService : BaseService, ICfdiService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IConfiguration _config;

        public CfdiService(
            TiPcComponentsContext context,
            IConfiguration config,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _config = config;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateCfdiXmlAsync(int facturaId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Orden)
                    .ThenInclude(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                .Include(f => f.Orden.Usuario)
                .FirstOrDefaultAsync(f => f.IdFactura == facturaId);

            if (factura == null) return null;

            var cfdi = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("cfdi:Comprobante",
                    new XAttribute(XNamespace.Xmlns + "cfdi", "http://www.sat.gob.mx/cfd/4"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute("Version", "4.0"),
                    new XAttribute("Serie", factura.Serie),
                    new XAttribute("Folio", factura.Folio),
                    new XAttribute("Fecha", factura.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss")),
                    new XAttribute("Subtotal", factura.Subtotal),
                    new XAttribute("Total", factura.Total),
                    new XAttribute("Moneda", "MXN"),
                    new XAttribute("TipoDeComprobante", "I"),
                    new XAttribute("LugarExpedicion", _config["Facturacion:CodigoPostal"]),

                    // Emisor
                    new XElement("cfdi:Emisor",
                        new XAttribute("Rfc", _config["Facturacion:RFC"]),
                        new XAttribute("Nombre", _config["Facturacion:RazonSocial"]),
                        new XAttribute("RegimenFiscal", _config["Facturacion:RegimenFiscal"])
                    ),

                    // Receptor
                    new XElement("cfdi:Receptor",
                        new XAttribute("Rfc", "XAXX010101000"), // Generic RFC
                        new XAttribute("Nombre", factura.Orden.Usuario.NombreUsuario),
                        new XAttribute("UsoCFDI", "G03")
                    ),

                    // Conceptos
                    new XElement("cfdi:Conceptos",
                        factura.Orden.OrdenItems.Select(item => new XElement("cfdi:Concepto",
                            new XAttribute("ClaveProdServ", "43230000"),
                            new XAttribute("Cantidad", item.Cantidad),
                            new XAttribute("ClaveUnidad", "H87"),
                            new XAttribute("Descripcion", item.Producto.NombreProducto),
                            new XAttribute("ValorUnitario", item.PrecioUnitario),
                            new XAttribute("Importe", item.PrecioUnitario * item.Cantidad)
                        ))
                    ),

                    // Impuestos
                    new XElement("cfdi:Impuestos",
                        new XAttribute("TotalImpuestosTrasladados", factura.Impuestos),
                        new XElement("cfdi:Traslados",
                            new XElement("cfdi:Traslado",
                                new XAttribute("Impuesto", "002"),
                                new XAttribute("TipoFactor", "Tasa"),
                                new XAttribute("TasaOCuota", "0.160000"),
                                new XAttribute("Importe", factura.Impuestos)
                            )
                        )
                    )
                )
            );

            var xml = cfdi.ToString();

            // Update factura
            factura.XmlFactura = xml;
            await _context.SaveChangesAsync();

            return xml;
        }

        public async Task<byte[]> GeneratePdfAsync(int facturaId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Orden)
                    .ThenInclude(o => o.OrdenItems)
                        .ThenInclude(oi => oi.Producto)
                .Include(f => f.Orden.Usuario)
                .Include(f => f.Orden.DireccionEnvio)
                .FirstOrDefaultAsync(f => f.IdFactura == facturaId);

            if (factura == null) return null;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, factura));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();

            // Save to DB
            factura.PdfFactura = pdf;
            await _context.SaveChangesAsync();

            return pdf;
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(_config["Facturacion:RazonSocial"]).FontSize(20).SemiBold();
                    column.Item().Text($"RFC: {_config["Facturacion:RFC"]}");
                });
            });
        }

        private void ComposeContent(IContainer container, Factura factura)
        {
            container.Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Factura: {factura.Serie}-{factura.Folio}").FontSize(16).SemiBold();
                    row.RelativeItem().AlignRight().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}");
                });

                column.Item().Text("Cliente").FontSize(14).SemiBold();
                column.Item().Text(factura.Orden.Usuario.NombreUsuario);
                column.Item().Text(factura.Orden.Usuario.Correo);

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Producto");
                        header.Cell().Element(CellStyle).AlignRight().Text("Cantidad");
                        header.Cell().Element(CellStyle).AlignRight().Text("Precio");
                        header.Cell().Element(CellStyle).AlignRight().Text("Total");
                    });

                    foreach (var item in factura.Orden.OrdenItems)
                    {
                        table.Cell().Element(CellStyle).Text(item.Producto.NombreProducto);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Cantidad.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.PrecioUnitario:N2}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.PrecioUnitario * item.Cantidad:N2}");
                    }
                });

                column.Item().AlignRight().Column(summary =>
                {
                    summary.Item().Text($"Subtotal: ${factura.Subtotal:N2}");
                    summary.Item().Text($"IVA: ${factura.Impuestos:N2}");
                    summary.Item().Text($"Total: ${factura.Total:N2}").FontSize(16).SemiBold();
                });

                column.Item().PaddingTop(20).Text($"UUID: {factura.Uuid}").FontSize(8);
            });
        }

        private IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);

    }
}