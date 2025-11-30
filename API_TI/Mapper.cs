using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Models.DTOs.ErrorDTOs;
using API_TI.Models.DTOs.MarcaDTOs;
using API_TI.Models.DTOs.OrdenDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Models.DTOs.UsuarioDTOs;
using API_TI.Models.DTOs.CheckoutDTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Models.DTOs.PagoDTOs;
using API_TI.Models.DTOs.ProveedorDTOs;
using API_TI.Models.DTOs.ReviewDTOs;
using API_TI.Models.DTOs.NotificacionDTOs;
using API_TI.Models.DTOs.CouponDTOs;
using CloudinaryDotNet.Actions;

namespace API_TI.Services.Helpers
{
    public static class Mapper
    {
        #region ErrorInfoDto
        public static ErrorInfoDto ToErroInforDto(ErrorCodigo errorInfo)
        {
            return new ErrorInfoDto
            {
                Codigo = errorInfo.Codigo,
                Nombre = errorInfo.Nombre,
                Mensaje = errorInfo.Mensaje,
                Severidad = errorInfo.SeveridadId,
                EstaActivo = errorInfo.Activo,
            };
        }

        public static List<ErrorInfoDto> ToErroInforDto(ICollection<ErrorCodigo> lista)
        {
            return lista.Select(x => ToErroInforDto(x)).ToList();
        }
        #endregion

        #region UsuarioDto
        public static UsuarioDto ToUsuarioDto(Usuario user)
        {
            return new UsuarioDto
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                NombreUsuario = user.NombreUsuario,
                ApellidoPaterno = user.ApellidoPaterno,
                ApellidoMaterno = user.ApellidoMaterno,
                //Telefono = user.Telefono,
                Correo = user.Correo,
                Rol = user.RolId,
                EstaActivo = user.EstaActivo,
                FechaCreacion = user.FechaCreacion
            };
        }
        #endregion

