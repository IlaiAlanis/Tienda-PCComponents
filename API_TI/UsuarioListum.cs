using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_lista")]
public partial class UsuarioListum
{
    [Key]
    [Column("id_lista")]
    public int IdLista { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column("es_publica")]
    public bool EsPublica { get; set; }

    [Column("es_compartible")]
    public bool EsCompartible { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("UsuarioLista")]
    public virtual Usuario Usuario { get; set; } = null!;

    [InverseProperty("Lista")]
    public virtual ICollection<UsuarioListaItem> UsuarioListaItems { get; set; } = new List<UsuarioListaItem>();
}
