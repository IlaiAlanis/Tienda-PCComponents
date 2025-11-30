using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("devolucion")]
public partial class Devolucion
{
    [Key]
    [Column("id_devolucion")]
    public int IdDevolucion { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("tipo_devolucion")]
    [StringLength(50)]
    [Unicode(false)]
    public string TipoDevolucion { get; set; } = null!;

    [Column("motivo")]
    [StringLength(500)]
    public string Motivo { get; set; } = null!;

    [Column("estatus_devolucion_id")]
    public int EstatusDevolucionId { get; set; }

    [Column("notas_admin")]
    [StringLength(500)]
    public string? NotasAdmin { get; set; }

    [Column("fecha_solicitud")]
    [Precision(3)]
    public DateTime FechaSolicitud { get; set; }

    [Column("fecha_procesado")]
    [Precision(3)]
    public DateTime? FechaProcesado { get; set; }

    [InverseProperty("Devolucion")]
    public virtual ICollection<DevolucionItem> DevolucionItems { get; set; } = new List<DevolucionItem>();

    [ForeignKey("OrdenId")]
    [InverseProperty("Devolucions")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Devolucions")]
    public virtual Usuario Usuario { get; set; } = null!;
}
