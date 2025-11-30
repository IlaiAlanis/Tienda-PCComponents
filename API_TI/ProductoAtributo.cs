using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto_atributo")]
public partial class ProductoAtributo
{
    [Key]
    [Column("id_atributo")]
    public int IdAtributo { get; set; }

    [Column("nombre")]
    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [Column("tipo")]
    [StringLength(50)]
    public string? Tipo { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Atributo")]
    public virtual ICollection<CategoriaAtributo> CategoriaAtributos { get; set; } = new List<CategoriaAtributo>();

    [InverseProperty("Atributo")]
    public virtual ICollection<ProductoVariacionAtributo> ProductoVariacionAtributos { get; set; } = new List<ProductoVariacionAtributo>();
}
