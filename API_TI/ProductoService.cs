using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.InventarioDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Azure.Core;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ProductoService : BaseService, IProductoService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(
            TiPcComponentsContext context,
            ILogger<ProductoService> logger,
            IErrorService errorService, 
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Include(x => x.Categoria)
                    .Include(x => x.Marca)
                    .Include(x => x.Proveedor)
                    .Include(x => x.ProductoImagens)
                    .Where(x => x.EstaActivo)
                    .ToListAsync();

                var dtoList = Mapper.ToProductoDto(productos);
                return ApiResponse<IEnumerable<ProductoDto>>.Ok(dtoList);
            }
            catch ( Exception ex ) 
            {
                _logger.LogError(ex, "Error getting products");
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IEnumerable<ProductoDto>>(9000);
            }
            
        }

        public async Task<ApiResponse<ProductoDto>> GetByIdAsync(int id)
        {
            try 
            { 
                var producto = await _context.Productos
                    .AsNoTracking()
                    .Include(x => x.Categoria)
                    .Include(x => x.Marca)
                    .Include(x => x.Proveedor)
                    .Include(x => x.ProductoImagens)
                    .Include(x => x.ProductoVariacions)
                    .FirstOrDefaultAsync(x => x.IdProducto == id && x.EstaActivo);

                if (producto == null)
                    return await ReturnErrorAsync<ProductoDto>(300);

                var dto = Mapper.ToProductoDto(producto);
                return ApiResponse<ProductoDto>.Ok(dto);
            }
            catch ( Exception ex ) 
            {
                await LogTechnicalErrorAsync(900, ex);
                return await ReturnErrorAsync<ProductoDto>(9000);
            }
        }

        public async Task<ApiResponse<ProductoDto>> CreateAsync(CreateProductoDto dto, int usuarioId)
        {
            try
            {
                var exists = await _context.Productos
                    .Where(x => x.EstaActivo && ( x.Sku == dto.Sku || x.CodigoBarras == dto.CodigoBarras))
                    .Select(x => new { x.Sku, x.CodigoBarras })
                    .FirstOrDefaultAsync();

                if (exists != null)
                {
                    if (exists.Sku == dto.Sku)
                        return await ReturnErrorAsync<ProductoDto>(300, $"SKU {dto.Sku} ya existe");
                    if (exists.CodigoBarras == dto.CodigoBarras)
                        return await ReturnErrorAsync<ProductoDto>(300, $"Código de barras {dto.CodigoBarras} ya existe");
                }

                if (!await _context.Categoria.AnyAsync(c => c.IdCategoria == dto.CategoriaId))
                    return await ReturnErrorAsync<ProductoDto>(400);

                if (!await _context.Marcas.AnyAsync(m => m.IdMarca == dto.MarcaId))
                    return await ReturnErrorAsync<ProductoDto>(450);

                if (!await _context.Proveedors.AnyAsync(p => p.IdProveedor == dto.ProveedorId))
                    return await ReturnErrorAsync<ProductoDto>(500);

                var producto = new Producto
                {
                    NombreProducto = dto.Nombre,
                    CategoriaId = dto.CategoriaId,
                    MarcaId = dto.MarcaId,
                    ProveedorId = dto.ProveedorId,
                    Descripcion = dto.Descripcion,
                    Dimensiones = dto.Dimensiones,
                    Peso = dto.Peso,
                    PrecioBase = dto.PrecioBase,
                    PrecioPromocional = dto.PrecioPromocional,
                    Sku = dto.Sku,
                    CodigoBarras = dto.CodigoBarras,
                    StockTotal = dto.Stock,
                    EstaActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await using var txn = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Productos.Add(producto);
                    await _context.SaveChangesAsync();
                    await txn.CommitAsync();

                    await AuditAsync("Product.Created", new { ProductoId = producto.IdProducto, Sku = producto.Sku });
                }
                catch (Exception ex)
                {
                    await txn.RollbackAsync();
                    throw;
                }

                return await GetByIdAsync(producto.IdProducto);
            }
            catch (Exception ex) 
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoDto>(9000);
            }          
        }

        public async Task<ApiResponse<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto, int usuarioId)
        {
            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(x => x.IdProducto == id && x.EstaActivo);

                if (producto == null)
                    return await ReturnErrorAsync<ProductoDto>(300);

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                    producto.NombreProducto = dto.Nombre;

                // Track price changes
                if (dto.PrecioBase.HasValue && dto.PrecioBase.Value != producto.PrecioBase)
                {
                    _context.ProductoHistorialPrecios.Add(new ProductoHistorialPrecio
                    {
                        UsuarioId = usuarioId,
                        ProductoId = producto.IdProducto,
                        PrecioAnterior = producto.PrecioBase,
                        PrecioNuevo = dto.PrecioBase.Value,
                        Motivo = "Actualización manual",
                        FuenteCambio = "API",
                        FechaCambio = DateTime.UtcNow
                    });
                    producto.PrecioBase = dto.PrecioBase.Value;
                }

                if (dto.PrecioPromocional.HasValue) producto.PrecioPromocional = dto.PrecioPromocional;
                if (dto.Stock.HasValue) producto.StockTotal = dto.Stock.Value;
                if (dto.EsDestacado.HasValue) producto.EsDestacado = dto.EsDestacado.Value;
                if (dto.Peso.HasValue) producto.Peso = dto.Peso;
                if (!string.IsNullOrWhiteSpace(dto.Dimensiones)) producto.Dimensiones = dto.Dimensiones;

                producto.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await AuditAsync("Product.Updated", new { ProductoId = id });

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoDto>(9000);
            }
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id, int usuarioId)
        {
            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(x => x.IdProducto == id && x.EstaActivo);

                if (producto == null)
                    return await ReturnErrorAsync<bool>(2000);

                producto.EstaActivo = false;
                producto.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await AuditAsync("Product.Deleted", new { ProductoId = id });

                return ApiResponse<bool>.Ok(true, "Producto eliminado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<bool>(9000);
            }
        }

        public async Task<ApiResponse<ProductoVariacionDto>> GetByIdVariationAsync(int idVariacion)
        {
            try
            {
                var variacion = await _context.ProductoVariacions
                    .AsNoTracking()
                    .Include(v => v.Producto)
                        .ThenInclude(p => p.Marca)
                    .Include(v => v.Producto)
                        .ThenInclude(p => p.Proveedor)
                    .FirstOrDefaultAsync(v => v.IdVariacion == idVariacion && v.EstaActivo);

                if (variacion == null)
                    return await ReturnErrorAsync<ProductoVariacionDto>(2003);

                var dto = Mapper.ToProductoVariacionDto(variacion);
                return ApiResponse<ProductoVariacionDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoVariacionDto>(9000);
            }
        }

        public async Task<ApiResponse<ProductoVariacionDto>> CreateVariationAsync(ProductoVariacionCreateDto dto, int usuarioId)
        {
            try
            {
                if (!await _context.Productos.AnyAsync(p => p.IdProducto == dto.ProductoId && p.EstaActivo))
                    return await ReturnErrorAsync<ProductoVariacionDto>(300);

                if (await _context.ProductoVariacions.AnyAsync(v => v.Sku == dto.Sku && v.EstaActivo))
                    return await ReturnErrorAsync<ProductoVariacionDto>(300, $"SKU {dto.Sku} ya existe");

                if (await _context.ProductoVariacions.AnyAsync(v => v.CodigoBarras == dto.CodigoBarras && v.EstaActivo))
                    return await ReturnErrorAsync<ProductoVariacionDto>(300, $"Código de barras {dto.CodigoBarras} ya existe");

                var variacion = new ProductoVariacion
                {
                    ProductoId = dto.ProductoId,
                    Sku = dto.Sku,
                    CodigoBarras = dto.CodigoBarras,
                    Precio = dto.Precio,
                    ImagenUrl = dto.ImagenUrl,
                    Stock = dto.Stock,
                    EstaActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await using var txn = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.ProductoVariacions.Add(variacion);
                    await _context.SaveChangesAsync();
                    await RecalculateStockAsync(dto.ProductoId);
                    await txn.CommitAsync();

                    await AuditAsync("ProductVariation.Created", new { VariacionId = variacion.IdVariacion });
                }
                catch
                {
                    await txn.RollbackAsync();
                    throw;
                }

                return await GetByIdVariationAsync(variacion.IdVariacion);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoVariacionDto>(9000);
            }
        }

        public async Task<ApiResponse<ProductoVariacionDto>> UpdateVariationAsync(int idVariacion, ProductoVariacionCreateDto dto, int usuarioId)
        {
            try
            {
                var variacion = await _context.ProductoVariacions
                    .FirstOrDefaultAsync(v => v.IdVariacion == idVariacion && v.EstaActivo);

                if (variacion == null)
                    return await ReturnErrorAsync<ProductoVariacionDto>(300);

                // Track price changes
                if (dto.Precio != variacion.Precio)
                {
                    _context.ProductoVariacionHistorialPrecios.Add(new ProductoVariacionHistorialPrecio
                    {
                        UsuarioId = usuarioId,
                        ProductoId = variacion.ProductoId,
                        PrecioAnterior = variacion.Precio,
                        PrecioNuevo = dto.Precio,
                        Motivo = "Actualización manual",
                        FuenteCambio = "API",
                        FechaCambio = DateTime.UtcNow
                    });
                    variacion.Precio = dto.Precio;
                }

                if (!string.IsNullOrWhiteSpace(dto.Sku)) variacion.Sku = dto.Sku;
                if (!string.IsNullOrWhiteSpace(dto.CodigoBarras)) variacion.CodigoBarras = dto.CodigoBarras;
                if (!string.IsNullOrWhiteSpace(dto.ImagenUrl)) variacion.ImagenUrl = dto.ImagenUrl;
                if (dto.Stock != variacion.Stock) variacion.Stock = dto.Stock;

                variacion.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await RecalculateStockAsync(variacion.ProductoId);
                await AuditAsync("ProductVariation.Updated", new { VariacionId = idVariacion });

                return await GetByIdVariationAsync(idVariacion);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoVariacionDto>(9000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteVariationAsync(int idVariacion, int usuario)
        {
            try
            {
                var variacion = await _context.ProductoVariacions
                    .FirstOrDefaultAsync(v => v.IdVariacion == idVariacion && v.EstaActivo);

                if (variacion == null)
                    return await ReturnErrorAsync<bool>(300);

                variacion.EstaActivo = false;
                variacion.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await RecalculateStockAsync(variacion.ProductoId);
                await AuditAsync("ProductVariation.Deleted", new { VariacionId = idVariacion });

                return ApiResponse<bool>.Ok(true, "Variación eliminada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<bool>(9000);
            }
        }

        public async Task<ApiResponse<PagedResult<ProductoDto>>> SearchAsync(ProductoSearchRequest request)
        {
            try
            {
                var query = _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Categoria)
                    .Include(p => p.Marca)
                    .Include(p => p.Proveedor)
                    .Include(p => p.ProductoImagens)
                    .Where(p => p.EstaActivo);

                // ========== SEARCH TERM ==========
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    var searchTerm = request.Query.ToLower();
                    query = query.Where(p =>
                        p.NombreProducto.ToLower().Contains(searchTerm) ||
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(searchTerm)) ||
                        p.Sku.ToLower().Contains(searchTerm) ||
                        (p.Categoria != null && p.Categoria.NombreCategoria.ToLower().Contains(searchTerm)) ||
                        (p.Marca != null && p.Marca.NombreMarca.ToLower().Contains(searchTerm))
                    );
                }

                // ========== CATEGORY FILTERS ==========
                // Support both single and multiple category filtering
                if (request.CategoriaIds != null && request.CategoriaIds.Length > 0)
                {
                    // NEW: Multiple categories (takes priority)
                    query = query.Where(p => request.CategoriaIds.Contains(p.CategoriaId));
                }
                else if (request.CategoriaId.HasValue)
                {
                    // LEGACY: Single category (backward compatibility)
                    query = query.Where(p => p.CategoriaId == request.CategoriaId.Value);
                }

                // ========== BRAND FILTERS ==========
                // Support both single and multiple brand filtering
                if (request.MarcaIds != null && request.MarcaIds.Length > 0)
                {
                    // NEW: Multiple brands (takes priority)
                    query = query.Where(p => request.MarcaIds.Contains(p.MarcaId));
                }
                else if (request.MarcaId.HasValue)
                {
                    // LEGACY: Single brand (backward compatibility)
                    query = query.Where(p => p.MarcaId == request.MarcaId.Value);
                }

                // ========== PRICE RANGE ==========
                if (request.PrecioMin.HasValue)
                {
                    query = query.Where(p => p.PrecioBase >= request.PrecioMin.Value);
                }

                if (request.PrecioMax.HasValue)
                {
                    query = query.Where(p => p.PrecioBase <= request.PrecioMax.Value);
                }

                // ========== IN STOCK FILTER ==========
                if (request.EnStock == true)
                {
                    query = query.Where(p => p.StockTotal > 0);
                }

                // ========== DISCOUNT FILTER ==========
                if (request.EnDescuento == true)
                {
                    query = query.Where(p => p.PrecioPromocional.HasValue && p.PrecioPromocional < p.PrecioBase);
                }

                // ========== SORTING ==========
                query = (request.OrderBy?.ToLower(), request.OrderDirection?.ToLower()) switch
                {
                    ("precio", "asc") => query.OrderBy(p => p.PrecioBase),
                    ("precio", "desc") => query.OrderByDescending(p => p.PrecioBase),
                    ("nombre", "desc") => query.OrderByDescending(p => p.NombreProducto),
                    ("nombre", "asc") => query.OrderBy(p => p.NombreProducto),
                    ("destacado", _) => query.OrderByDescending(p => p.EsDestacado).ThenBy(p => p.NombreProducto),
                    ("fecha", "desc") => query.OrderByDescending(p => p.FechaCreacion),
                    ("fecha", "asc") => query.OrderBy(p => p.FechaCreacion),
                    _ => query.OrderBy(p => p.NombreProducto) // default
                };

                // Get total count BEFORE pagination
                var totalItems = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var dtos = Mapper.ToProductoDto(items);

                // Return paged result
                var pagedResult = new PagedResult<ProductoDto>(
                    dtos,
                    totalItems,
                    request.Page,
                    request.PageSize
                );

                return ApiResponse<PagedResult<ProductoDto>>.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagedResult<ProductoDto>>(9000);
            }
        }

        //public async Task<ApiResponse<PagedResult<ProductoDto>>> SearchAsync(ProductoSearchRequest request)
        //{
        //    var query = _context.Productos
        //        .Include(p => p.Categoria)
        //        .Include(p => p.Marca)
        //        .Include(p => p.ProductoImagens)
        //        .Where(p => p.EstaActivo)
        //        .AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(request.Query))
        //        query = query.Where(p =>
        //            p.NombreProducto.Contains(request.Query) ||
        //            p.Descripcion.Contains(request.Query));

        //    if (request.CategoriaId.HasValue)
        //        query = query.Where(p => p.CategoriaId == request.CategoriaId);

        //    if (request.MarcaId.HasValue)
        //        query = query.Where(p => p.MarcaId == request.MarcaId);

        //    if (request.PrecioMin.HasValue)
        //        query = query.Where(p => p.PrecioBase >= request.PrecioMin);

        //    if (request.PrecioMax.HasValue)
        //        query = query.Where(p => p.PrecioBase <= request.PrecioMax);

        //    var total = await query.CountAsync();
        //    var productos = await query
        //        .Skip((request.Page - 1) * request.PageSize)
        //        .Take(request.PageSize)
        //        .ToListAsync();

        //    var dtos = Mapper.ToProductoDto(productos);

        //    return ApiResponse<PagedResult<ProductoDto>>.Ok(new PagedResult<ProductoDto>
        //    {
        //        Items = dtos,
        //        TotalItems = total,
        //        Page = request.Page,
        //        PageSize = request.PageSize
        //    });
        //}

        public async Task<ApiResponse<List<ProductoDto>>> GetFeaturedAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Categoria)
                    .Include(p => p.Marca)
                    .Include(p => p.ProductoImagens)
                    .Where(p => p.EstaActivo && p.EsDestacado && p.StockTotal > 0)
                    .OrderByDescending(p => p.FechaCreacion)
                    .Take(12)
                    .ToListAsync();

                var dtos = Mapper.ToProductoDto(productos);
                return ApiResponse<List<ProductoDto>>.Ok(dtos.ToList());
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<ProductoDto>>(9000);
            }
        }

        public async Task<ApiResponse<StockStatusDto>> CheckStockAsync(int productoId, int cantidad)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
                return await ReturnErrorAsync<StockStatusDto>(300);

            var reservedStock = await GetReservedStockAsync(productoId);
            var availableStock = producto.StockTotal - reservedStock;

            return ApiResponse<StockStatusDto>.Ok(new StockStatusDto
            {
                ProductoId = productoId,
                StockTotal = producto.StockTotal,
                StockReservado = reservedStock,
                StockDisponible = availableStock,
                SuficienteStock = availableStock >= cantidad,
                EsPreorden = producto.PermitePreorden && availableStock < cantidad
            });
        }

        private async Task<int> GetReservedStockAsync(int productoId)
        {
            // Stock in active carts (pending checkout)
            var cartReserved = await _context.CarritoItems
                .Where(ci => ci.ProductoId == productoId && ci.Carrito.EstatusVentaId == 1)
                .SumAsync(ci => ci.Cantidad);

            // Stock in unpaid orders
            var orderReserved = await _context.OrdenItems
                .Where(oi => oi.ProductoId == productoId &&
                            oi.Orden.EstatusVentaId >= 1 && oi.Orden.EstatusVentaId <= 3)
                .SumAsync(oi => oi.Cantidad);

            return cartReserved + orderReserved;
        }

        public async Task RecalculateStockAsync(int productoId)
        {
            var total = await _context.ProductoVariacions
            .Where(v => v.ProductoId == productoId && v.EstaActivo)
            .SumAsync(v => (int?)v.Stock) ?? 0;

            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == productoId);
            if (producto != null)
            {
                producto.StockTotal = total;
                producto.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

    }
}
