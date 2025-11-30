using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("reembolso")]
public partial class Reembolso
{
    [Key]
    [Column("id_reembolso")]
    public int IdReembolso { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("monto", TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column("motivo")]
    [StringLength(500)]
    public string Motivo { get; set; } = null!;

    [Column("estatus_reembolso_id")]
    public int EstatusReembolsoId { get; set; }

    [Column("fecha_solicitud")]
    [Precision(3)]
    public DateTime? FechaSolicitud { get; set; }

    [Column("fecha_procesado")]
    [Precision(3)]
    public DateTime? FechaProcesado { get; set; }

    [ForeignKey("OrdenId")]
    [InverseProperty("Reembolsos")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Reembolsos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
