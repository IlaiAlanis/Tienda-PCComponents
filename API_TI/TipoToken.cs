using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("tipo_token")]
[Index("NombreTipoToken", Name = "UQ__tipo_tok__AF00178ABC2F2004", IsUnique = true)]
public partial class TipoToken
{
    [Key]
    [Column("id_tipo_token")]
    public int IdTipoToken { get; set; }

    [Column("nombre_tipo_token")]
    [StringLength(150)]
    public string NombreTipoToken { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }
}
