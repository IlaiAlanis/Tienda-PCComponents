using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("estatus_pago")]
[Index("NombreEstatusPago", Name = "UQ__estatus___D653B1FD5985A832", IsUnique = true)]
public partial class EstatusPago
{
    [Key]
    [Column("id_estatus_pago")]
    public int IdEstatusPago { get; set; }

    [Column("nombre_estatus_pago")]
    [StringLength(150)]
    public string NombreEstatusPago { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("categoria")]
    [StringLength(50)]
    public string? Categoria { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("EstatusPago")]
    public virtual ICollection<PagoTransaccion> PagoTransaccions { get; set; } = new List<PagoTransaccion>();
}
