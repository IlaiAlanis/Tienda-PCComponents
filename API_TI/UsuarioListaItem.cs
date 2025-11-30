using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario_lista_item")]
public partial class UsuarioListaItem
{
    [Key]
    [Column("id_item")]
    public int IdItem { get; set; }

    [Column("lista_id")]
    public int ListaId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("variacion_id")]
    public int? VariacionId { get; set; }

    [Column("cantidad_deseada")]
    public int CantidadDeseada { get; set; }

    [Column("prioridad")]
    public int? Prioridad { get; set; }

    [Column("notas")]
    [StringLength(500)]
    public string? Notas { get; set; }

    [Column("fecha_agregado")]
    [Precision(3)]
    public DateTime FechaAgregado { get; set; }

    [ForeignKey("ListaId")]
    [InverseProperty("UsuarioListaItems")]
    public virtual UsuarioListum Lista { get; set; } = null!;

    [ForeignKey("ProductoId")]
    [InverseProperty("UsuarioListaItems")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("VariacionId")]
    [InverseProperty("UsuarioListaItems")]
    public virtual ProductoVariacion? Variacion { get; set; }
}
