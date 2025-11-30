using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class AdminDashboardService : BaseService, IAdminDashboardService
    {
        private readonly TiPcComponentsContext _context;

        public AdminDashboardService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardMetricsDto>> GetMetricsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var last30Days = today.AddDays(-30);
                var last7Days = today.AddDays(-7);

                var metrics = new DashboardMetricsDto
                {
                    // Sales
                    TotalSales = await _context.Ordens
                        .Where(o => o.EstatusVentaId >= 4)
                        .SumAsync(o => o.Total),

                    SalesToday = await _context.Ordens
                        .Where(o => o.FechaOrden >= today && o.EstatusVentaId >= 4)
                        .SumAsync(o => (decimal?)o.Total) ?? 0,

                    SalesLast7Days = await _context.Ordens
                        .Where(o => o.FechaOrden >= last7Days && o.EstatusVentaId >= 4)
                        .SumAsync(o => o.Total),

                    SalesLast30Days = await _context.Ordens
                        .Where(o => o.FechaOrden >= last30Days && o.EstatusVentaId >= 4)
                        .SumAsync(o => o.Total),

                    // Orders
                    TotalOrders = await _context.Ordens.CountAsync(),

                    OrdersToday = await _context.Ordens
                        .CountAsync(o => o.FechaOrden >= today),

                    PendingOrders = await _context.Ordens
                        .CountAsync(o => o.EstatusVentaId < 4),

                    CompletedOrders = await _context.Ordens
                        .CountAsync(o => o.EstatusVentaId >= 4),

                    // Users
                    TotalUsers = await _context.Usuarios.CountAsync(u => u.EstaActivo),

                    ActiveUsers = await _context.Usuarios
                        .CountAsync(u => u.EstaActivo && u.UltimoLoginUsuario >= last30Days),

                    NewUsersToday = await _context.Usuarios
                        .CountAsync(u => u.FechaCreacion >= today),

                    NewUsersLast7Days = await _context.Usuarios
                        .CountAsync(u => u.FechaCreacion >= last7Days),

                    NewUsersLast30Days = await _context.Usuarios
                        .CountAsync(u => u.FechaCreacion >= last30Days),

                    // Products
                    TotalProducts = await _context.Productos.CountAsync(p => p.EstaActivo),

                    LowStockProducts = await _context.Productos
                        .CountAsync(p => p.EstaActivo && p.StockTotal <= p.StockMinimo),

                    OutOfStockProducts = await _context.Productos
                        .CountAsync(p => p.EstaActivo && p.StockTotal == 0),

                    SalesData = await GetWeeklySalesDataAsync(28), // Last 4 weeks

                    LowStockProductsList = await _context.Productos
                        .Where(p => p.EstaActivo && p.StockTotal <= p.StockMinimo)
                        .OrderBy(p => p.StockTotal)
                        .Take(10)
                        .Select(p => new LowStockProductDto
                        {
                            IdProducto = p.IdProducto,
                            NombreProducto = p.NombreProducto,
                            Stock = p.StockTotal,
                            StockMinimo = p.StockMinimo
                        })
                        .ToListAsync(),

                    RecentOrders = await _context.Ordens
                        .Include(o => o.Usuario)
                        .Include(o => o.EstatusVenta)
                        .OrderByDescending(o => o.FechaOrden)
                        .Take(10)
                        .Select(o => new RecentOrderDto
                        {
                            IdOrden = o.IdOrden,
                            NombreCliente = $"{o.Usuario.NombreUsuario} {o.Usuario.ApellidoPaterno}",
                            FechaCreacion = o.FechaOrden,
                            Total = o.Total,
                            Estado = o.EstatusVenta.NombreEstatusVenta
                        })
                        .ToListAsync(),

                    // Top products
                    TopSellingProducts = await _context.OrdenItems
                        .Where(oi => oi.Orden.FechaOrden >= last30Days && oi.Orden.EstatusVentaId >= 4)
                        .GroupBy(oi => new { oi.ProductoId, oi.Producto.NombreProducto, oi.Producto.PrecioBase })
                        .Select(g => new TopProductDto
                        {
                            ProductoId = g.Key.ProductoId,
                            Nombre = g.Key.NombreProducto,
                            CantidadVendida = g.Sum(oi => oi.Cantidad),
                            TotalVentas = g.Sum(oi => oi.PrecioUnitario * oi.Cantidad),
                            PrecioBase = g.Key.PrecioBase // ⭐ ADDED
                        })
                        .OrderByDescending(p => p.TotalVentas)
                        .Take(5)
                        .ToListAsync(),

                    // Revenue trend (last 7 days)
                    RevenueTrend = await GetRevenueTrendAsync(7)
                };

                return ApiResponse<DashboardMetricsDto>.Ok(metrics);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DashboardMetricsDto>(9000);
            }
        }

        private async Task<List<WeeklySalesDto>> GetWeeklySalesDataAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var weeksCount = days / 7;
            var salesData = new List<WeeklySalesDto>();

            for (int i = 0; i < weeksCount; i++)
            {
                var weekStart = startDate.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(7);

                var weekSales = await _context.Ordens
                    .Where(o => o.FechaOrden >= weekStart && o.FechaOrden < weekEnd && o.EstatusVentaId >= 4)
                    .SumAsync(o => (decimal?)o.Total) ?? 0;

                salesData.Add(new WeeklySalesDto
                {
                    Label = $"Semana {i + 1}",
                    Value = weekSales
                });
            }

            return salesData;
        }


        private async Task<List<DailyRevenueDto>> GetRevenueTrendAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var revenue = await _context.Ordens
                .Where(o => o.FechaOrden >= startDate && o.EstatusVentaId >= 4)
                .GroupBy(o => o.FechaOrden.Date)
                .Select(g => new DailyRevenueDto
                {
                    Fecha = g.Key,
                    Ventas = g.Sum(o => o.Total),
                    Ordenes = g.Count()
                })
                .OrderBy(r => r.Fecha)
                .ToListAsync();

            return revenue;
        }
    }
}
