using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("ciudad")]
[Index("IdExterno", Name = "UQ__ciudad__109C46F3A3CF4D31", IsUnique = true)]
public partial class Ciudad
{
    [Key]
    [Column("id_ciudad")]
    public int IdCiudad { get; set; }

    [Column("estado_id")]
    public int EstadoId { get; set; }

    [Column("id_externo")]
    [StringLength(500)]
    public string IdExterno { get; set; } = null!;

    [Column("nombre_ciudad")]
    [StringLength(150)]
    public string NombreCiudad { get; set; } = null!;

    [InverseProperty("Ciudad")]
    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    [ForeignKey("EstadoId")]
    [InverseProperty("Ciudads")]
    public virtual Estado Estado { get; set; } = null!;

    [InverseProperty("Ciudad")]
    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
