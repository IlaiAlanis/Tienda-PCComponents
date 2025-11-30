using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("coupon_usado")]
[Index("UsuarioId", Name = "IX_coupon_usado_usuario")]
[Index("ReglaDescuentoId", Name = "UQ__coupon_u__076DC2BBEAE4108D", IsUnique = true)]
[Index("UsuarioId", Name = "UQ__coupon_u__2ED7D2AE535A2227", IsUnique = true)]
public partial class CouponUsado
{
    [Key]
    [Column("id_coupon_usado")]
    public int IdCouponUsado { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("regla_descuento_id")]
    public int ReglaDescuentoId { get; set; }

    [Column("codigo_cupon")]
    [StringLength(50)]
    public string? CodigoCupon { get; set; }

    [Column("descuento_aplicado", TypeName = "decimal(18, 2)")]
    public decimal DescuentoAplicado { get; set; }

    [Column("aplicado")]
    public bool Aplicado { get; set; }

    [Column("tipo_valor")]
    [StringLength(50)]
    public string TipoValor { get; set; } = null!;

    [Column("valor", TypeName = "decimal(18, 2)")]
    public decimal Valor { get; set; }

    [Column("fecha_usado")]
    [Precision(3)]
    public DateTime FechaUsado { get; set; }

    [ForeignKey("OrdenId")]
    [InverseProperty("CouponUsados")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("ReglaDescuentoId")]
    [InverseProperty("CouponUsado")]
    public virtual ReglaDescuento ReglaDescuento { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("CouponUsado")]
    public virtual Usuario Usuario { get; set; } = null!;
}
