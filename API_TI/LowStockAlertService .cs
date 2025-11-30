using API_TI.Data;
using API_TI.Models.dbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API_TI.Services.Background
{
    public class LowStockAlertService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LowStockAlertService> _logger;

        public LowStockAlertService(
            IServiceProvider serviceProvider,
            ILogger<LowStockAlertService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckLowStockProducts();
                    await Task.Delay(TimeSpan.FromHours(6), stoppingToken); // Check every 6 hours
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in low stock alert service");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
        }

        private async Task CheckLowStockProducts()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TiPcComponentsContext>();

            var lowStockProducts = await context.Productos
                .Where(p => p.EstaActivo &&
                           p.AlertaBajoStock &&
                           p.StockTotal <= p.StockMinimo)
                .Select(p => new
                {
                    p.IdProducto,
                    p.NombreProducto,
                    p.StockTotal,
                    p.StockMinimo
                })
                .ToListAsync();

            if (!lowStockProducts.Any()) return;

            // Get admin users
            var admins = await context.Usuarios
                .Where(u => u.RolId == 1 && u.EstaActivo)
                .ToListAsync();

            foreach (var admin in admins)
            {
                foreach (var producto in lowStockProducts)
                {
                    // Check if already notified
                    var exists = await context.NotificacionUsuarios
                        .AnyAsync(n => n.UsuarioId == admin.IdUsuario &&
                                      n.Tipo == "LOW_STOCK" &&
                                      n.Mensaje.Contains(producto.IdProducto.ToString()) &&
                                      n.FechaCreacion >= DateTime.UtcNow.AddHours(-24));

                    if (!exists)
                    {
                        context.NotificacionUsuarios.Add(new NotificacionUsuario
                        {
                            UsuarioId = admin.IdUsuario,
                            Titulo = "Stock Bajo",
                            Mensaje = $"{producto.NombreProducto} - Stock: {producto.StockTotal}/{producto.StockMinimo}",
                            Tipo = "LOW_STOCK",
                            Leido = false,
                            FechaCreacion = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Created low stock alerts for {Count} products", lowStockProducts.Count);
        }
    }
}
