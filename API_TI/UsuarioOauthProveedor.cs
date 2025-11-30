using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_oauth_proveedor")]
public partial class UsuarioOauthProveedor
{
    [Key]
    [Column("id_usuario_oauth")]
    public int IdUsuarioOauth { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("autenticacion_proveedor_id")]
    public int AutenticacionProveedorId { get; set; }

    [Column("id_usuario_externo")]
    [StringLength(255)]
    public string? IdUsuarioExterno { get; set; }

    [Column("access_token")]
    [MaxLength(500)]
    public byte[]? AccessToken { get; set; }

    [Column("fecha_expiracion")]
    [Precision(3)]
    public DateTime? FechaExpiracion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("AutenticacionProveedorId")]
    [InverseProperty("UsuarioOauthProveedors")]
    public virtual AutenticacionProveedor AutenticacionProveedor { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("UsuarioOauthProveedors")]
    public virtual Usuario Usuario { get; set; } = null!;
}
