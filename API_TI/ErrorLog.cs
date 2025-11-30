using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("error_log")]
[Index("Codigo", "Fecha", Name = "IX_error_log_codigo", IsDescending = new[] { false, true })]
[Index("Fecha", Name = "IX_error_log_fecha", AllDescending = true)]
public partial class ErrorLog
{
    [Key]
    [Column("id_error_log")]
    public int IdErrorLog { get; set; }

    [Column("codigo")]
    public int Codigo { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("mensaje")]
    [StringLength(255)]
    public string? Mensaje { get; set; }

    [Column("detalle_tecnico")]
    public string? DetalleTecnico { get; set; }

    [Column("endpoint_app")]
    [StringLength(255)]
    public string? EndpointApp { get; set; }

    [Column("fecha")]
    [Precision(3)]
    public DateTime? Fecha { get; set; }

    [ForeignKey("Codigo")]
    [InverseProperty("ErrorLogs")]
    public virtual ErrorCodigo CodigoNavigation { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("ErrorLogs")]
    public virtual Usuario? Usuario { get; set; }
}
