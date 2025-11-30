using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[PrimaryKey("ProductoId", "VariacionId")]
[Table("inventario_actual")]
public partial class InventarioActual
{
    [Key]
    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Key]
    [Column("variacion_id")]
    public int VariacionId { get; set; }

    [Column("stock_actual")]
    public int StockActual { get; set; }

    [Column("stock_minimo")]
    public int? StockMinimo { get; set; }

    [Column("stock_maximo")]
    public int? StockMaximo { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("InventarioActuals")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("VariacionId")]
    [InverseProperty("InventarioActuals")]
    public virtual ProductoVariacion Variacion { get; set; } = null!;
}
