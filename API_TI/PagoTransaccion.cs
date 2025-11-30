using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("pago_transaccion")]
public partial class PagoTransaccion
{
    [Key]
    [Column("id_pago")]
    public int IdPago { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("metodo_pago_id")]
    public int MetodoPagoId { get; set; }

    [Column("estatus_pago_id")]
    public int? EstatusPagoId { get; set; }

    [Column("monto", TypeName = "decimal(10, 2)")]
    public decimal Monto { get; set; }

    [Column("referencia_gateway")]
    [StringLength(255)]
    public string? ReferenciaGateway { get; set; }

    [Column("transaccion_gateway_id")]
    [StringLength(100)]
    public string? TransaccionGatewayId { get; set; }

    [Column("respuesta_gateway")]
    public string? RespuestaGateway { get; set; }

    [Column("payment_intent_id")]
    [StringLength(255)]
    public string? PaymentIntentId { get; set; }

    [Column("paypal_order_id")]
    [StringLength(255)]
    public string? PaypalOrderId { get; set; }

    [Column("cliente_ip")]
    [StringLength(45)]
    public string? ClienteIp { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("fecha_transaccion")]
    [Precision(3)]
    public DateTime FechaTransaccion { get; set; }

    [ForeignKey("EstatusPagoId")]
    [InverseProperty("PagoTransaccions")]
    public virtual EstatusPago? EstatusPago { get; set; }

    [ForeignKey("MetodoPagoId")]
    [InverseProperty("PagoTransaccions")]
    public virtual MetodoPago MetodoPago { get; set; } = null!;

    [ForeignKey("OrdenId")]
    [InverseProperty("PagoTransaccions")]
    public virtual Orden Orden { get; set; } = null!;
}
