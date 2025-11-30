using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("suscripcion_newsletter")]
[Index("Correo", Name = "UQ__suscripc__2A586E0BD4DE37C1", IsUnique = true)]
public partial class SuscripcionNewsletter
{
    [Key]
    [Column("id_suscripcion")]
    public int IdSuscripcion { get; set; }

    [Column("correo")]
    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("fecha_alta")]
    [Precision(3)]
    public DateTime FechaAlta { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("SuscripcionNewsletters")]
    public virtual Usuario? Usuario { get; set; }
}
