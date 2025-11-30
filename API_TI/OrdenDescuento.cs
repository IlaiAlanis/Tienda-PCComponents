using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("orden_descuento")]
public partial class OrdenDescuento
{
    [Key]
    [Column("id_orden_descuento")]
    public int IdOrdenDescuento { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("orden_item_id")]
    public int? OrdenItemId { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("monto_aplicado", TypeName = "decimal(18, 2)")]
    public decimal MontoAplicado { get; set; }

    [Column("fecha_aplicacion")]
    [Precision(3)]
    public DateTime FechaAplicacion { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("OrdenDescuentos")]
    public virtual Descuento Descuento { get; set; } = null!;

    [ForeignKey("OrdenId")]
    [InverseProperty("OrdenDescuentos")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("OrdenItemId")]
    [InverseProperty("OrdenDescuentos")]
    public virtual OrdenItem? OrdenItem { get; set; }
}
