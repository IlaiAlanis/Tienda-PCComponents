using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_metodo_pago")]
public partial class UsuarioMetodoPago
{
    [Key]
    [Column("id_metodo_pago")]
    public int IdMetodoPago { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("stripe_payment_method_id")]
    [StringLength(255)]
    public string? StripePaymentMethodId { get; set; }

    [Column("last4")]
    [StringLength(4)]
    public string Last4 { get; set; } = null!;

    [Column("marca")]
    [StringLength(20)]
    public string? Marca { get; set; }

    [Column("titular")]
    [StringLength(100)]
    public string Titular { get; set; } = null!;

    [Column("exp_month")]
    public int ExpMonth { get; set; }

    [Column("exp_year")]
    public int ExpYear { get; set; }

    [Column("es_principal")]
    public bool EsPrincipal { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("UsuarioMetodoPagos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
