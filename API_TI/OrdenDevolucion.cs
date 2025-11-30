using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("orden_devolucion")]
public partial class OrdenDevolucion
{
    [Key]
    [Column("id_devolucion")]
    public int IdDevolucion { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("orden_item_id")]
    public int? OrdenItemId { get; set; }

    [Column("metodo_reembolso_id")]
    public int? MetodoReembolsoId { get; set; }

    [Column("estatus_venta_id")]
    public int EstatusVentaId { get; set; }

    [Column("motivo")]
    [StringLength(255)]
    public string Motivo { get; set; } = null!;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("monto_reembolso", TypeName = "decimal(18, 2)")]
    public decimal? MontoReembolso { get; set; }

    [Column("fecha_solicitud")]
    [Precision(3)]
    public DateTime FechaSolicitud { get; set; }

    [Column("fecha_resolucion")]
    [Precision(3)]
    public DateTime? FechaResolucion { get; set; }

    [ForeignKey("EstatusVentaId")]
    [InverseProperty("OrdenDevolucions")]
    public virtual EstatusVentum EstatusVenta { get; set; } = null!;

    [ForeignKey("MetodoReembolsoId")]
    [InverseProperty("OrdenDevolucions")]
    public virtual MetodoPago? MetodoReembolso { get; set; }

    [ForeignKey("OrdenId")]
    [InverseProperty("OrdenDevolucions")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("OrdenItemId")]
    [InverseProperty("OrdenDevolucions")]
    public virtual OrdenItem? OrdenItem { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("OrdenDevolucions")]
    public virtual Usuario Usuario { get; set; } = null!;
}
