using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("impuesto_regla")]
public partial class ImpuestoRegla
{
    [Key]
    [Column("id_regla")]
    public int IdRegla { get; set; }

    [Column("impuesto_id")]
    public int ImpuestoId { get; set; }

    [Column("pais_codigo")]
    [StringLength(2)]
    [Unicode(false)]
    public string? PaisCodigo { get; set; }

    [Column("estado_provincia")]
    [StringLength(100)]
    public string? EstadoProvincia { get; set; }

    [Column("ciudad")]
    [StringLength(100)]
    public string? Ciudad { get; set; }

    [Column("codigo_postal")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CodigoPostal { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("es_exento")]
    public bool EsExento { get; set; }

    [Column("fecha_inicio")]
    public DateOnly? FechaInicio { get; set; }

    [Column("fecha_fin")]
    public DateOnly? FechaFin { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("ImpuestoReglas")]
    public virtual Categorium? Categoria { get; set; }

    [ForeignKey("ImpuestoId")]
    [InverseProperty("ImpuestoReglas")]
    public virtual Impuesto Impuesto { get; set; } = null!;
}
