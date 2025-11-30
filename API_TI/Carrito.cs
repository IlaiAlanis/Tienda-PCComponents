using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("carrito")]
public partial class Carrito
{
    [Key]
    [Column("id_carrito")]
    public int IdCarrito { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("estatus_venta_id")]
    public int EstatusVentaId { get; set; }

    [Column("cantidad_total")]
    public int CantidadTotal { get; set; }

    [Column("descuento_total", TypeName = "decimal(18, 2)")]
    public decimal DescuentoTotal { get; set; }

    [Column("subtotal", TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [Column("ultima_actividad")]
    [Precision(3)]
    public DateTime UltimaActividad { get; set; }

    [InverseProperty("Carrito")]
    public virtual ICollection<CarritoDescuento> CarritoDescuentos { get; set; } = new List<CarritoDescuento>();

    [InverseProperty("Carrito")]
    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

    [ForeignKey("EstatusVentaId")]
    [InverseProperty("Carritos")]
    public virtual EstatusVentum EstatusVenta { get; set; } = null!;

    [InverseProperty("Carrito")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Carritos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
