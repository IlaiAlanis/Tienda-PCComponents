using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("categoria")]
[Index("NombreCategoria", Name = "UQ__categori__4EBF6259A1CA3725", IsUnique = true)]
public partial class Categorium
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("nombre_categoria")]
    [StringLength(150)]
    public string NombreCategoria { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Categoria")]
    public virtual ICollection<CategoriaAtributo> CategoriaAtributos { get; set; } = new List<CategoriaAtributo>();

    [InverseProperty("Categoria")]
    public virtual ICollection<ImpuestoRegla> ImpuestoReglas { get; set; } = new List<ImpuestoRegla>();

    [InverseProperty("Categoria")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
