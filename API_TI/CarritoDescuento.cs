using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("carrito_descuento")]
public partial class CarritoDescuento
{
    [Key]
    [Column("id_carrito_descuento")]
    public int IdCarritoDescuento { get; set; }

    [Column("carrito_id")]
    public int CarritoId { get; set; }

    [Column("carrito_item_id")]
    public int? CarritoItemId { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("regla_descuento_id")]
    public int? ReglaDescuentoId { get; set; }

    [Column("monto_aplicado", TypeName = "decimal(18, 2)")]
    public decimal MontoAplicado { get; set; }

    [Column("fecha_aplicacion")]
    [Precision(3)]
    public DateTime FechaAplicacion { get; set; }

    [ForeignKey("CarritoId")]
    [InverseProperty("CarritoDescuentos")]
    public virtual Carrito Carrito { get; set; } = null!;

    [ForeignKey("CarritoItemId")]
    [InverseProperty("CarritoDescuentos")]
    public virtual CarritoItem? CarritoItem { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("CarritoDescuentos")]
    public virtual Descuento Descuento { get; set; } = null!;

    [ForeignKey("ReglaDescuentoId")]
    [InverseProperty("CarritoDescuentos")]
    public virtual ReglaDescuento? ReglaDescuento { get; set; }
}
