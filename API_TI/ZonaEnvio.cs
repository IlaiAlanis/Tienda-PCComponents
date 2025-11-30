using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("zona_envio")]
[Index("CodigoPostalDesde", "CodigoPostalHasta", Name = "IX_zona_envio_codigo_postal")]
public partial class ZonaEnvio
{
    [Key]
    [Column("id_zona")]
    public int IdZona { get; set; }

    [Column("nombre_zona")]
    [StringLength(100)]
    public string NombreZona { get; set; } = null!;

    [Column("pais_nombre")]
    [StringLength(100)]
    public string? PaisNombre { get; set; }

    [Column("estado_nombre")]
    [StringLength(100)]
    public string? EstadoNombre { get; set; }

    [Column("codigo_postal_desde")]
    [StringLength(10)]
    public string? CodigoPostalDesde { get; set; }

    [Column("codigo_postal_hasta")]
    [StringLength(10)]
    public string? CodigoPostalHasta { get; set; }

    [Column("costo_base", TypeName = "decimal(10, 2)")]
    public decimal CostoBase { get; set; }

    [Column("costo_por_kg", TypeName = "decimal(10, 2)")]
    public decimal CostoPorKg { get; set; }

    [Column("costo_gratis_desde", TypeName = "decimal(18, 2)")]
    public decimal? CostoGratisDesde { get; set; }

    [Column("dias_entrega_min")]
    public int DiasEntregaMin { get; set; }

    [Column("dias_entrega_max")]
    public int DiasEntregaMax { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }
}
