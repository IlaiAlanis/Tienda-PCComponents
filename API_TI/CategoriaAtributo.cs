using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("categoria_atributo")]
[Index("CategoriaId", "AtributoId", Name = "UQ_categoria_atributo", IsUnique = true)]
public partial class CategoriaAtributo
{
    [Key]
    [Column("id_categoria_atributo")]
    public int IdCategoriaAtributo { get; set; }

    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [Column("atributo_id")]
    public int AtributoId { get; set; }

    [Column("es_obligatorio")]
    public bool EsObligatorio { get; set; }

    [Column("orden")]
    public int? Orden { get; set; }

    [ForeignKey("AtributoId")]
    [InverseProperty("CategoriaAtributos")]
    public virtual ProductoAtributo Atributo { get; set; } = null!;

    [ForeignKey("CategoriaId")]
    [InverseProperty("CategoriaAtributos")]
    public virtual Categorium Categoria { get; set; } = null!;
}
