using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("impuesto")]
[Index("Codigo", Name = "UQ__impuesto__40F9A206F8CB6489", IsUnique = true)]
public partial class Impuesto
{
    [Key]
    [Column("id_impuesto")]
    public int IdImpuesto { get; set; }

    [Column("nombre")]
    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [Column("codigo")]
    [StringLength(20)]
    public string Codigo { get; set; } = null!;

    [Column("tipo")]
    [StringLength(20)]
    public string Tipo { get; set; } = null!;

    [Column("valor", TypeName = "decimal(10, 4)")]
    public decimal Valor { get; set; }

    [Column("prioridad")]
    public int Prioridad { get; set; }

    [Column("descripcion")]
    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Impuesto")]
    public virtual ICollection<ImpuestoRegla> ImpuestoReglas { get; set; } = new List<ImpuestoRegla>();

    [InverseProperty("Impuesto")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}
