using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("envio_estatus_historial")]
public partial class EnvioEstatusHistorial
{
    [Key]
    [Column("id_envio_historial")]
    public int IdEnvioHistorial { get; set; }

    [Column("envio_id")]
    public int EnvioId { get; set; }

    [Column("estatus_envio_id")]
    public int EstatusEnvioId { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("comentario")]
    [StringLength(255)]
    public string? Comentario { get; set; }

    [Column("fecha_cambio")]
    [Precision(3)]
    public DateTime FechaCambio { get; set; }

    [ForeignKey("EnvioId")]
    [InverseProperty("EnvioEstatusHistorials")]
    public virtual Envio Envio { get; set; } = null!;

    [ForeignKey("EstatusEnvioId")]
    [InverseProperty("EnvioEstatusHistorials")]
    public virtual EstatusEnvio EstatusEnvio { get; set; } = null!;
}
