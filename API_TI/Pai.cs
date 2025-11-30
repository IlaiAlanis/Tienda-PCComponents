using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("pais")]
[Index("IdExterno", Name = "UQ__pais__109C46F3A628BB48", IsUnique = true)]
[Index("NombrePais", Name = "UQ__pais__ACBDCE73C6EC3946", IsUnique = true)]
public partial class Pai
{
    [Key]
    [Column("id_pais")]
    public int IdPais { get; set; }

    [Column("id_externo")]
    [StringLength(50)]
    public string IdExterno { get; set; } = null!;

    [Column("nombre_pais")]
    [StringLength(100)]
    public string NombrePais { get; set; } = null!;

    [Column("codigo")]
    [StringLength(5)]
    public string Codigo { get; set; } = null!;

    [InverseProperty("Pais")]
    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    [InverseProperty("Pais")]
    public virtual ICollection<Estado> Estados { get; set; } = new List<Estado>();

    [InverseProperty("Pais")]
    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
