using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("marca")]
[Index("NombreMarca", Name = "UQ__marca__6059F572EE789E12", IsUnique = true)]
public partial class Marca
{
    [Key]
    [Column("id_marca")]
    public int IdMarca { get; set; }

    [Column("nombre_marca")]
    [StringLength(150)]
    public string NombreMarca { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Marca")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
