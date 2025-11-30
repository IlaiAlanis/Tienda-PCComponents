using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("devolucion_item")]
public partial class DevolucionItem
{
    [Key]
    [Column("id_devolucion_item")]
    public int IdDevolucionItem { get; set; }

    [Column("devolucion_id")]
    public int DevolucionId { get; set; }

    [Column("orden_item_id")]
    public int OrdenItemId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("producto_intercambio_id")]
    public int? ProductoIntercambioId { get; set; }

    [ForeignKey("DevolucionId")]
    [InverseProperty("DevolucionItems")]
    public virtual Devolucion Devolucion { get; set; } = null!;

    [ForeignKey("OrdenItemId")]
    [InverseProperty("DevolucionItems")]
    public virtual OrdenItem OrdenItem { get; set; } = null!;

    [ForeignKey("ProductoIntercambioId")]
    [InverseProperty("DevolucionItems")]
    public virtual Producto? ProductoIntercambio { get; set; }
}
