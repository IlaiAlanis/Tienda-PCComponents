using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("severidad")]
public partial class Severidad
{
    [Key]
    [Column("id_severidad")]
    public int IdSeveridad { get; set; }

    [Column("categoria_severidad")]
    [StringLength(150)]
    public string CategoriaSeveridad { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [InverseProperty("Severidad")]
    public virtual ICollection<ErrorCodigo> ErrorCodigos { get; set; } = new List<ErrorCodigo>();
}
