using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_favorito")]
public partial class UsuarioFavorito
{
    [Key]
    [Column("id_favorito")]
    public int IdFavorito { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("variacion_id")]
    public int? VariacionId { get; set; }

    [Column("notificar_precio")]
    public bool NotificarPrecio { get; set; }

    [Column("notificar_stock")]
    public bool NotificarStock { get; set; }

    [Column("precio_deseado", TypeName = "decimal(18, 2)")]
    public decimal? PrecioDeseado { get; set; }

    [Column("notas")]
    [StringLength(500)]
    public string? Notas { get; set; }

    [Column("fecha_agregado")]
    [Precision(3)]
    public DateTime FechaAgregado { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("UsuarioFavoritos")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("UsuarioFavoritos")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("VariacionId")]
    [InverseProperty("UsuarioFavoritos")]
    public virtual ProductoVariacion? Variacion { get; set; }
}
