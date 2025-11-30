using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class RecommendationService : BaseService, IRecommendationService
    {
        private readonly TiPcComponentsContext _context;

        public RecommendationService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetRecommendationsAsync(int usuarioId, int take = 10)
        {
            try
            {
                var userOrders = await _context.OrdenItems
                    .Where(oi => oi.Orden.UsuarioId == usuarioId)
                    .Select(oi => oi.ProductoId)
                    .Distinct()
                    .ToListAsync();

                if (userOrders.Any())
                {
                    // Users who bought same products also bought
                    var recommendations = await _context.OrdenItems
                        .Where(oi => userOrders.Contains(oi.ProductoId) &&
                                    oi.Orden.UsuarioId != usuarioId)
                        .GroupBy(oi => oi.ProductoId)
                        .Select(g => new { ProductoId = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(take)
                        .Join(_context.Productos.Include(p => p.ProductoImagens).Include(p => p.Marca),
                              x => x.ProductoId,
                              p => p.IdProducto,
                              (x, p) => p)
                        .Where(p => p.EstaActivo && !userOrders.Contains(p.IdProducto))
                        .ToListAsync();

                    if (recommendations.Any())
                        return ApiResponse<List<ProductoDto>>.Ok(Mapper.ToProductoDto(recommendations).ToList());
                }

                // Fallback: trending products
                return await GetTrendingProductsAsync(take);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<ProductoDto>>(9000);
            }
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetSimilarProductsAsync(int productoId, int take = 6)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null)
                    return await ReturnErrorAsync<List<ProductoDto>>(300);

                var similar = await _context.Productos
                    .Include(p => p.ProductoImagens)
                    .Include(p => p.Marca)
                    .Where(p => p.EstaActivo &&
                               p.IdProducto != productoId &&
                               (p.CategoriaId == producto.CategoriaId || p.MarcaId == producto.MarcaId))
                    .OrderBy(p => Math.Abs(p.PrecioBase - producto.PrecioBase))
                    .Take(take)
                    .ToListAsync();

                return ApiResponse<List<ProductoDto>>.Ok(Mapper.ToProductoDto(similar).ToList());
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<ProductoDto>>(9000);
            }
        }

        private async Task<ApiResponse<List<ProductoDto>>> GetTrendingProductsAsync(int take)
        {
            var last30Days = DateTime.UtcNow.AddDays(-30);
            var trending = await _context.OrdenItems
                .Where(oi => oi.Orden.FechaOrden >= last30Days && oi.Orden.EstatusVentaId >= 4)
                .GroupBy(oi => oi.ProductoId)
                .Select(g => new { ProductoId = g.Key, Count = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .Join(_context.Productos.Include(p => p.ProductoImagens).Include(p => p.Marca),
                      x => x.ProductoId,
                      p => p.IdProducto,
                      (x, p) => p)
                .ToListAsync();

            return ApiResponse<List<ProductoDto>>.Ok(Mapper.ToProductoDto(trending).ToList());
        }
    }
}
