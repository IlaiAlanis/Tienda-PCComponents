using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("envio")]
[Index("EstatusEnvioId", "FechaCreacion", Name = "IX_envio_estatus", IsDescending = new[] { false, true })]
[Index("NumeroGuia", Name = "IX_envio_numero_guia")]
[Index("OrdenId", Name = "IX_envio_orden")]
[Index("NumeroGuia", Name = "UQ__envio__FD7B4CA67697194C", IsUnique = true)]
public partial class Envio
{
    [Key]
    [Column("id_envio")]
    public int IdEnvio { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("operador_envio_id")]
    public int OperadorEnvioId { get; set; }

    [Column("estatus_envio_id")]
    public int EstatusEnvioId { get; set; }

    [Column("numero_guia")]
    [StringLength(100)]
    public string? NumeroGuia { get; set; }

    [Column("costo_envio", TypeName = "decimal(10, 2)")]
    public decimal CostoEnvio { get; set; }

    [Column("peso_total", TypeName = "decimal(10, 2)")]
    public decimal? PesoTotal { get; set; }

    [Column("fecha_recoleccion")]
    [Precision(3)]
    public DateTime? FechaRecoleccion { get; set; }

    [Column("fecha_estimada_entrega")]
    [Precision(3)]
    public DateTime? FechaEstimadaEntrega { get; set; }

    [Column("fecha_entrega")]
    [Precision(3)]
    public DateTime? FechaEntrega { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [InverseProperty("Envio")]
    public virtual ICollection<EnvioEstatusHistorial> EnvioEstatusHistorials { get; set; } = new List<EnvioEstatusHistorial>();

    [ForeignKey("EstatusEnvioId")]
    [InverseProperty("Envios")]
    public virtual EstatusEnvio EstatusEnvio { get; set; } = null!;

    [ForeignKey("OperadorEnvioId")]
    [InverseProperty("Envios")]
    public virtual OperadorEnvio OperadorEnvio { get; set; } = null!;

    [ForeignKey("OrdenId")]
    [InverseProperty("Envios")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Envios")]
    public virtual Usuario Usuario { get; set; } = null!;
}
