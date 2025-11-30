using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("producto_resena")]
public partial class ProductoResena
{
    [Key]
    [Column("id_resena")]
    public int IdResena { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("calificacion")]
    public int Calificacion { get; set; }

    [Column("comentario")]
    [StringLength(500)]
    public string? Comentario { get; set; }

    [Column("verificado")]
    public bool Verificado { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoResenas")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("ProductoResenas")]
    public virtual Usuario Usuario { get; set; } = null!;
}
