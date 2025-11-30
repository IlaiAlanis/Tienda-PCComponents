using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("Faq")]
[Index("EstaActivo", Name = "IX_Faq_EstaActivo")]
public partial class Faq
{
    [Key]
    public int IdFaq { get; set; }

    [StringLength(500)]
    public string Pregunta { get; set; } = null!;

    [StringLength(2000)]
    public string Respuesta { get; set; } = null!;

    [StringLength(50)]
    public string Categoria { get; set; } = null!;

    public int Orden { get; set; }

    public bool EstaActivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
