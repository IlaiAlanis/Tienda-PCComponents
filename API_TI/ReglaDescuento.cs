using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("regla_descuento")]
[Index("CodigoCupon", Name = "UQ__regla_de__96C7773BDA59EB37", IsUnique = true)]
public partial class ReglaDescuento
{
    [Key]
    [Column("id_regla")]
    public int IdRegla { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("codigo_cupon")]
    [StringLength(50)]
    public string? CodigoCupon { get; set; }

    [Column("valor", TypeName = "decimal(10, 2)")]
    public decimal Valor { get; set; }

    [Column("requiere_autenticacion")]
    public bool RequiereAutenticacion { get; set; }

    [Column("solo_nuevos_usuarios")]
    public bool SoloNuevosUsuarios { get; set; }

    [Column("cantidad_min_productos")]
    public int? CantidadMinProductos { get; set; }

    [Column("monto_minimo_compra", TypeName = "decimal(18, 2)")]
    public decimal? MontoMinimoCompra { get; set; }

    [Column("limite_usos_total")]
    public int? LimiteUsosTotal { get; set; }

    [Column("limite_usos_por_usuario")]
    public int? LimiteUsosPorUsuario { get; set; }

    [Column("usos_actuales")]
    public int UsosActuales { get; set; }

    [Column("fecha_inicio")]
    [Precision(3)]
    public DateTime FechaInicio { get; set; }

    [Column("fecha_fin")]
    [Precision(3)]
    public DateTime FechaFin { get; set; }

    [Column("dias_semana")]
    [StringLength(20)]
    [Unicode(false)]
    public string? DiasSemana { get; set; }

    [Column("hora_inicio")]
    public TimeOnly? HoraInicio { get; set; }

    [Column("hora_fin")]
    public TimeOnly? HoraFin { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("ReglaDescuento")]
    public virtual ICollection<CarritoDescuento> CarritoDescuentos { get; set; } = new List<CarritoDescuento>();

    [ForeignKey("DescuentoId")]
    [InverseProperty("ReglaDescuentos")]
    public virtual Descuento Descuento { get; set; } = null!;
}
