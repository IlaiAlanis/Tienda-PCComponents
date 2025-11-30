using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("cotizacion_envio")]
public partial class CotizacionEnvio
{
    [Key]
    [Column("id_cotizacion")]
    public int IdCotizacion { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("direccion_id")]
    public int? DireccionId { get; set; }

    [Column("proveedor")]
    [StringLength(50)]
    public string Proveedor { get; set; } = null!;

    [Column("servicio")]
    [StringLength(100)]
    public string? Servicio { get; set; }

    [Column("costo", TypeName = "decimal(10, 2)")]
    public decimal Costo { get; set; }

    [Column("dias_entrega")]
    public int? DiasEntrega { get; set; }

    [Column("peso_kg", TypeName = "decimal(10, 2)")]
    public decimal PesoKg { get; set; }

    [Column("respuesta_api")]
    public string? RespuestaApi { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("DireccionId")]
    [InverseProperty("CotizacionEnvios")]
    public virtual Direccion? Direccion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("CotizacionEnvios")]
    public virtual Usuario? Usuario { get; set; }
}
