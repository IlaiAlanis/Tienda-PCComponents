using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("descuento_alcance")]
public partial class DescuentoAlcance
{
    [Key]
    [Column("id_alcance")]
    public int IdAlcance { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("tipo_entidad")]
    [StringLength(20)]
    public string TipoEntidad { get; set; } = null!;

    [Column("entidad_id")]
    public int? EntidadId { get; set; }

    [Column("incluir")]
    public bool Incluir { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("DescuentoAlcances")]
    public virtual Descuento Descuento { get; set; } = null!;
}
