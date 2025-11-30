using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("orden")]
[Index("EstatusVentaId", "FechaOrden", Name = "IX_orden_estatus", IsDescending = new[] { false, true })]
[Index("NumeroOrden", Name = "IX_orden_numero")]
[Index("UsuarioId", "FechaOrden", Name = "IX_orden_usuario_fecha", IsDescending = new[] { false, true })]
public partial class Orden
{
    [Key]
    [Column("id_orden")]
    public int IdOrden { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("carrito_id")]
    public int CarritoId { get; set; }

    [Column("metodo_pago_id")]
    public int MetodoPagoId { get; set; }

    [Column("impuesto_id")]
    public int ImpuestoId { get; set; }

    [Column("estatus_venta_id")]
    public int EstatusVentaId { get; set; }

    [Column("direccion_envio_id")]
    public int DireccionEnvioId { get; set; }

    [Column("operador_envio_id")]
    public int? OperadorEnvioId { get; set; }

    [Column("numero_orden")]
    [StringLength(255)]
    public string NumeroOrden { get; set; } = null!;

    [Column("numero_seguimiento")]
    [StringLength(100)]
    public string? NumeroSeguimiento { get; set; }

    [Column("fecha_orden")]
    [Precision(3)]
    public DateTime FechaOrden { get; set; }

    [Column("descuento_total", TypeName = "decimal(18, 2)")]
    public decimal DescuentoTotal { get; set; }

    [Column("impuesto_total", TypeName = "decimal(18, 2)")]
    public decimal ImpuestoTotal { get; set; }

    [Column("costo_envio", TypeName = "decimal(18, 2)")]
    public decimal CostoEnvio { get; set; }

    [Column("subtotal", TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column("total", TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    [Column("referencia_metodo_pago")]
    [StringLength(255)]
    public string? ReferenciaMetodoPago { get; set; }

    [Column("fecha_envio")]
    [Precision(3)]
    public DateTime? FechaEnvio { get; set; }

    [Column("fecha_entrega")]
    [Precision(3)]
    public DateTime? FechaEntrega { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [ForeignKey("CarritoId")]
    [InverseProperty("Ordens")]
    public virtual Carrito Carrito { get; set; } = null!;

    [InverseProperty("Orden")]
    public virtual ICollection<DescuentoUso> DescuentoUsos { get; set; } = new List<DescuentoUso>();

    [InverseProperty("Orden")]
    public virtual ICollection<Devolucion> Devolucions { get; set; } = new List<Devolucion>();

    [ForeignKey("DireccionEnvioId")]
    [InverseProperty("Ordens")]
    public virtual Direccion DireccionEnvio { get; set; } = null!;

    [InverseProperty("Orden")]
    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();

    [ForeignKey("EstatusVentaId")]
    [InverseProperty("Ordens")]
    public virtual EstatusVentum EstatusVenta { get; set; } = null!;

    [InverseProperty("Orden")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [ForeignKey("ImpuestoId")]
    [InverseProperty("Ordens")]
    public virtual Impuesto Impuesto { get; set; } = null!;

    [ForeignKey("MetodoPagoId")]
    [InverseProperty("Ordens")]
    public virtual MetodoPago MetodoPago { get; set; } = null!;

    [ForeignKey("OperadorEnvioId")]
    [InverseProperty("Ordens")]
    public virtual OperadorEnvio? OperadorEnvio { get; set; }

    [InverseProperty("Orden")]
    public virtual ICollection<OrdenDescuento> OrdenDescuentos { get; set; } = new List<OrdenDescuento>();

    [InverseProperty("Orden")]
    public virtual ICollection<OrdenDevolucion> OrdenDevolucions { get; set; } = new List<OrdenDevolucion>();

    [InverseProperty("Orden")]
    public virtual ICollection<OrdenEstadoHistorial> OrdenEstadoHistorials { get; set; } = new List<OrdenEstadoHistorial>();

    [InverseProperty("Orden")]
    public virtual ICollection<OrdenItem> OrdenItems { get; set; } = new List<OrdenItem>();

    [InverseProperty("Orden")]
    public virtual ICollection<PagoTransaccion> PagoTransaccions { get; set; } = new List<PagoTransaccion>();

    [InverseProperty("Orden")]
    public virtual ICollection<Reembolso> Reembolsos { get; set; } = new List<Reembolso>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Ordens")]
    public virtual Usuario Usuario { get; set; } = null!;
}
