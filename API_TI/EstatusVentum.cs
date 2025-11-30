using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estatus_venta")]
[Index("Codigo", Name = "UQ__estatus___40F9A206ABABA597", IsUnique = true)]
public partial class EstatusVentum
{
    [Key]
    [Column("id_estatus_venta")]
    public int IdEstatusVenta { get; set; }

    [Column("nombre_estatus_venta")]
    [StringLength(150)]
    public string NombreEstatusVenta { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("categoria")]
    [StringLength(50)]
    public string Categoria { get; set; } = null!;

    [Column("codigo")]
    [StringLength(50)]
    public string Codigo { get; set; } = null!;

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("EstatusVenta")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("EstatusVenta")]
    public virtual ICollection<OrdenDevolucion> OrdenDevolucions { get; set; } = new List<OrdenDevolucion>();

    [InverseProperty("EstatusVenta")]
    public virtual ICollection<OrdenEstadoHistorial> OrdenEstadoHistorials { get; set; } = new List<OrdenEstadoHistorial>();

    [InverseProperty("EstatusVenta")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}
