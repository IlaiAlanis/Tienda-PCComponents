using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("descuento_uso")]
public partial class DescuentoUso
{
    [Key]
    [Column("id_uso")]
    public int IdUso { get; set; }

    [Column("descuento_id")]
    public int DescuentoId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("orden_id")]
    public int OrdenId { get; set; }

    [Column("codigo_cupon")]
    [StringLength(50)]
    public string? CodigoCupon { get; set; }

    [Column("monto_descuento", TypeName = "decimal(18, 2)")]
    public decimal MontoDescuento { get; set; }

    [Column("fecha_uso")]
    [Precision(3)]
    public DateTime FechaUso { get; set; }

    [ForeignKey("DescuentoId")]
    [InverseProperty("DescuentoUsos")]
    public virtual Descuento Descuento { get; set; } = null!;

    [ForeignKey("OrdenId")]
    [InverseProperty("DescuentoUsos")]
    public virtual Orden Orden { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("DescuentoUsos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
