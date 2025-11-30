using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estatus_reembolso")]
public partial class EstatusReembolso
{
    [Key]
    [Column("id_estatus_reembolso")]
    public int IdEstatusReembolso { get; set; }

    [Column("nombre_estatus")]
    [StringLength(50)]
    public string NombreEstatus { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }
}
