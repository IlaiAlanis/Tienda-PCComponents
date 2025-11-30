using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CouponDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_TI.Services.Implementations
{
    public class DescuentoService : BaseService, IDescuentoService
    {
        private readonly TiPcComponentsContext _context;

        public DescuentoService(
            TiPcComponentsContext context, 
            IErrorService errorService, 
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<List<Descuento>> GetApplicableDiscountsAsync(int carritoId, string codigoCupon = null)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritoItems)
                    .ThenInclude(ci => ci.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(c => c.CarritoItems)
                    .ThenInclude(ci => ci.Producto)
                        .ThenInclude(p => p.Marca)
                .FirstOrDefaultAsync(c => c.IdCarrito == carritoId);

            if (carrito == null) return new List<Descuento>();

            var now = DateTime.UtcNow;
            var query = _context.Descuentos
                .Include(d => d.ReglaDescuentos)
                .Include(d => d.DescuentoAlcances)
                .Where(d => d.EstaActivo)
                .Where(d => d.ReglaDescuentos.Any(r =>
                    r.EstaActivo &&
                    r.FechaInicio <= now &&
                    r.FechaFin >= now));

            // Filter by coupon if provided
            if (!string.IsNullOrWhiteSpace(codigoCupon))
            {
                query = query.Where(d => d.ReglaDescuentos.Any(r => r.CodigoCupon == codigoCupon));
            }
            else
            {
                // Auto-apply discounts (no coupon required)
                query = query.Where(d => d.ReglaDescuentos.Any(r => r.CodigoCupon == null));
            }

            var descuentos = await query.ToListAsync();

            // Filter by scope (products in cart)
            var applicableDescuentos = new List<Descuento>();
            foreach (var descuento in descuentos)
            {
                if (await IsDiscountApplicableToCart(descuento, carrito))
                    applicableDescuentos.Add(descuento);
            }

            return applicableDescuentos.OrderBy(d => d.Prioridad).ToList();
        }

        private async Task<bool> IsDiscountApplicableToCart(Descuento descuento, Carrito carrito)
        {
            var alcances = descuento.DescuentoAlcances.ToList();

            // Global discount
            if (alcances.Any(a => a.TipoEntidad == "GLOBAL"))
                return true;

            // Check if any cart item matches scope
            foreach (var item in carrito.CarritoItems)
            {
                if (alcances.Any(a =>
                    (a.TipoEntidad == "PRODUCTO" && a.EntidadId == item.ProductoId && a.Incluir) ||
                    (a.TipoEntidad == "CATEGORIA" && a.EntidadId == item.Producto.CategoriaId && a.Incluir) ||
                    (a.TipoEntidad == "MARCA" && a.EntidadId == item.Producto.MarcaId && a.Incluir)))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<ApiResponse<object>> ValidateCouponAsync(string codigoCupon, int usuarioId)
        {
            try
            {
                var regla = await _context.ReglaDescuentos
                    .Include(r => r.Descuento)
                    .FirstOrDefaultAsync(r => r.CodigoCupon == codigoCupon && r.EstaActivo);

                if (regla == null)
                    return await ReturnErrorAsync<object>(851, "Cupón inválido");

                var now = DateTime.UtcNow;
                if (regla.FechaInicio > now || regla.FechaFin < now)
                    return await ReturnErrorAsync<object>(852, "Cupón expirado");

                // Check total uses
                if (regla.LimiteUsosTotal.HasValue && regla.UsosActuales >= regla.LimiteUsosTotal)
                    return await ReturnErrorAsync<object>(855, "Cupón agotado");

                // Check user uses
                if (regla.LimiteUsosPorUsuario.HasValue)
                {
                    var userUses = await _context.DescuentoUsos
                        .CountAsync(u => u.DescuentoId == regla.DescuentoId && u.UsuarioId == usuarioId);

                    if (userUses >= regla.LimiteUsosPorUsuario)
                        return await ReturnErrorAsync<object>(862, "Ya usaste este cupón");
                }

                // Check new user requirement
                if (regla.SoloNuevosUsuarios)
                {
                    var hasOrders = await _context.Ordens.AnyAsync(o => o.UsuarioId == usuarioId);
                    if (hasOrders)
                        return await ReturnErrorAsync<object>(866, "Cupón solo para nuevos usuarios");
                }

                return ApiResponse<object>.Ok(new { Valid = true, Descuento = regla.Descuento.NombreDescuento });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }
    }
}

