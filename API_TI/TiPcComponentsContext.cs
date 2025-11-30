using System;
using System.Collections.Generic;
using API_TI.Models.dbModels;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Data;

public partial class TiPcComponentsContext : DbContext
{
    public TiPcComponentsContext()
    {
    }

    public TiPcComponentsContext(DbContextOptions<TiPcComponentsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<AutenticacionProveedor> AutenticacionProveedors { get; set; }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<CarritoDescuento> CarritoDescuentos { get; set; }

    public virtual DbSet<CarritoItem> CarritoItems { get; set; }

    public virtual DbSet<CategoriaAtributo> CategoriaAtributos { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Ciudad> Ciudads { get; set; }

    public virtual DbSet<ConfiguracionGlobal> ConfiguracionGlobals { get; set; }

    public virtual DbSet<Contacto> Contactos { get; set; }

    public virtual DbSet<ContrasenaReseteo> ContrasenaReseteos { get; set; }

    public virtual DbSet<CorreoVerificacion> CorreoVerificacions { get; set; }

    public virtual DbSet<CotizacionEnvio> CotizacionEnvios { get; set; }

    public virtual DbSet<Descuento> Descuentos { get; set; }

    public virtual DbSet<DescuentoAlcance> DescuentoAlcances { get; set; }

    public virtual DbSet<DescuentoUso> DescuentoUsos { get; set; }

    public virtual DbSet<Devolucion> Devolucions { get; set; }

    public virtual DbSet<DevolucionItem> DevolucionItems { get; set; }

    public virtual DbSet<Direccion> Direccions { get; set; }

    public virtual DbSet<Envio> Envios { get; set; }

    public virtual DbSet<EnvioEstatusHistorial> EnvioEstatusHistorials { get; set; }

    public virtual DbSet<ErrorCodigo> ErrorCodigos { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<EstatusDevolucion> EstatusDevolucions { get; set; }

    public virtual DbSet<EstatusEnvio> EstatusEnvios { get; set; }

    public virtual DbSet<EstatusPago> EstatusPagos { get; set; }

    public virtual DbSet<EstatusReembolso> EstatusReembolsos { get; set; }

    public virtual DbSet<EstatusVentum> EstatusVenta { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Impuesto> Impuestos { get; set; }

    public virtual DbSet<ImpuestoRegla> ImpuestoReglas { get; set; }

    public virtual DbSet<InventarioActual> InventarioActuals { get; set; }

    public virtual DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<NotificacionUsuario> NotificacionUsuarios { get; set; }

    public virtual DbSet<OperadorEnvio> OperadorEnvios { get; set; }

    public virtual DbSet<Orden> Ordens { get; set; }

    public virtual DbSet<OrdenDescuento> OrdenDescuentos { get; set; }

    public virtual DbSet<OrdenDevolucion> OrdenDevolucions { get; set; }

    public virtual DbSet<OrdenEstadoHistorial> OrdenEstadoHistorials { get; set; }

    public virtual DbSet<OrdenItem> OrdenItems { get; set; }

    public virtual DbSet<PagoTransaccion> PagoTransaccions { get; set; }

    public virtual DbSet<Pai> Pais { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoAtributo> ProductoAtributos { get; set; }

    public virtual DbSet<ProductoHistorialPrecio> ProductoHistorialPrecios { get; set; }

    public virtual DbSet<ProductoImagen> ProductoImagens { get; set; }

    public virtual DbSet<ProductoResena> ProductoResenas { get; set; }

    public virtual DbSet<ProductoVariacion> ProductoVariacions { get; set; }

    public virtual DbSet<ProductoVariacionAtributo> ProductoVariacionAtributos { get; set; }

    public virtual DbSet<ProductoVariacionHistorialPrecio> ProductoVariacionHistorialPrecios { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Reembolso> Reembolsos { get; set; }

    public virtual DbSet<ReglaDescuento> ReglaDescuentos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Severidad> Severidads { get; set; }

    public virtual DbSet<SuscripcionNewsletter> SuscripcionNewsletters { get; set; }

    public virtual DbSet<TipoMovimientoInventario> TipoMovimientoInventarios { get; set; }

    public virtual DbSet<TipoToken> TipoTokens { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioFavorito> UsuarioFavoritos { get; set; }

    public virtual DbSet<UsuarioListaItem> UsuarioListaItems { get; set; }

    public virtual DbSet<UsuarioListum> UsuarioLista { get; set; }

    public virtual DbSet<UsuarioMetodoPago> UsuarioMetodoPagos { get; set; }

    public virtual DbSet<UsuarioOauthProveedor> UsuarioOauthProveedors { get; set; }

    public virtual DbSet<UsuarioToken> UsuarioTokens { get; set; }

    public virtual DbSet<ZonaEnvio> ZonaEnvios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.IdAuditoriaEvento).HasName("PK__audit_lo__C0517563E91F4756");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.AuditLogs).HasConstraintName("Fk_audit_logs_usuario_id_usuario");
        });

        modelBuilder.Entity<AutenticacionProveedor>(entity =>
        {
            entity.HasKey(e => e.IdAutenticacionProveedor).HasName("PK__autentic__21438632BB37F838");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Tipo).HasDefaultValue("local");
        });

        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.IdCarrito).HasName("PK__carrito__83A2AD9C7F06D310");

            entity.HasIndex(e => new { e.UsuarioId, e.EstaActivo }, "IX_carrito_usuario_activo").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UltimaActividad).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.EstatusVenta).WithMany(p => p.Carritos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_estatus_venta_id_estatus_venta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Carritos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_usuario_id_usuario");
        });

        modelBuilder.Entity<CarritoDescuento>(entity =>
        {
            entity.HasKey(e => e.IdCarritoDescuento).HasName("PK__carrito___A3FB0E25B4BE3686");

            entity.Property(e => e.FechaAplicacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Carrito).WithMany(p => p.CarritoDescuentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_descuento_carrito_id_carrito");

            entity.HasOne(d => d.CarritoItem).WithMany(p => p.CarritoDescuentos).HasConstraintName("Fk_carrito_descuento_carrito_item_id_carrito_item");

            entity.HasOne(d => d.Descuento).WithMany(p => p.CarritoDescuentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_descuento_descuento_id_descuento");

            entity.HasOne(d => d.ReglaDescuento).WithMany(p => p.CarritoDescuentos).HasConstraintName("Fk_carrito_descuento_regla_descuento_id_id_regla");
        });

        modelBuilder.Entity<CarritoItem>(entity =>
        {
            entity.HasKey(e => e.IdCarritoItem).HasName("PK__carrito___240DF0BE8B73D7D2");

            entity.Property(e => e.FechaReserva).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrecioFinal).HasComputedColumnSql("([precio_unitario]-[descuento_aplicado])", true);

            entity.HasOne(d => d.Carrito).WithMany(p => p.CarritoItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_item_id_carrito_carrito");

            entity.HasOne(d => d.Producto).WithMany(p => p.CarritoItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_carrito_item_producto_id_producto");
        });

        modelBuilder.Entity<CategoriaAtributo>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaAtributo).HasName("PK__categori__327F2EE2D982EAE6");

            entity.HasOne(d => d.Atributo).WithMany(p => p.CategoriaAtributos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_categoria_atributo_producto");

            entity.HasOne(d => d.Categoria).WithMany(p => p.CategoriaAtributos).HasConstraintName("FK_categoria_atributo_categoria");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__categori__CD54BC5ADA345811");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.HasKey(e => e.IdCiudad).HasName("PK__ciudad__B7DC4CD56490A67D");

            entity.HasOne(d => d.Estado).WithMany(p => p.Ciudads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_ciudad_estado_id_pais");
        });

        modelBuilder.Entity<ConfiguracionGlobal>(entity =>
        {
            entity.HasKey(e => e.IdConfiguracion).HasName("PK__configur__16A13EBDEC55BD99");

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ConfiguracionGlobals).HasConstraintName("Fk_configuracion_global_usuario_id_usuario");
        });

        modelBuilder.Entity<Contacto>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<ContrasenaReseteo>(entity =>
        {
            entity.HasKey(e => e.IdContrasenaReseteo).HasName("PK__contrase__CEA4C91A453EB55A");

            entity.Property(e => e.Correo).HasDefaultValue("");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ContrasenaReseteos).HasConstraintName("FK_contrasena_reseteo_usuario");
        });

        modelBuilder.Entity<CorreoVerificacion>(entity =>
        {
            entity.HasKey(e => e.IdCorreoVerificacion).HasName("PK__correo_v__3325020CD81205FA");

            entity.Property(e => e.Correo).HasDefaultValue("");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CorreoVerificacions).HasConstraintName("FK_correo_verificacion_usuario");
        });

        modelBuilder.Entity<CotizacionEnvio>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK__cotizaci__9713D174292C686F");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Direccion).WithMany(p => p.CotizacionEnvios).HasConstraintName("FK_cotizacion_envio_direccion_id_direccion");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CotizacionEnvios).HasConstraintName("FK_cotizacion_envio_usuario_id_usuario");
        });

        modelBuilder.Entity<Descuento>(entity =>
        {
            entity.HasKey(e => e.IdDescuento).HasName("PK__descuent__4F9A1A803762CA8D");

            entity.HasIndex(e => e.EstaActivo, "IX_descuento_activo").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Prioridad).HasDefaultValue(100);
        });

        modelBuilder.Entity<DescuentoAlcance>(entity =>
        {
            entity.HasKey(e => e.IdAlcance).HasName("PK__descuent__F0D4C0611986CC96");

            entity.Property(e => e.Incluir).HasDefaultValue(true);

            entity.HasOne(d => d.Descuento).WithMany(p => p.DescuentoAlcances).HasConstraintName("FK_descuento_alcance_descuento_id_descuento");
        });

        modelBuilder.Entity<DescuentoUso>(entity =>
        {
            entity.HasKey(e => e.IdUso).HasName("PK__descuent__6AE80FBDF89631C0");

            entity.Property(e => e.FechaUso).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Descuento).WithMany(p => p.DescuentoUsos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_descuento_uso_descuento_id_descuento");

            entity.HasOne(d => d.Orden).WithMany(p => p.DescuentoUsos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_descuento_uso_orden");

            entity.HasOne(d => d.Usuario).WithMany(p => p.DescuentoUsos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_descuento_uso_usuario_id");
        });

        modelBuilder.Entity<Devolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PK__devoluci__0BBAEF8D8A3323FC");

            entity.HasOne(d => d.Orden).WithMany(p => p.Devolucions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__orden__3DE82FB7");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Devolucions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__usuar__3EDC53F0");
        });

        modelBuilder.Entity<DevolucionItem>(entity =>
        {
            entity.HasKey(e => e.IdDevolucionItem).HasName("PK__devoluci__54E82030C5F98545");

            entity.HasOne(d => d.Devolucion).WithMany(p => p.DevolucionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__devol__41B8C09B");

            entity.HasOne(d => d.OrdenItem).WithMany(p => p.DevolucionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__devolucio__orden__42ACE4D4");

            entity.HasOne(d => d.ProductoIntercambio).WithMany(p => p.DevolucionItems).HasConstraintName("FK__devolucio__produ__43A1090D");
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__direccio__25C35D070FDA32E9");

            entity.HasIndex(e => e.GooglePlaceId, "IX_Direccion_GooglePlaceId").HasFilter("([google_PlaceId] IS NOT NULL)");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Ciudad).WithMany(p => p.Direccions).HasConstraintName("Fk_direccion_ciudad_id_ciudad");

            entity.HasOne(d => d.Estado).WithMany(p => p.Direccions).HasConstraintName("Fk_direccion_estado_id_estado");

            entity.HasOne(d => d.Pais).WithMany(p => p.Direccions).HasConstraintName("Fk_direccion_pais_id_pais");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Direccions).HasConstraintName("Fk_direccion_usuario_id_usuario");
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasKey(e => e.IdEnvio).HasName("PK__envio__8C48C8CA5811AA99");

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.EstatusEnvio).WithMany(p => p.Envios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_envio_estatus_envio_id_estatus_envio");

            entity.HasOne(d => d.OperadorEnvio).WithMany(p => p.Envios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_envio_operador_envio_id_operador_envio");

            entity.HasOne(d => d.Orden).WithMany(p => p.Envios).HasConstraintName("Fk_envio_orden_id_");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Envios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_envio_usuario_id_usuario");
        });

        modelBuilder.Entity<EnvioEstatusHistorial>(entity =>
        {
            entity.HasKey(e => e.IdEnvioHistorial).HasName("PK__envio_es__C26AFB5CACF3AA85");

            entity.Property(e => e.FechaCambio).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Envio).WithMany(p => p.EnvioEstatusHistorials).HasConstraintName("Fk_envio_estatus_historial_envio_id_envio");

            entity.HasOne(d => d.EstatusEnvio).WithMany(p => p.EnvioEstatusHistorials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_envio_estatus_historial_estatus_envio_id_");
        });

        modelBuilder.Entity<ErrorCodigo>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PK__error_co__40F9A2079309DBB1");

            entity.Property(e => e.Codigo).ValueGeneratedNever();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.SeveridadId).HasDefaultValue(1);

            entity.HasOne(d => d.Severidad).WithMany(p => p.ErrorCodigos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_error_codigo_severidad_id_severidad");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.IdErrorLog).HasName("PK__error_lo__565A9BB113DAC0BE");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.CodigoNavigation).WithMany(p => p.ErrorLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_error_log_codigo_error_codigo");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ErrorLogs).HasConstraintName("Fk_error_log_usuario_id_usuario");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__estado__86989FB25DD0D381");

            entity.HasOne(d => d.Pais).WithMany(p => p.Estados)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_estado_pais_id_pais");
        });

        modelBuilder.Entity<EstatusDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdEstatusDevolucion).HasName("PK__estatus___A87E7E310686A491");
        });

        modelBuilder.Entity<EstatusEnvio>(entity =>
        {
            entity.HasKey(e => e.IdEstatusEnvio).HasName("PK__estatus___558B51D388D73B74");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<EstatusPago>(entity =>
        {
            entity.HasKey(e => e.IdEstatusPago).HasName("PK__estatus___89D1D33E5729496E");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<EstatusReembolso>(entity =>
        {
            entity.HasKey(e => e.IdEstatusReembolso).HasName("PK__estatus___A877BACD45149901");
        });

        modelBuilder.Entity<EstatusVentum>(entity =>
        {
            entity.HasKey(e => e.IdEstatusVenta).HasName("PK__estatus___E676626A1C69E390");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PK__factura__6C08ED53C5B47B82");

            entity.Property(e => e.FechaEmision).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Orden).WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_factura_orden_id_orden");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_factura_usuario_id_usuario");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasIndex(e => new { e.Categoria, e.Orden }, "IX_Faq_Categoria_Orden").HasFilter("([EstaActivo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Impuesto>(entity =>
        {
            entity.HasKey(e => e.IdImpuesto).HasName("PK__impuesto__8546BDFC88BD2409");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Prioridad).HasDefaultValue(100);
            entity.Property(e => e.Tipo).HasDefaultValue("PORCENTAJE");
        });

        modelBuilder.Entity<ImpuestoRegla>(entity =>
        {
            entity.HasKey(e => e.IdRegla).HasName("PK__impuesto__46D1C192A33E42DE");

            entity.Property(e => e.PaisCodigo).IsFixedLength();

            entity.HasOne(d => d.Categoria).WithMany(p => p.ImpuestoReglas).HasConstraintName("FK_regla_categoria");

            entity.HasOne(d => d.Impuesto).WithMany(p => p.ImpuestoReglas).HasConstraintName("FK_regla_impuesto");
        });

        modelBuilder.Entity<InventarioActual>(entity =>
        {
            entity.HasKey(e => new { e.ProductoId, e.VariacionId }).HasName("PK__inventar__BD553D655F2492AB");

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.InventarioActuals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventario_actual_producto");

            entity.HasOne(d => d.Variacion).WithMany(p => p.InventarioActuals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventario_actual_variacion_producto");
        });

        modelBuilder.Entity<InventarioMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__inventar__2A071C246782A882");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaMovimiento).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.InventarioMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_inventario_movimiento_producto_id_producto");

            entity.HasOne(d => d.TipoMovimientoInventario).WithMany(p => p.InventarioMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_inventario_movimiento_tipo_movimiento_inventario_id_tipo_movimiento_inventario");

            entity.HasOne(d => d.Usuario).WithMany(p => p.InventarioMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_inventario_movimiento_usuario_id_usuario");

            entity.HasOne(d => d.Variacion).WithMany(p => p.InventarioMovimientos).HasConstraintName("Fk_inventario_movimiento_variacion_id_producto_variacion");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK__marca__7E43E99EBCC52176");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK__metodo_p__85BE0EBCD3A8A95C");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<NotificacionUsuario>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__notifica__8270F9A54397F962");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.NotificacionUsuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_notificacion_usuario_usuario_id_usuario");
        });

        modelBuilder.Entity<OperadorEnvio>(entity =>
        {
            entity.HasKey(e => e.IdOperadorEnvio).HasName("PK__operador__0109F5795E699C99");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Orden>(entity =>
        {
            entity.HasKey(e => e.IdOrden).HasName("PK__orden__DD5B8F33F532B4BB");

            entity.Property(e => e.EstatusVentaId).HasDefaultValue(2);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaOrden).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Carrito).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_carrito_id_carrito");

            entity.HasOne(d => d.DireccionEnvio).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_direccion_envio_id_direccion");

            entity.HasOne(d => d.EstatusVenta).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_estatus_venta_id_estatus_venta");

            entity.HasOne(d => d.Impuesto).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_impuesto_id_impuesto");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_metodo_pago_id_metodo_pago");

            entity.HasOne(d => d.OperadorEnvio).WithMany(p => p.Ordens).HasConstraintName("FK_orden_operador_envio_id_operador_envio");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Ordens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_usuario_id_usuario");
        });

        modelBuilder.Entity<OrdenDescuento>(entity =>
        {
            entity.HasKey(e => e.IdOrdenDescuento).HasName("PK__orden_de__3432CEA2BE3F110D");

            entity.Property(e => e.FechaAplicacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Descuento).WithMany(p => p.OrdenDescuentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_descuento_descuento_id_descuento");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenDescuentos).HasConstraintName("Fk_orden_descuento_orden_id_orden");

            entity.HasOne(d => d.OrdenItem).WithMany(p => p.OrdenDescuentos).HasConstraintName("Fk_orden_descuento_orden_item_id_orden_item");
        });

        modelBuilder.Entity<OrdenDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PK__orden_de__0BBAEF8DE7F2EA64");

            entity.Property(e => e.Cantidad).HasDefaultValue(1);
            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.EstatusVenta).WithMany(p => p.OrdenDevolucions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_devolucion_estatus_venta_id_estatus_venta");

            entity.HasOne(d => d.MetodoReembolso).WithMany(p => p.OrdenDevolucions).HasConstraintName("Fk_orden_devolucion_metodo_pago_id_metodo_pago");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenDevolucions).HasConstraintName("Fk_orden_devolucion_orden_id_orden");

            entity.HasOne(d => d.OrdenItem).WithMany(p => p.OrdenDevolucions).HasConstraintName("Fk_orden_devolucion_orden_item_id_orden_item");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OrdenDevolucions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_devolucion_usuario_id_usuario");
        });

        modelBuilder.Entity<OrdenEstadoHistorial>(entity =>
        {
            entity.HasKey(e => e.IdHistorialOrden).HasName("PK__orden_es__45B1AC091EFC9562");

            entity.Property(e => e.FechaCambio).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.EstatusVenta).WithMany(p => p.OrdenEstadoHistorials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orden_estado_historial_estatus_venta_id_estatus_venta");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenEstadoHistorials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orden_estado_historial_orden_id_orden");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OrdenEstadoHistorials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orden_estado_historial_usuario_id_usuario");
        });

        modelBuilder.Entity<OrdenItem>(entity =>
        {
            entity.HasKey(e => e.IdOrdenItem).HasName("PK__orden_it__ADF7D768C6964783");

            entity.Property(e => e.DescuentoAplicado).HasDefaultValue(0m);
            entity.Property(e => e.Subtotal).HasComputedColumnSql("(([precio_unitario]-[descuento_aplicado])*[cantidad])", true);

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenItems).HasConstraintName("Fk_orden_item_orden_id_orden");

            entity.HasOne(d => d.Producto).WithMany(p => p.OrdenItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_orden_item_producto_id_producto");
        });

        modelBuilder.Entity<PagoTransaccion>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__pago_tra__0941B074018EB85D");

            entity.Property(e => e.FechaTransaccion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.EstatusPago).WithMany(p => p.PagoTransaccions).HasConstraintName("Fk_pago_transaccion_estatus_pago_id_estatus_pago");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.PagoTransaccions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_pago_transaccion_metodo_pago_id_metodo_pago");

            entity.HasOne(d => d.Orden).WithMany(p => p.PagoTransaccions).HasConstraintName("Fk_pago_transaccion_orden_id_orden");
        });

        modelBuilder.Entity<Pai>(entity =>
        {
            entity.HasKey(e => e.IdPais).HasName("PK__pais__0941A3A70A489C5F");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__producto__FF341C0DFC3C4124");

            entity.HasIndex(e => e.CategoriaId, "IX_producto_categoria").HasFilter("([esta_activo]=(1))");

            entity.HasIndex(e => e.EsDestacado, "IX_producto_destacado").HasFilter("([esta_activo]=(1) AND [es_destacado]=(1))");

            entity.HasIndex(e => e.MarcaId, "IX_producto_marca").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.AlertaBajoStock).HasDefaultValue(true);
            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.StockMinimo).HasDefaultValue(10);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_categoria_id_categoria");

            entity.HasOne(d => d.Marca).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_marca_id_marca");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_proveedor_id_proveedor");

            entity.HasMany(d => d.ProductoRelacionados).WithMany(p => p.Productos)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductoRelacionado",
                    r => r.HasOne<Producto>().WithMany()
                        .HasForeignKey("ProductoRelacionadoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_producto_relacionado_relacionado"),
                    l => l.HasOne<Producto>().WithMany()
                        .HasForeignKey("ProductoId")
                        .HasConstraintName("FK_producto_relacionado_producto"),
                    j =>
                    {
                        j.HasKey("ProductoId", "ProductoRelacionadoId");
                        j.ToTable("producto_relacionado");
                        j.IndexerProperty<int>("ProductoId").HasColumnName("producto_id");
                        j.IndexerProperty<int>("ProductoRelacionadoId").HasColumnName("producto_relacionado_id");
                    });

            entity.HasMany(d => d.Productos).WithMany(p => p.ProductoRelacionados)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductoRelacionado",
                    r => r.HasOne<Producto>().WithMany()
                        .HasForeignKey("ProductoId")
                        .HasConstraintName("FK_producto_relacionado_producto"),
                    l => l.HasOne<Producto>().WithMany()
                        .HasForeignKey("ProductoRelacionadoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_producto_relacionado_relacionado"),
                    j =>
                    {
                        j.HasKey("ProductoId", "ProductoRelacionadoId");
                        j.ToTable("producto_relacionado");
                        j.IndexerProperty<int>("ProductoId").HasColumnName("producto_id");
                        j.IndexerProperty<int>("ProductoRelacionadoId").HasColumnName("producto_relacionado_id");
                    });
        });

        modelBuilder.Entity<ProductoAtributo>(entity =>
        {
            entity.HasKey(e => e.IdAtributo).HasName("PK__producto__3BB14C8834F5CE5D");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<ProductoHistorialPrecio>(entity =>
        {
            entity.HasKey(e => e.IdHistorialPrecio).HasName("PK__producto__5EDBC272D90F6F9F");

            entity.Property(e => e.FechaCambio).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoHistorialPrecios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_historial_precio_producto_id_producto");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ProductoHistorialPrecios)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Fk_producto_historial_precio_usuario_id_usuario");
        });

        modelBuilder.Entity<ProductoImagen>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__producto__27CC26895F8987B5");

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoImagens).HasConstraintName("FK_producto_imagen_producto");
        });

        modelBuilder.Entity<ProductoResena>(entity =>
        {
            entity.HasKey(e => e.IdResena).HasName("PK__producto__06CD9363BD616F54");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Verificado).HasDefaultValue(true);

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoResenas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_resena_producto_id_producto");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ProductoResenas).HasConstraintName("Fk_producto_resena_usuario_id_usuario");
        });

        modelBuilder.Entity<ProductoVariacion>(entity =>
        {
            entity.HasKey(e => e.IdVariacion).HasName("PK__producto__950135CE8DAE6623");

            entity.HasIndex(e => e.Sku, "IX_producto_variacion_sku").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoVariacions).HasConstraintName("Fk_producto_variacion_producto_id_producto");
        });

        modelBuilder.Entity<ProductoVariacionAtributo>(entity =>
        {
            entity.HasOne(d => d.Atributo).WithMany(p => p.ProductoVariacionAtributos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_variacion_atributo_id_atributo_producto_atributo");

            entity.HasOne(d => d.Variacion).WithMany(p => p.ProductoVariacionAtributos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_producto_variacion_atributo_id_variacion_producto_variacion");
        });

        modelBuilder.Entity<ProductoVariacionHistorialPrecio>(entity =>
        {
            entity.HasKey(e => e.IdHistorialVariacionPrecio).HasName("PK__producto__1B933795743D257A");

            entity.Property(e => e.FechaCambio).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoVariacionHistorialPrecios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_prod_var_hist_precio_producto");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ProductoVariacionHistorialPrecios)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_prod_var_hist_precio_usuario");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__proveedo__8D3DFE28D5BA944A");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Ciudad).WithMany(p => p.Proveedors).HasConstraintName("Fk_proveedor_ciudad_id_ciudad");

            entity.HasOne(d => d.Estado).WithMany(p => p.Proveedors).HasConstraintName("Fk_proveedor_estado_id_estado");

            entity.HasOne(d => d.Pais).WithMany(p => p.Proveedors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_proveedor_pais_id_pais");
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.IdReembolso).HasName("PK__reembols__51DA51365498DF51");

            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Orden).WithMany(p => p.Reembolsos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_reembolso_orden_id_id_orden");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reembolsos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_reembolso_usuario_id_id_usuario");
        });

        modelBuilder.Entity<ReglaDescuento>(entity =>
        {
            entity.HasKey(e => e.IdRegla).HasName("PK__regla_de__46D1C192C538BD77");

            entity.HasIndex(e => e.CodigoCupon, "IX_regla_descuento_codigo").HasFilter("([esta_activo]=(1))");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "IX_regla_descuento_fechas").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);

            entity.HasOne(d => d.Descuento).WithMany(p => p.ReglaDescuentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_regla_descuento_descuento_id_descuento");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__rol__6ABCB5E0F22E9825");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Severidad>(entity =>
        {
            entity.HasKey(e => e.IdSeveridad).HasName("PK__severida__9E59231E516CD8C1");
        });

        modelBuilder.Entity<SuscripcionNewsletter>(entity =>
        {
            entity.HasKey(e => e.IdSuscripcion).HasName("PK__suscripc__4E8926BB496BEAE0");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaAlta).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.SuscripcionNewsletters)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_suscripcion_newsletter_usuario");
        });

        modelBuilder.Entity<TipoMovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimientoInventario).HasName("PK__tipo_mov__D8AF2C3BE9F277A8");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<TipoToken>(entity =>
        {
            entity.HasKey(e => e.IdTipoToken).HasName("PK__tipo_tok__EBA9B06DE1CAD86D");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario__4E3E04AD5222227D");

            entity.HasIndex(e => e.EstaActivo, "IX_usuario_activo").HasFilter("([esta_activo]=(1))");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.AutenticacionProveedor).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_usuario_autenticacion_proveedor_id_autenticacion_proveedor");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_usuario_rol_id_rol");
        });

        modelBuilder.Entity<UsuarioFavorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PK__usuario___78F875AECB921073");

            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaAgregado).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Producto).WithMany(p => p.UsuarioFavoritos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorito_producto");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioFavoritos).HasConstraintName("FK_favorito_usuario");

            entity.HasOne(d => d.Variacion).WithMany(p => p.UsuarioFavoritos).HasConstraintName("FK_favorito_variacion");
        });

        modelBuilder.Entity<UsuarioListaItem>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PK__usuario___87C9438BA5399064");

            entity.Property(e => e.CantidadDeseada).HasDefaultValue(1);
            entity.Property(e => e.FechaAgregado).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Lista).WithMany(p => p.UsuarioListaItems).HasConstraintName("FK_lista_item_lista");

            entity.HasOne(d => d.Producto).WithMany(p => p.UsuarioListaItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_lista_item_producto");

            entity.HasOne(d => d.Variacion).WithMany(p => p.UsuarioListaItems).HasConstraintName("FK_lista_item_variacion");
        });

        modelBuilder.Entity<UsuarioListum>(entity =>
        {
            entity.HasKey(e => e.IdLista).HasName("PK__usuario___C100E2E5F5F93FB2");

            entity.Property(e => e.EsCompartible).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioLista).HasConstraintName("FK_lista_usuario");
        });

        modelBuilder.Entity<UsuarioMetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK__usuario___85BE0EBCA62A2580");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioMetodoPagos).HasConstraintName("FK_usuario_metodo_pago_usuario_id_usuario");
        });

        modelBuilder.Entity<UsuarioOauthProveedor>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioOauth).HasName("PK__usuario___CB87F614B4BDD7A2");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.AutenticacionProveedor).WithMany(p => p.UsuarioOauthProveedors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_usuario_oauth_proveedor_autenticacion_proveedor_id_autenticacion_proveedor");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioOauthProveedors).HasConstraintName("Fk_usuario_oauth_proveedor_usuario_id_usuario");
        });

        modelBuilder.Entity<UsuarioToken>(entity =>
        {
            entity.HasKey(e => e.IdToken).HasName("PK__usuario___3C2FA9C497F5EAE2");

            entity.HasIndex(e => e.TokenHash, "IX_usuario_token_hash").HasFilter("([revoked]=(0))");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioTokens).HasConstraintName("FK_usuario_token_usuario_id_usuario");
        });

        modelBuilder.Entity<ZonaEnvio>(entity =>
        {
            entity.HasKey(e => e.IdZona).HasName("PK__zona_env__67C93611ADCB03E2");

            entity.Property(e => e.DiasEntregaMax).HasDefaultValue(7);
            entity.Property(e => e.DiasEntregaMin).HasDefaultValue(3);
            entity.Property(e => e.EstaActivo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
