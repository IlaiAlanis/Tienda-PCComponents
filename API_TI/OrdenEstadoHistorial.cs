using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("orden_estado_historial")]
public partial class OrdenEstadoHistorial
{
    [Key]
    [Column("id_historial_orden")]
    public int IdHistorialOrden { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("estatus_venta_id")]
    public int EstatusVentaId { get; set; }

    [Column("fecha_cambio")]
    [Precision(3)]
    public DateTime FechaCambio { get; set; }

    [Column("comentario")]
    [StringLength(255)]
    public string? Comentario { get; set; }

    [ForeignKey("EstatusVentaId")]
    [InverseProperty("OrdenEstadoHistorials")]
    public virtual EstatusVentum EstatusVenta { get; set; } = null!;

    [ForeignKey("OrdenId")]
    [InverseProperty("OrdenEstadoHistorials")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("OrdenEstadoHistorials")]
    public virtual Usuario Usuario { get; set; } = null!;
}
