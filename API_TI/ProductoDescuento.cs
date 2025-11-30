using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[PrimaryKey("ProductoId", "DescuentoId")]
[Table("producto_descuento")]
public partial class ProductoDescuento
{
    [Key]
    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Key]
    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("ProductoDescuentos")]
    public virtual Descuento Descuento { get; set; } = null!;

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoDescuentos")]
    public virtual Producto Producto { get; set; } = null!;
}
