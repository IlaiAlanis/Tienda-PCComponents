using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("factura")]
[Index("NumeroFactura", Name = "UQ__factura__3DC4B241279E597A", IsUnique = true)]
public partial class Factura
{
    [Key]
    [Column("id_factura")]
    public int IdFactura { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("numero_factura")]
    [StringLength(50)]
    public string NumeroFactura { get; set; } = null!;

    [Column("rfc_cliente")]
    [StringLength(20)]
    public string? RfcCliente { get; set; }

    [Column("razon_social")]
    [StringLength(255)]
    public string? RazonSocial { get; set; }

    [Column("direccion_fiscal")]
    [StringLength(255)]
    public string? DireccionFiscal { get; set; }

    [Column("subtotal", TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [Column("impuestos", TypeName = "decimal(10, 2)")]
    public decimal Impuestos { get; set; }

    [Column("total", TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [Column("tipo_factura")]
    [StringLength(100)]
    public string? TipoFactura { get; set; }

    [Column("serie")]
    [StringLength(10)]
    public string? Serie { get; set; }

    [Column("folio")]
    public int? Folio { get; set; }

    [Column("uuid")]
    [StringLength(100)]
    public string? Uuid { get; set; }

    [Column("xml_factura")]
    public string? XmlFactura { get; set; }

    [Column("pdf_factura")]
    public byte[]? PdfFactura { get; set; }

    [Column("fecha_emision")]
    [Precision(3)]
    public DateTime FechaEmision { get; set; }

    [Column("fecha_cancelacion")]
    [Precision(3)]
    public DateTime? FechaCancelacion { get; set; }

    [ForeignKey("OrdenId")]
    [InverseProperty("Facturas")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Facturas")]
    public virtual Usuario Usuario { get; set; } = null!;
}
