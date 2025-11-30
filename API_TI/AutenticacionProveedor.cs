using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("autenticacion_proveedor")]
[Index("Nombre", Name = "UQ__autentic__72AFBCC69AEDD81E", IsUnique = true)]
public partial class AutenticacionProveedor
{
    [Key]
    [Column("id_autenticacion_proveedor")]
    public int IdAutenticacionProveedor { get; set; }

    [Column("nombre")]
    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string Tipo { get; set; } = null!;

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("AutenticacionProveedor")]
    public virtual ICollection<UsuarioOauthProveedor> UsuarioOauthProveedors { get; set; } = new List<UsuarioOauthProveedor>();

    [InverseProperty("AutenticacionProveedor")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
