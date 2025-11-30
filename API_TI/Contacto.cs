using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("Contacto")]
[Index("Email", Name = "IX_Contacto_Email")]
[Index("FechaCreacion", Name = "IX_Contacto_FechaCreacion", AllDescending = true)]
[Index("Leido", Name = "IX_Contacto_Leido")]
public partial class Contacto
{
    [Key]
    public int IdContacto { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string Motivo { get; set; } = null!;

    [StringLength(2000)]
    public string Mensaje { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public bool Leido { get; set; }

    [StringLength(2000)]
    public string? Respuesta { get; set; }

    public DateTime? FechaRespuesta { get; set; }
}
