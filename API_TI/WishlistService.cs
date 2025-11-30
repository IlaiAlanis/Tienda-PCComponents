using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.WishlistDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class WishlistService : BaseService, IWishlistService
    {
        private readonly TiPcComponentsContext _context;

        public WishlistService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<WishlistDto>> GetWishlistAsync(int usuarioId)
        {
            try
            {
                var wishlist = await GetOrCreateWishlistAsync(usuarioId);
                return ApiResponse<WishlistDto>.Ok(await MapToDto(wishlist));
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<WishlistDto>(9000);
            }
        }

        public async Task<ApiResponse<WishlistDto>> AddItemAsync(int usuarioId, int productoId)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null)
                    return await ReturnErrorAsync<WishlistDto>(300);

                var wishlist = await GetOrCreateWishlistAsync(usuarioId);

                if (await _context.UsuarioListaItems.AnyAsync(i =>
                    i.ListaId == wishlist.IdLista && i.ProductoId == productoId))
                    return await ReturnErrorAsync<WishlistDto>(6, "Ya está en favoritos");

                _context.UsuarioListaItems.Add(new UsuarioListaItem
                {
                    ListaId = wishlist.IdLista,
                    ProductoId = productoId,
                    FechaAgregado = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await AuditAsync("Wishlist.ItemAdded", new { ProductoId = productoId }, usuarioId);

                return await GetWishlistAsync(usuarioId);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<WishlistDto>(9000);
            }
        }

        public async Task<ApiResponse<WishlistDto>> RemoveItemAsync(int usuarioId, int productoId)
        {
            try
            {
                var wishlist = await _context.UsuarioLista
                    .FirstOrDefaultAsync(w => w.UsuarioId == usuarioId);

                if (wishlist == null)
                    return await ReturnErrorAsync<WishlistDto>(5);

                var item = await _context.UsuarioListaItems
                    .FirstOrDefaultAsync(i => i.ListaId == wishlist.IdLista &&
                                             i.ProductoId == productoId);

                if (item == null)
                    return await ReturnErrorAsync<WishlistDto>(5);

                _context.UsuarioListaItems.Remove(item);
                await _context.SaveChangesAsync();

                await AuditAsync("Wishlist.ItemRemoved", new { ProductoId = productoId }, usuarioId);

                return await GetWishlistAsync(usuarioId);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<WishlistDto>(9000);
            }
        }

        private async Task<UsuarioListum> GetOrCreateWishlistAsync(int usuarioId)
        {
            var wishlist = await _context.UsuarioLista
                .Include(w => w.UsuarioListaItems)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.ProductoImagens)
                .FirstOrDefaultAsync(w => w.UsuarioId == usuarioId);

            if (wishlist == null)
            {
                wishlist = new UsuarioListum
                {
                    UsuarioId = usuarioId,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.UsuarioLista.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return wishlist;
        }

        private async Task<WishlistDto> MapToDto(UsuarioListum wishlist)
        {
            return new WishlistDto
            {
                IdListaDeseo = wishlist.IdLista,
                UsuarioId = wishlist.UsuarioId,
                TotalItems = wishlist.UsuarioListaItems?.Count ?? 0,
                Items = wishlist.UsuarioListaItems?.Select(i => new WishlistItemDto
                {
                    IdItem = i.IdItem,
                    ProductoId = i.ProductoId,
                    NombreProducto = i.Producto.NombreProducto,
                    ImagenUrl = i.Producto.ProductoImagens.FirstOrDefault(img => img.EsPrincipal)?.UrlImagen,
                    Precio = i.Producto.PrecioPromocional ?? i.Producto.PrecioBase,
                    EnStock = i.Producto.StockTotal > 0,
                    FechaAgregado = i.FechaAgregado
                }).ToList() ?? new List<WishlistItemDto>()
            };
        }
    }
}
