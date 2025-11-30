using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("proveedor")]
public partial class Proveedor
{
    [Key]
    [Column("id_proveedor")]
    public int IdProveedor { get; set; }

    [Column("pais_id")]
    public int PaisId { get; set; }

    [Column("estado_id")]
    public int? EstadoId { get; set; }

    [Column("ciudad_id")]
    public int? CiudadId { get; set; }

    [Column("codigo_postal")]
    [StringLength(20)]
    public string CodigoPostal { get; set; } = null!;

    [Column("nombre_proveedor")]
    [StringLength(255)]
    public string NombreProveedor { get; set; } = null!;

    [Column("nombre_contacto")]
    [StringLength(255)]
    public string? NombreContacto { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string Telefono { get; set; } = null!;

    [Column("correo")]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [Column("direccion")]
    [StringLength(200)]
    public string? Direccion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [ForeignKey("CiudadId")]
    [InverseProperty("Proveedors")]
    public virtual Ciudad? Ciudad { get; set; }

    [ForeignKey("EstadoId")]
    [InverseProperty("Proveedors")]
    public virtual Estado? Estado { get; set; }

    [ForeignKey("PaisId")]
    [InverseProperty("Proveedors")]
    public virtual Pai Pais { get; set; } = null!;

    [InverseProperty("Proveedor")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
