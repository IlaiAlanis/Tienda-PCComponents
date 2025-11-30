using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estatus_envio")]
[Index("Codigo", Name = "UQ__estatus___40F9A206ABD40F8C", IsUnique = true)]
public partial class EstatusEnvio
{
    [Key]
    [Column("id_estatus_envio")]
    public int IdEstatusEnvio { get; set; }

    [Column("nombre_estatus_envio")]
    [StringLength(100)]
    public string NombreEstatusEnvio { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("categoria")]
    [StringLength(50)]
    public string Categoria { get; set; } = null!;

    [Column("codigo")]
    [StringLength(50)]
    public string Codigo { get; set; } = null!;

    [Column("orden")]
    public int? Orden { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("EstatusEnvio")]
    public virtual ICollection<EnvioEstatusHistorial> EnvioEstatusHistorials { get; set; } = new List<EnvioEstatusHistorial>();

    [InverseProperty("EstatusEnvio")]
    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();
}
