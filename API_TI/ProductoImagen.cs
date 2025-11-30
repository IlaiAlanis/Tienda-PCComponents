using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto_imagen")]
[Index("ProductoId", "EsPrincipal", Name = "IX_producto_imagen_principal")]
public partial class ProductoImagen
{
    [Key]
    [Column("id_imagen")]
    public int IdImagen { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("url_imagen")]
    [StringLength(500)]
    public string UrlImagen { get; set; } = null!;

    [Column("es_principal")]
    public bool EsPrincipal { get; set; }

    [Column("orden")]
    public int? Orden { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoImagens")]
    public virtual Producto Producto { get; set; } = null!;
}
