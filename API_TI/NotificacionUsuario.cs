using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("notificacion_usuario")]
public partial class NotificacionUsuario
{
    [Key]
    [Column("id_notificacion")]
    public int IdNotificacion { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("titulo")]
    [StringLength(100)]
    public string Titulo { get; set; } = null!;

    [Column("mensaje")]
    [StringLength(500)]
    public string Mensaje { get; set; } = null!;

    [Column("tipo")]
    [StringLength(50)]
    [Unicode(false)]
    public string Tipo { get; set; } = null!;

    [Column("leido")]
    public bool Leido { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("NotificacionUsuarios")]
    public virtual Usuario Usuario { get; set; } = null!;
}
