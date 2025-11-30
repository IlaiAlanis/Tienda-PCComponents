using API_TI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API_TI.Services.Background
{
    public class StockReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockReservationCleanupService> _logger;
        private readonly TimeSpan _reservationTimeout = TimeSpan.FromMinutes(15);

        public StockReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<StockReservationCleanupService> logger)
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
                    await CleanupExpiredReservations(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in stock cleanup service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task CleanupExpiredReservations(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TiPcComponentsContext>();

            var expiredTime = DateTime.UtcNow.Add(-_reservationTimeout);

            var expiredItems = await context.CarritoItems
                .Include(ci => ci.Carrito)
                .Where(ci => ci.Carrito != null &&
                             ci.FechaReserva < expiredTime &&
                             ci.Carrito.EstatusVentaId == 1)
                .ToListAsync(cancellationToken);

            if (expiredItems.Any())
            {
                context.CarritoItems.RemoveRange(expiredItems);
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Cleared {Count} expired cart reservations", expiredItems.Count);
            }
        }
    }

}
