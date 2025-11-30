using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estado")]
[Index("IdExterno", Name = "UQ__estado__109C46F306197762", IsUnique = true)]
public partial class Estado
{
    [Key]
    [Column("id_estado")]
    public int IdEstado { get; set; }

    [Column("pais_id")]
    public int PaisId { get; set; }

    [Column("id_externo")]
    [StringLength(50)]
    public string IdExterno { get; set; } = null!;

    [Column("nombre_estado")]
    [StringLength(150)]
    public string NombreEstado { get; set; } = null!;

    [InverseProperty("Estado")]
    public virtual ICollection<Ciudad> Ciudads { get; set; } = new List<Ciudad>();

    [InverseProperty("Estado")]
    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    [ForeignKey("PaisId")]
    [InverseProperty("Estados")]
    public virtual Pai Pais { get; set; } = null!;

    [InverseProperty("Estado")]
    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
