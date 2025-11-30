using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("operador_envio")]
[Index("CodigoOperador", Name = "UQ__operador__2C1428C11CE29F28", IsUnique = true)]
public partial class OperadorEnvio
{
    [Key]
    [Column("id_operador_envio")]
    public int IdOperadorEnvio { get; set; }

    [Column("nombre_operador")]
    [StringLength(150)]
    public string? NombreOperador { get; set; }

    [Column("codigo_operador")]
    [StringLength(50)]
    public string? CodigoOperador { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string Telefono { get; set; } = null!;

    [Column("correo")]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [Column("sitio_web")]
    [StringLength(150)]
    public string? SitioWeb { get; set; }

    [Column("nota")]
    [StringLength(255)]
    public string? Nota { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("OperadorEnvio")]
    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();

    [InverseProperty("OperadorEnvio")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}
