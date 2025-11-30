using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto_variacion")]
[Index("CodigoBarras", Name = "UQ__producto__730FA6AB1C1FD8E6", IsUnique = true)]
[Index("Sku", Name = "UQ__producto__DDDF4BE7455ED4B7", IsUnique = true)]
public partial class ProductoVariacion
{
    [Key]
    [Column("id_variacion")]
    public int IdVariacion { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("sku")]
    [StringLength(100)]
    public string? Sku { get; set; }

    [Column("codigo_barras")]
    [StringLength(50)]
    public string CodigoBarras { get; set; } = null!;

    [Column("precio", TypeName = "decimal(18, 2)")]
    public decimal Precio { get; set; }

    [Column("imagen_url")]
    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("stock_reservado")]
    public int StockReservado { get; set; }

    [Column("peso_diferencial", TypeName = "decimal(10, 2)")]
    public decimal? PesoDiferencial { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Variacion")]
    public virtual ICollection<InventarioActual> InventarioActuals { get; set; } = new List<InventarioActual>();

    [InverseProperty("Variacion")]
    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoVariacions")]
    public virtual Producto Producto { get; set; } = null!;

    [InverseProperty("Variacion")]
    public virtual ICollection<ProductoVariacionAtributo> ProductoVariacionAtributos { get; set; } = new List<ProductoVariacionAtributo>();

    [InverseProperty("Variacion")]
    public virtual ICollection<UsuarioFavorito> UsuarioFavoritos { get; set; } = new List<UsuarioFavorito>();

    [InverseProperty("Variacion")]
    public virtual ICollection<UsuarioListaItem> UsuarioListaItems { get; set; } = new List<UsuarioListaItem>();
}
