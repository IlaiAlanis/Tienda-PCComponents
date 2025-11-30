using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_token")]
[Index("UsuarioId", "FechaExpiracion", Name = "IX_usuario_token_usuario_id")]
public partial class UsuarioToken
{
    [Key]
    [Column("id_token")]
    public int IdToken { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string Tipo { get; set; } = null!;

    [Column("token_hash")]
    [StringLength(255)]
    public string TokenHash { get; set; } = null!;

    [Column("created_by_ip")]
    [StringLength(45)]
    public string CreatedByIp { get; set; } = null!;

    [Column("user_agent")]
    [StringLength(512)]
    public string? UserAgent { get; set; }

    [Column("device_name")]
    [StringLength(150)]
    public string? DeviceName { get; set; }

    [Column("device_fingerprint")]
    [StringLength(64)]
    public string? DeviceFingerprint { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_expiracion")]
    [Precision(3)]
    public DateTime? FechaExpiracion { get; set; }

    [Column("usado")]
    public bool Usado { get; set; }

    [Column("revoked")]
    public bool Revoked { get; set; }

    [Column("replaced_by_token_hash")]
    [StringLength(256)]
    public string? ReplacedByTokenHash { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("UsuarioTokens")]
    public virtual Usuario Usuario { get; set; } = null!;
}
