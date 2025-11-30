using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("descuento")]
public partial class Descuento
{
    [Key]
    [Column("id_descuento")]
    public int IdDescuento { get; set; }

    [Column("nombre_descuento")]
    [StringLength(150)]
    public string NombreDescuento { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column("tipo_descuento")]
    [StringLength(20)]
    public string TipoDescuento { get; set; } = null!;

    [Column("valor", TypeName = "decimal(10, 2)")]
    public decimal Valor { get; set; }

    [Column("valor_maximo", TypeName = "decimal(18, 2)")]
    public decimal? ValorMaximo { get; set; }

    [Column("es_acumulable")]
    public bool EsAcumulable { get; set; }

    [Column("prioridad")]
    public int Prioridad { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Descuento")]
    public virtual ICollection<CarritoDescuento> CarritoDescuentos { get; set; } = new List<CarritoDescuento>();

    [InverseProperty("Descuento")]
    public virtual ICollection<DescuentoAlcance> DescuentoAlcances { get; set; } = new List<DescuentoAlcance>();

    [InverseProperty("Descuento")]
    public virtual ICollection<DescuentoUso> DescuentoUsos { get; set; } = new List<DescuentoUso>();

    [InverseProperty("Descuento")]
    public virtual ICollection<OrdenDescuento> OrdenDescuentos { get; set; } = new List<OrdenDescuento>();

    [InverseProperty("Descuento")]
    public virtual ICollection<ReglaDescuento> ReglaDescuentos { get; set; } = new List<ReglaDescuento>();
}
