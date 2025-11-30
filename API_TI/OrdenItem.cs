using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("orden_item")]
[Index("OrdenId", Name = "IX_orden_item_orden")]
[Index("ProductoId", Name = "IX_orden_item_producto")]
public partial class OrdenItem
{
    [Key]
    [Column("id_orden_item")]
    public int IdOrdenItem { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario", TypeName = "decimal(18, 2)")]
    public decimal PrecioUnitario { get; set; }

    [Column("descuento_aplicado", TypeName = "decimal(18, 2)")]
    public decimal? DescuentoAplicado { get; set; }

    [Column("subtotal", TypeName = "decimal(30, 2)")]
    public decimal? Subtotal { get; set; }

    [Column("es_preorden")]
    public bool EsPreorden { get; set; }

    [Column("fecha_estimada_entrega")]
    [Precision(3)]
    public DateTime? FechaEstimadaEntrega { get; set; }

    [InverseProperty("OrdenItem")]
    public virtual ICollection<DevolucionItem> DevolucionItems { get; set; } = new List<DevolucionItem>();

    [ForeignKey("OrdenId")]
    [InverseProperty("OrdenItems")]
    public virtual Orden Orden { get; set; } = null!;

    [InverseProperty("OrdenItem")]
    public virtual ICollection<OrdenDescuento> OrdenDescuentos { get; set; } = new List<OrdenDescuento>();

    [InverseProperty("OrdenItem")]
    public virtual ICollection<OrdenDevolucion> OrdenDevolucions { get; set; } = new List<OrdenDevolucion>();

    [ForeignKey("ProductoId")]
    [InverseProperty("OrdenItems")]
    public virtual Producto Producto { get; set; } = null!;
}
