using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.InventarioDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API_TI.Services.Implementations
{
    public class InventarioService : BaseService, IInventarioService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly TiPcComponentsContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public InventarioService(
            ILogger<AuditService> logger,
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor
        ) : base(errorService, httpContextAccessor, null)
        {
            _logger = logger;
            _context = context;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<object>> AdjustStockAsync(AdjustStockRequest request, int usuarioId)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(request.ProductoId);
                if (producto == null) return await ReturnErrorAsync<object>(300);

                var stockAnterior = producto.StockTotal;
                producto.StockTotal += request.Cantidad;

                _context.InventarioMovimientos.Add(new InventarioMovimiento
                {
                    ProductoId = request.ProductoId,
                    TipoMovimientoInventarioId = request.TipoMovimientoId,
                    Cantidad = Math.Abs(request.Cantidad),
                    StockAnterior = stockAnterior,
                    StockNuevo = producto.StockTotal,
                    Referencia = request.Referencia,
                    FechaMovimiento = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await AuditAsync("Inventory.StockAdjusted", new { ProductoId = request.ProductoId, Cantidad = request.Cantidad }, usuarioId);

                return ApiResponse<object>.Ok(null, "Stock ajustado");

            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
            
        }
    }
}
