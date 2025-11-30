using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto")]
[Index("Sku", Name = "IX_producto_sku")]
[Index("CodigoBarras", Name = "UQ__producto__730FA6ABCEC6B661", IsUnique = true)]
[Index("Sku", Name = "UQ__producto__DDDF4BE734E5ED8D", IsUnique = true)]
public partial class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [Column("proveedor_id")]
    public int ProveedorId { get; set; }

    [Column("marca_id")]
    public int MarcaId { get; set; }

    [Column("nombre_producto")]
    [StringLength(255)]
    public string NombreProducto { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("dimensiones")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Dimensiones { get; set; }

    [Column("es_destacado")]
    public bool EsDestacado { get; set; }

    [Column("precio_base", TypeName = "decimal(18, 2)")]
    public decimal PrecioBase { get; set; }

    [Column("precio_promocional", TypeName = "decimal(18, 2)")]
    public decimal? PrecioPromocional { get; set; }

    [Column("sku")]
    [StringLength(100)]
    public string Sku { get; set; } = null!;

    [Column("codigo_barras")]
    [StringLength(50)]
    public string CodigoBarras { get; set; } = null!;

    [Column("stock_total")]
    public int StockTotal { get; set; }

    [Column("permite_preorden")]
    public bool PermitePreorden { get; set; }

    [Column("fecha_restock")]
    [Precision(3)]
    public DateTime? FechaRestock { get; set; }

    [Column("stock_minimo")]
    public int StockMinimo { get; set; }

    [Column("alerta_bajo_stock")]
    public bool AlertaBajoStock { get; set; }

    [Column("calificacion_promedio", TypeName = "decimal(3, 2)")]
    public decimal? CalificacionPromedio { get; set; }

    [Column("total_resenas")]
    public int TotalResenas { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("fecha_eliminacion")]
    [Precision(3)]
    public DateTime? FechaEliminacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [Column("peso", TypeName = "decimal(10, 2)")]
    public decimal? Peso { get; set; }

    [InverseProperty("Producto")]
    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

    [ForeignKey("CategoriaId")]
    [InverseProperty("Productos")]
    public virtual Categorium Categoria { get; set; } = null!;

    [InverseProperty("ProductoIntercambio")]
    public virtual ICollection<DevolucionItem> DevolucionItems { get; set; } = new List<DevolucionItem>();

    [InverseProperty("Producto")]
    public virtual ICollection<InventarioActual> InventarioActuals { get; set; } = new List<InventarioActual>();

    [InverseProperty("Producto")]
    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    [ForeignKey("MarcaId")]
    [InverseProperty("Productos")]
    public virtual Marca Marca { get; set; } = null!;

    [InverseProperty("Producto")]
    public virtual ICollection<OrdenItem> OrdenItems { get; set; } = new List<OrdenItem>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoHistorialPrecio> ProductoHistorialPrecios { get; set; } = new List<ProductoHistorialPrecio>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoImagen> ProductoImagens { get; set; } = new List<ProductoImagen>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoResena> ProductoResenas { get; set; } = new List<ProductoResena>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoVariacionHistorialPrecio> ProductoVariacionHistorialPrecios { get; set; } = new List<ProductoVariacionHistorialPrecio>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoVariacion> ProductoVariacions { get; set; } = new List<ProductoVariacion>();

    [ForeignKey("ProveedorId")]
    [InverseProperty("Productos")]
    public virtual Proveedor Proveedor { get; set; } = null!;

    [InverseProperty("Producto")]
    public virtual ICollection<UsuarioFavorito> UsuarioFavoritos { get; set; } = new List<UsuarioFavorito>();

    [InverseProperty("Producto")]
    public virtual ICollection<UsuarioListaItem> UsuarioListaItems { get; set; } = new List<UsuarioListaItem>();

    [ForeignKey("ProductoId")]
    [InverseProperty("Productos")]
    public virtual ICollection<Producto> ProductoRelacionados { get; set; } = new List<Producto>();

    [ForeignKey("ProductoRelacionadoId")]
    [InverseProperty("ProductoRelacionados")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
