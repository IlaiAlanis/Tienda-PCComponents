using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("audit_logs")]
[Index("EventType", "FechaCreacion", Name = "IX_audit_logs_tipo_fecha", IsDescending = new[] { false, true })]
[Index("UsuarioId", "FechaCreacion", Name = "IX_audit_logs_usuario_fecha", IsDescending = new[] { false, true })]
public partial class AuditLog
{
    [Key]
    [Column("id_auditoria_evento")]
    public int IdAuditoriaEvento { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("event_type")]
    [StringLength(100)]
    public string EventType { get; set; } = null!;

    [Column("event_data")]
    [StringLength(500)]
    public string? EventData { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    [Unicode(false)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(512)]
    [Unicode(false)]
    public string? UserAgent { get; set; }

    [Column("correlation_id")]
    [StringLength(50)]
    [Unicode(false)]
    public string? CorrelationId { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("AuditLogs")]
    public virtual Usuario? Usuario { get; set; }
}
