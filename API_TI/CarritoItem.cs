using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("carrito_item")]
[Index("CarritoId", Name = "IX_carrito_item_carrito")]
public partial class CarritoItem
{
    [Key]
    [Column("id_carrito_item")]
    public int IdCarritoItem { get; set; }

    [Column("carrito_id")]
    public int CarritoId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario", TypeName = "decimal(18, 2)")]
    public decimal PrecioUnitario { get; set; }

    [Column("descuento_aplicado", TypeName = "decimal(18, 2)")]
    public decimal DescuentoAplicado { get; set; }

    [Column("precio_final", TypeName = "decimal(19, 2)")]
    public decimal? PrecioFinal { get; set; }

    [Column("es_preorden")]
    public bool EsPreorden { get; set; }

    [Column("fecha_estimada_entrega")]
    [Precision(3)]
    public DateTime? FechaEstimadaEntrega { get; set; }

    [Column("fecha_reserva")]
    [Precision(3)]
    public DateTime FechaReserva { get; set; }

    [ForeignKey("CarritoId")]
    [InverseProperty("CarritoItems")]
    public virtual Carrito Carrito { get; set; } = null!;

    [InverseProperty("CarritoItem")]
    public virtual ICollection<CarritoDescuento> CarritoDescuentos { get; set; } = new List<CarritoDescuento>();

    [ForeignKey("ProductoId")]
    [InverseProperty("CarritoItems")]
    public virtual Producto Producto { get; set; } = null!;
}
