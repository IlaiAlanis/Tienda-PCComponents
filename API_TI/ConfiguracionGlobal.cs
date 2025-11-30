using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("configuracion_global")]
[Index("Clave", Name = "UQ__configur__71DCA3DBB79205F5", IsUnique = true)]
public partial class ConfiguracionGlobal
{
    [Key]
    [Column("id_configuracion")]
    public int IdConfiguracion { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("clave")]
    [StringLength(255)]
    public string Clave { get; set; } = null!;

    [Column("valor")]
    [StringLength(500)]
    public string? Valor { get; set; }

    [Column("tipo_dato")]
    [StringLength(50)]
    public string? TipoDato { get; set; }

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("ConfiguracionGlobals")]
    public virtual Usuario? Usuario { get; set; }
}
