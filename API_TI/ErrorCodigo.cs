using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("error_codigo")]
public partial class ErrorCodigo
{
    [Key]
    [Column("codigo")]
    public int Codigo { get; set; }

    [Column("severidad_id")]
    public int SeveridadId { get; set; }

    [Column("categoria")]
    [StringLength(50)]
    public string Categoria { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("mensaje")]
    [StringLength(255)]
    public string Mensaje { get; set; } = null!;

    [Column("activo")]
    public bool Activo { get; set; }

    [InverseProperty("CodigoNavigation")]
    public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();

    [ForeignKey("SeveridadId")]
    [InverseProperty("ErrorCodigos")]
    public virtual Severidad Severidad { get; set; } = null!;
}
