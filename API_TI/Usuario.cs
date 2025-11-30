using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("usuario")]
[Index("Correo", Name = "IX_usuario_correo")]
[Index("Correo", Name = "UQ__usuario__2A586E0B8D34CF69", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("rol_id")]
    public int RolId { get; set; }

    [Column("autenticacion_proveedor_id")]
    public int AutenticacionProveedorId { get; set; }

    [Column("nombre_usuario")]
    [StringLength(150)]
    public string NombreUsuario { get; set; } = null!;

    [Column("nombre")]
    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [Column("apellido_paterno")]
    [StringLength(100)]
    public string? ApellidoPaterno { get; set; }

    [Column("apellido_materno")]
    [StringLength(100)]
    public string? ApellidoMaterno { get; set; }

    [Column("fecha_nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [Column("correo")]
    [StringLength(255)]
    public string Correo { get; set; } = null!;

    [Column("correo_verificado")]
    public bool CorreoVerificado { get; set; }

    [Column("contrasena_hash")]
    [StringLength(255)]
    public string? ContrasenaHash { get; set; }

    [Column("ultimo_login_usuario")]
    [Precision(3)]
    public DateTime? UltimoLoginUsuario { get; set; }

    [Column("intentos_fallidos_login")]
    public int IntentosFallidosLogin { get; set; }

    [Column("fecha_bloqueo_login")]
    [Precision(3)]
    public DateTime? FechaBloqueoLogin { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    [Precision(3)]
    public DateTime FechaActualizacion { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [ForeignKey("AutenticacionProveedorId")]
    [InverseProperty("Usuarios")]
    public virtual AutenticacionProveedor AutenticacionProveedor { get; set; } = null!;

    [InverseProperty("Usuario")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ConfiguracionGlobal> ConfiguracionGlobals { get; set; } = new List<ConfiguracionGlobal>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ContrasenaReseteo> ContrasenaReseteos { get; set; } = new List<ContrasenaReseteo>();

    [InverseProperty("Usuario")]
    public virtual ICollection<CorreoVerificacion> CorreoVerificacions { get; set; } = new List<CorreoVerificacion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<CotizacionEnvio> CotizacionEnvios { get; set; } = new List<CotizacionEnvio>();

    [InverseProperty("Usuario")]
    public virtual ICollection<DescuentoUso> DescuentoUsos { get; set; } = new List<DescuentoUso>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Devolucion> Devolucions { get; set; } = new List<Devolucion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Usuario")]
    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    [InverseProperty("Usuario")]
    public virtual ICollection<NotificacionUsuario> NotificacionUsuarios { get; set; } = new List<NotificacionUsuario>();

    [InverseProperty("Usuario")]
    public virtual ICollection<OrdenDevolucion> OrdenDevolucions { get; set; } = new List<OrdenDevolucion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<OrdenEstadoHistorial> OrdenEstadoHistorials { get; set; } = new List<OrdenEstadoHistorial>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ProductoHistorialPrecio> ProductoHistorialPrecios { get; set; } = new List<ProductoHistorialPrecio>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ProductoResena> ProductoResenas { get; set; } = new List<ProductoResena>();

    [InverseProperty("Usuario")]
    public virtual ICollection<ProductoVariacionHistorialPrecio> ProductoVariacionHistorialPrecios { get; set; } = new List<ProductoVariacionHistorialPrecio>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Reembolso> Reembolsos { get; set; } = new List<Reembolso>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Rol Rol { get; set; } = null!;

    [InverseProperty("Usuario")]
    public virtual ICollection<SuscripcionNewsletter> SuscripcionNewsletters { get; set; } = new List<SuscripcionNewsletter>();

    [InverseProperty("Usuario")]
    public virtual ICollection<UsuarioFavorito> UsuarioFavoritos { get; set; } = new List<UsuarioFavorito>();

    [InverseProperty("Usuario")]
    public virtual ICollection<UsuarioListum> UsuarioLista { get; set; } = new List<UsuarioListum>();

    [InverseProperty("Usuario")]
    public virtual ICollection<UsuarioMetodoPago> UsuarioMetodoPagos { get; set; } = new List<UsuarioMetodoPago>();

    [InverseProperty("Usuario")]
    public virtual ICollection<UsuarioOauthProveedor> UsuarioOauthProveedors { get; set; } = new List<UsuarioOauthProveedor>();

    [InverseProperty("Usuario")]
    public virtual ICollection<UsuarioToken> UsuarioTokens { get; set; } = new List<UsuarioToken>();
}
