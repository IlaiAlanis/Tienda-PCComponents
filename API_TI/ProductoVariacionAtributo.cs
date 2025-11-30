using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[PrimaryKey("VariacionId", "AtributoId")]
[Table("producto_variacion_atributo")]
public partial class ProductoVariacionAtributo
{
    [Key]
    [Column("variacion_id")]
    public int VariacionId { get; set; }

    [Key]
    [Column("atributo_id")]
    public int AtributoId { get; set; }

    [Column("valor")]
    [StringLength(255)]
    public string Valor { get; set; } = null!;

    [Column("orden")]
    public int? Orden { get; set; }

    [Column("unidad")]
    [StringLength(50)]
    public string? Unidad { get; set; }

    [ForeignKey("AtributoId")]
    [InverseProperty("ProductoVariacionAtributos")]
    public virtual ProductoAtributo Atributo { get; set; } = null!;

    [ForeignKey("VariacionId")]
    [InverseProperty("ProductoVariacionAtributos")]
    public virtual ProductoVariacion Variacion { get; set; } = null!;
}
