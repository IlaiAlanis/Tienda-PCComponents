using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("inventario_movimiento")]
[Index("ProductoId", "FechaMovimiento", Name = "IX_inventario_movimiento_producto_fecha", IsDescending = new[] { false, true })]
public partial class InventarioMovimiento
{
    [Key]
    [Column("id_movimiento")]
    public int IdMovimiento { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("variacion_id")]
    public int? VariacionId { get; set; }

    [Column("tipo_movimiento_inventario_id")]
    public int TipoMovimientoInventarioId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("stock_anterior")]
    public int StockAnterior { get; set; }

    [Column("stock_nuevo")]
    public int StockNuevo { get; set; }

    [Column("referencia")]
    [StringLength(500)]
    public string? Referencia { get; set; }

    [Column("comentario")]
    [StringLength(500)]
    public string? Comentario { get; set; }

    [Column("fecha_movimiento")]
    [Precision(3)]
    public DateTime FechaMovimiento { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("InventarioMovimientos")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("TipoMovimientoInventarioId")]
    [InverseProperty("InventarioMovimientos")]
    public virtual TipoMovimientoInventario TipoMovimientoInventario { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("InventarioMovimientos")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("VariacionId")]
    [InverseProperty("InventarioMovimientos")]
    public virtual ProductoVariacion? Variacion { get; set; }
}