        #region ProductoDto
        public static ProductoDto ToProductoDto(Producto producto)
        {
            return new ProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = producto.NombreProducto,
                Descripcion = producto.Descripcion,
                Dimensiones = producto.Dimensiones,
                Peso = producto.Peso ?? 0,
                EsDestacado = producto.EsDestacado,
                EstaActivo = producto.EstaActivo,
                PrecioBase = producto.PrecioBase,
                PrecioPromocional = producto.PrecioPromocional,
                Sku = producto.Sku,
                CodigoBarras = producto.CodigoBarras,
                Stock = producto.StockTotal,
                Categoria = producto.Categoria?.NombreCategoria,
                Marca = producto.Marca?.NombreMarca,
                Proveedor = producto.Proveedor?.NombreProveedor,
                Imagenes = producto.ProductoImagens?.Select(i => new ProductoImagenDto
                {
                    IdImagen = i.IdImagen,
                    ProductoId = i.ProductoId,
                    UrlImagen = i.UrlImagen,
                    EsPrincipal = i.EsPrincipal,
                    Orden = i.Orden,
                }).ToList() ?? new List<ProductoImagenDto>(),
                Variaciones = new List<ProductoVariacionDto>()
            };

        }

        public static List<ProductoDto> ToProductoDto(ICollection<Producto> lista)
        {
            return lista.Select(x => ToProductoDto(x)).ToList();
        }
        #endregion

        #region ProductoVariacionDto
        public static ProductoVariacionDto ToProductoVariacionDto(ProductoVariacion productoVeariacion)
        {
            return new ProductoVariacionDto
            {
                IdVariacion = productoVeariacion.IdVariacion,
                ProductoId = productoVeariacion.ProductoId,
                Sku = productoVeariacion.Sku,
                CodigoBarras = productoVeariacion.CodigoBarras,
                Precio = productoVeariacion.Precio,
                ImagenUrl = productoVeariacion.ImagenUrl,
                Stock = productoVeariacion.Stock,
                Categoria = productoVeariacion.Producto.Categoria?.NombreCategoria,
                Marca = productoVeariacion.Producto.Marca?.NombreMarca,
                Proveedor = productoVeariacion.Producto.Proveedor?.NombreProveedor,
                EstaActivo = productoVeariacion.EstaActivo
            };

        }
        #endregion

        #region ProductoDescuentoDto
        public static ProductoDescuentoDto ToProductoDescuentoDto(ProductoDescuento productoDescuento)
        {
            return new ProductoDescuentoDto
            {
                ProductoId = productoDescuento.ProductoId,
                DescuentoId = productoDescuento.DescuentoId
            };
        }


        public static List<ProductoDescuentoDto> ToProductoDescuentoDto(ICollection<ProductoDescuento> lista)
        {
            return lista.Select(x => ToProductoDescuentoDto(x)).ToList();
        }
        #endregion

        //#region ReviewDto 
        //public static Checkout ToCheckoutDto(CarritoDto carrito, MetodoPagoDto metodoPago, DireccionDto direccionEnvio, decimal costoEnvio)
        //{
        //    return new Checkout
        //    {
        //        Carrito = carrito,
        //        MetodoPago = metodoPago,
        //        DireccionEnvio = direccionEnvio,
        //        CostoEnvio = costoEnvio
        //    };
        //}

        //#endregion

        #region ReviewDto 
        public static ReviewDto ToReviewDto(ProductoResena review)
        {
            return new ReviewDto
            {
                IdReview = review.IdResena,
                ProductoId = review.ProductoId,
                UsuarioId = review.UsuarioId,
                NombreUsuario = review.Usuario?.NombreUsuario ?? "Usuario",
                Calificacion = review.Calificacion,
                Comentario = review.Comentario,
                FechaCreacion = review.FechaCreacion,
                //Verificado = review.Verificado
            };
        }

        public static List<ReviewDto> ToReviewDto(ICollection<ProductoResena> lista)
        {
            return lista.Select(x => ToReviewDto(x)).ToList();
        }
        #endregion

        #region NotificacionDto 
        public static NotificacionDto ToNotificacionDto(NotificacionUsuario notificacion)
        {
            return new NotificacionDto
            {
                IdNotificacion = notificacion.IdNotificacion,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Tipo = notificacion.Tipo,
                Leido = notificacion.Leido,
                FechaCreacion = notificacion.FechaCreacion
            };
        }

        public static List<NotificacionDto> ToNotificacionDto(ICollection<NotificacionUsuario> lista)
        {
            return lista.Select(x => ToNotificacionDto(x)).ToList();
        }
        #endregion

        #region OrdenDto
        public static OrdenDto ToOrdenDto(Orden orden)
        {
            return new OrdenDto
            {
                IdOrden = orden.IdOrden,
                NumeroOrden = orden.NumeroOrden,
                FechaOrden = orden.FechaOrden,
                Estado = orden.EstatusVenta.NombreEstatusVenta,
                NombreCliente = $"{orden.Usuario.NombreUsuario} {orden.Usuario.ApellidoPaterno ?? ""}".Trim(),
                EmailCliente = orden.Usuario.Correo,
                TelefonoCliente = orden.DireccionEnvio.Telefono,
                DireccionEnvio = orden.DireccionEnvio != null ? new DireccionDto
                {
                    Calle = orden.DireccionEnvio.Calle,
                    NumeroExterior = orden.DireccionEnvio.NumeroExterior,
                    NumeroInterior = orden.DireccionEnvio.NumeroInterior,
                    CodigoPostal = orden.DireccionEnvio.CodigoPostal,
                    Colonia = orden.DireccionEnvio.Colonia,
                    CiudadNombre = orden.DireccionEnvio.Ciudad.NombreCiudad,
                    EstadoNombre = orden.DireccionEnvio.Estado.NombreEstado,
                    DireccionCompleta = $"{orden.DireccionEnvio.Calle} {orden.DireccionEnvio.NumeroExterior}, " +
                       $"{orden.DireccionEnvio.Colonia}, {orden.DireccionEnvio.CodigoPostal}, " +
                       $"{orden.DireccionEnvio.Ciudad}, {orden.DireccionEnvio.Estado}"
                } : null,
                Items = orden.OrdenItems?.Select(oi => new OrdenItemDto
                {
                    IdOrdenItem = oi.IdOrdenItem,
                    ProductoId = oi.ProductoId,
                    NombreProducto = oi.Producto?.NombreProducto ?? "Producto",
                    ImagenUrl = oi.Producto?.ProductoImagens?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen,
                    Cantidad = oi.Cantidad,
                    PrecioUnitario = oi.PrecioUnitario,
                    DescuentoAplicado = oi.DescuentoAplicado,
                    Subtotal = (oi.PrecioUnitario - oi.DescuentoAplicado) * oi.Cantidad,
                    Producto = oi.Producto != null ? new ProductoSimpleDto
                    {
                        Imagenes = oi.Producto.ProductoImagens
                            ?.OrderBy(i => i.Orden)
                            ?.Select(i => i.UrlImagen)
                            ?.ToList()
                    } : null
                }).ToList() ?? new List<OrdenItemDto>(),
                Descuentos = orden.OrdenDescuentos?.Select(od => new OrdenDescuentoDto
                {
                    NombreDescuento = od.Descuento?.NombreDescuento ?? "Descuento",
                    MontoDescuento = od.MontoAplicado
                }).ToList() ?? new List<OrdenDescuentoDto>(),
                Subtotal = orden.Subtotal,
                Descuento = orden.DescuentoTotal,
                Impuestos = orden.ImpuestoTotal,
                CostoEnvio = orden.CostoEnvio,
                Total = orden.Total
            };
        }

        public static List<OrdenDto> ToDesToOrdenDtocuentoDto(List<Orden> lista)
        {
            return lista.Select(x => ToOrdenDto(x)).ToList();
        }
        #endregion

        #region DescuentoDto
        public static DescuentoDto ToDescuentoDto(Descuento descuento) 
        {
            var regla = descuento.ReglaDescuentos?.FirstOrDefault();
            return new DescuentoDto
            {
                IdDescuento = descuento.IdDescuento,
                NombreDescuento = descuento.NombreDescuento,
                Descripcion = descuento.Descripcion,
                TipoDescuento = descuento.TipoDescuento,
                CodigoCupon = regla?.CodigoCupon,
                Valor = regla?.Valor ?? 0,
                FechaInicio = regla?.FechaInicio,
                FechaFin = regla?.FechaFin,
                LimiteUsosTotal = regla?.LimiteUsosTotal,
                UsosActuales = regla?.UsosActuales ?? 0,
                EstaActivo = descuento.EstaActivo
            };
        }

        public static List<DescuentoDto> ToDescuentoDto(List<Descuento> lista)
        {
            return lista.Select(x => ToDescuentoDto(x)).ToList();
        }
        #endregion
        
        #region DescuentodetailsDto
        public static DescuentoDetailDto ToDescuentodetailsDto(Descuento descuento) 
        {
            var dto = ToDescuentoDto(descuento);
            var regla = descuento.ReglaDescuentos?.FirstOrDefault();

            return new DescuentoDetailDto
            {
                IdDescuento = dto.IdDescuento,
                NombreDescuento = dto.NombreDescuento,
                Descripcion = dto.Descripcion,
                TipoDescuento = dto.TipoDescuento,
                CodigoCupon = dto.CodigoCupon,
                Valor = dto.Valor,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                LimiteUsosTotal = dto.LimiteUsosTotal,
                LimiteUsosPorUsuario = regla?.LimiteUsosPorUsuario,
                UsosActuales = dto.UsosActuales,
                MontoMinimo = regla?.MontoMinimoCompra,
                SoloNuevosUsuarios = regla?.SoloNuevosUsuarios ?? false,
                EstaActivo = dto.EstaActivo,
                Alcances = descuento.DescuentoAlcances?.Select(a => new AlcanceDto
                {
                    TipoEntidad = a.TipoEntidad,
                    EntidadId = a.EntidadId,
                    Incluir = a.Incluir
                }).ToList()
            };
        }

       
        #endregion

        #region ReglaDescuentoDto
        public static ReglaDescuentoDto ToReglaDescuentoDto(ReglaDescuento reglaDescuento)
        {
            return new ReglaDescuentoDto
            {
                //IdReglaDescuento = reglaDescuento.IdReglaDescuento,
                //Prioridad = reglaDescuento.Prioridad,
                //UsuariosNuevosOnly = reglaDescuento.UsuariosNuevosOnly,
                //CantidadMaxPorUsuario = reglaDescuento.CantidadMaxPorUsuario,
                //CantidadMaxUso = reglaDescuento.CantidadMaxUso,
                //CantidadMinima = reglaDescuento.CantidadMinima,
                //TotalMinimo = reglaDescuento.TotalMinimo,
                //UsoActual = reglaDescuento.UsoActual,
                //Coupon = reglaDescuento.Coupon,
                FechaInicio = reglaDescuento.FechaInicio,
                FechaFin = reglaDescuento.FechaFin
            };

        }


        public static List<ReglaDescuentoDto> ToReglaDescuentoDto(ICollection<ReglaDescuento> lista)
        {
            return lista.Select(x => ToReglaDescuentoDto(x)).ToList();
        }
        #endregion

        #region DescuentoObjetivoDto
        public static DescuentoObjetivoDto ToDescuentoObjetivoDto(DescuentoObjetivo descuentoObjetivo)
        {
            return new DescuentoObjetivoDto
            {
                IdDescuentoObjetivo = descuentoObjetivo.IdDescuentoObjetivo,
                DescuentoId = descuentoObjetivo.DescuentoId,
                TipoObjetivo = descuentoObjetivo.TipoObjetivo,
                ObjetivoId = descuentoObjetivo.ObjetivoId
            };
        }


        public static List<DescuentoObjetivoDto> ToDescuentoObjetivoDto(ICollection<DescuentoObjetivo> lista)
        {
            return lista.Select(x => ToDescuentoObjetivoDto(x)).ToList();
        }
        #endregion

        #region CuponUsadoDto
        public static CuponUsadoDto ToCuponUsadoDto(ReglaDescuento reglaDescuento, int usuarioId, decimal descuentoAplicado, decimal totalFinal)
        {
            return new CuponUsadoDto
            {
                UsuarioId = usuarioId,
                Aplicado = true,
                DescuentoAplicado = descuentoAplicado,
                TotalFinal = totalFinal,
                //CodigoCupon = reglaDescuento.Coupon ?? string.Empty,
                NombreDescuento = reglaDescuento.Descuento.NombreDescuento,
                //TipoValor = reglaDescuento.Descuento.TipoValor,
                Valor = reglaDescuento.Descuento.Valor,
                //ReglaDescuentoId = reglaDescuento.IdReglaDescuento,
                DescuentoId = reglaDescuento.DescuentoId
            };
        }

        public static List<CuponUsadoDto> ToCuponUsadoDto(
               ICollection<(ReglaDescuento regla, int usuarioId, decimal descuentoAplicado, decimal totalFinal)> lista)
        {
            return lista.Select(x =>
                ToCuponUsadoDto(x.regla, x.usuarioId, x.descuentoAplicado, x.totalFinal)
            ).ToList();
        }

        #endregion

        #region CarritoDto
        public static CarritoDto ToCarritoDto(Carrito carrito)
        {
            return new CarritoDto
            {
                IdCarrito = carrito.IdCarrito,
                UsuarioId = carrito.UsuarioId,
                Items = carrito.CarritoItems?
                    .Select(i => new CarritoItemDto
                    {
                        IdCarritoItem = i.IdCarritoItem,
                        ProductoId = i.ProductoId,
                        NombreProducto = i.Producto?.NombreProducto,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        DescuentoAplicado = i.DescuentoAplicado,
                   
                    }).ToList() ?? new List<CarritoItemDto>(),
                DescuentosAplicados = carrito.CarritoDescuentos?
                    .Select(d => new CarritoDescuentoDto
                    {
                        IdDescuento = d.DescuentoId,
                        NombreDescuento = d.Descuento?.NombreDescuento,
                        TipoDescuento = d.Descuento?.TipoDescuento,
                        MontoDescuento = d.MontoAplicado, // FIXED
                        CodigoCupon = d.ReglaDescuento?.CodigoCupon,
                    }).ToList() ?? new List<CarritoDescuentoDto>(),
                Subtotal = carrito.Subtotal,
                DescuentoTotal = carrito.DescuentoTotal,
                ImpuestoTotal = 0,
                EnvioTotal = 0,
                Total = carrito.Subtotal - carrito.DescuentoTotal,
                TotalItems = carrito.CarritoItems?.Sum(i => i.Cantidad) ?? 0
            };
        }


        public static List<CarritoDto> ToCarritoDto(ICollection<Carrito> lista)
        {
            return lista.Select(x => ToCarritoDto(x)).ToList();
        }
        #endregion

        #region CheckoutDto
        public static Checkout ToCheckoutDto(CarritoDto carrito, MetodoPagoDto metodoPago, DireccionDto direccionEnvio, decimal costoEnvio)
        {
            return new Checkout
            {
                Carrito = carrito,
                MetodoPago = metodoPago,
                DireccionEnvio = direccionEnvio,
                CostoEnvio = costoEnvio
            };
        }
           
        #endregion

        #region MarcaDto
        public static MarcaDto ToMarcaDto(Marca marcas)
        {
            return new MarcaDto
            {
                IdMarca = marcas.IdMarca,
                Nombre = marcas.NombreMarca,
                Descripcion = marcas.Descripcion,
                EstaActivo = marcas.EstaActivo
            };

        }

        public static List<MarcaDto> ToMarcaDto(List<Marca> lista)
        {
            return lista.Select(x => ToMarcaDto(x)).ToList();
        }
        #endregion

        #region CategoriaDto
        public static CategoriaDto ToCategoriaDto(Categorium categoria)
        {
            return new CategoriaDto
            {
                IdCategoria = categoria.IdCategoria,
                Nombre = categoria.NombreCategoria,
                Descripcion = categoria.Descripcion,
                EstaActivo = categoria.EstaActivo
            };

        }

        public static List<CategoriaDto> ToCategoriaDto(ICollection<Categorium> lista)
        {
            return lista.Select(x => ToCategoriaDto(x)).ToList();
        }
        #endregion

        #region DireccionDto
        public static DireccionDto ToDireccionDto(Direccion direccion)
        {
            return new DireccionDto
            {
                IdDireccion = direccion.IdDireccion,
                UsuarioId = direccion.UsuarioId,
                PaisId = direccion.PaisId,
                EstadoId = direccion.EstadoId,
                CiudadId = direccion.CiudadId,
                CodigoPostal = direccion.CodigoPostal,
                Colonia = direccion.Colonia,
                Calle = direccion.Calle,
                NumeroInterior = direccion.NumeroInterior,
                NumeroExterior = direccion.NumeroExterior,
                Etiqueta = direccion.Etiqueta,
                EsDefault = direccion.EsDefault
            };
        }

        public static List<DireccionDto> ToDireccionDto(List<Direccion> lista)
        {
            return lista.Select(x => ToDireccionDto(x)).ToList();
        }
        #endregion

        #region DireccionDto
        public static ProveedorDto ToProveedorDto(Proveedor proveedor)
        {
            return new ProveedorDto
            {
                IdProveedor = proveedor.IdProveedor,
                NombreProveedor = proveedor.NombreProveedor,
                NombreContacto = proveedor.NombreContacto,
                Telefono = proveedor.Telefono,
                Correo = proveedor.Correo,
                Direccion = proveedor.Direccion,
                EstaActivo = proveedor.EstaActivo
            };
        }

        public static List<ProveedorDto> ToProveedorDto(List<Proveedor> lista)
        {
            return lista.Select(x => ToProveedorDto(x)).ToList();
        }
        #endregion
    }
}
