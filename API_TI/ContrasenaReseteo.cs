using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("contrasena_reseteo")]
public partial class ContrasenaReseteo
{
    [Key]
    [Column("id_contrasena_reseteo")]
    public int IdContrasenaReseteo { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("codigo")]
    [StringLength(100)]
    public string Codigo { get; set; } = null!;

    [Column("correo")]
    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [Column("usado")]
    public bool Usado { get; set; }

    [Column("fecha_uso")]
    [Precision(3)]
    public DateTime? FechaUso { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_expiracion")]
    [Precision(3)]
    public DateTime? FechaExpiracion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("ContrasenaReseteos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
