using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("tipo_movimiento_inventario")]
[Index("NombreMovimientoInventario", Name = "UQ__tipo_mov__5C071637A04EA25A", IsUnique = true)]
[Index("NombreMovimientoInventario", Name = "UQ_tipo_movimiento_inventario", IsUnique = true)]
public partial class TipoMovimientoInventario
{
    [Key]
    [Column("id_tipo_movimiento_inventario")]
    public int IdTipoMovimientoInventario { get; set; }

    [Column("nombre_movimiento_inventario")]
    [StringLength(150)]
    public string NombreMovimientoInventario { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("es_entrada")]
    public bool EsEntrada { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("TipoMovimientoInventario")]
    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();
}
