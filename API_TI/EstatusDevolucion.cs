using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estatus_devolucion")]
public partial class EstatusDevolucion
{
    [Key]
    [Column("id_estatus_devolucion")]
    public int IdEstatusDevolucion { get; set; }

    [Column("nombre_estatus")]
    [StringLength(100)]
    public string NombreEstatus { get; set; } = null!;

    [Column("codigo")]
    [StringLength(50)]
    [Unicode(false)]
    public string Codigo { get; set; } = null!;
}
