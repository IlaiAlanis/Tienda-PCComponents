using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto_variacion_historial_precio")]
public partial class ProductoVariacionHistorialPrecio
{
    [Key]
    [Column("id_historial_variacion_precio")]
    public int IdHistorialVariacionPrecio { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("precio_anterior", TypeName = "decimal(18, 2)")]
    public decimal PrecioAnterior { get; set; }

    [Column("precio_nuevo", TypeName = "decimal(18, 2)")]
    public decimal PrecioNuevo { get; set; }

    [Column("motivo")]
    [StringLength(255)]
    public string? Motivo { get; set; }

    [Column("fuente_cambio")]
    [StringLength(50)]
    public string? FuenteCambio { get; set; }

    [Column("fecha_cambio")]
    [Precision(3)]
    public DateTime FechaCambio { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoVariacionHistorialPrecios")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("ProductoVariacionHistorialPrecios")]
    public virtual Usuario? Usuario { get; set; }
}
