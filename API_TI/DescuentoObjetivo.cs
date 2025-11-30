using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("descuento_objetivo")]
public partial class DescuentoObjetivo
{
    [Key]
    [Column("id_descuento_objetivo")]
    public int IdDescuentoObjetivo { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("tipo_objetivo")]
    [StringLength(50)]
    public string TipoObjetivo { get; set; } = null!;

    [Column("objetivo_id")]
    public int ObjetivoId { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("DescuentoObjetivos")]
    public virtual Descuento Descuento { get; set; } = null!;
}
