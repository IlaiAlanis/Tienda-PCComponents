using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CotizacionDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ShippingService : BaseService, IShippingService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<ShippingService> _logger;

        public ShippingService(
            TiPcComponentsContext context,
            ILogger<ShippingService> logger,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<CotizacionesEnvioResponse>> GetShippingQuotesAsync(int userId, CotizarEnvioRequest request)
        {
            var direccion = await _context.Direccions
                .Include(d => d.Pais)
                .Include(d => d.Estado)
                .Include(d => d.Ciudad)
                .FirstOrDefaultAsync(d => d.IdDireccion == request.DireccionId && d.UsuarioId == userId);

            if (direccion == null)
                return await ReturnErrorAsync<CotizacionesEnvioResponse>(902);

            var costo = CalculateShippingCost(direccion.Estado.NombreEstado, request.PesoKg);

            // ADD THIS:
            var cotizacion = new CotizacionEnvioDto
            {
                Proveedor = "TI PC Components",
                Servicio = "Envío estándar",
                Costo = costo,
                DiasEntrega = 5,
                Descripcion = "Entrega en 3-5 días hábiles"
            };

            var direccionCompleta = $"{direccion.Calle} {direccion.NumeroExterior}, " +
                                   $"{direccion.Ciudad.NombreCiudad}, {direccion.Estado.NombreEstado}";

            return ApiResponse<CotizacionesEnvioResponse>.Ok(new CotizacionesEnvioResponse
            {
                Cotizaciones = new List<CotizacionEnvioDto> { cotizacion },
                DireccionDestino = direccionCompleta
            });
        }

        public async Task<ApiResponse<CotizacionEnvioDto>> GetLocalShippingRateAsync(int direccionId, decimal pesoKg)
        {
            try
            {
                var direccion = await _context.Direccions.FindAsync(direccionId);
                if (direccion == null)
                    return await ReturnErrorAsync<CotizacionEnvioDto>(902);

                // Find matching zone
                var zona = await _context.ZonaEnvios
                    .Where(z => z.EstaActivo)
                    .Where(z => z.PaisNombre == null || z.PaisNombre == direccion.Pais.NombrePais)
                    .Where(z => z.EstadoNombre == null || z.EstadoNombre == direccion.Estado.NombreEstado)
                    .Where(z => z.CodigoPostalDesde == null ||
                               (direccion.CodigoPostal.CompareTo(z.CodigoPostalDesde) >= 0 &&
                                direccion.CodigoPostal.CompareTo(z.CodigoPostalHasta) <= 0))
                    .FirstOrDefaultAsync();

                if (zona == null)
                {
                    // Default zone
                    zona = new ZonaEnvio
                    {
                        CostoBase = 100m,
                        CostoPorKg = 15m,
                        DiasEntregaMin = 5,
                        DiasEntregaMax = 10
                    };
                }

                var costo = zona.CostoBase + (pesoKg * zona.CostoPorKg);

                var cotizacion = new CotizacionEnvioDto
                {
                    Proveedor = "Local",
                    Servicio = zona.NombreZona ?? "Envío estándar",
                    Costo = costo,
                    DiasEntrega = zona.DiasEntregaMax,
                    Descripcion = $"Entrega en {zona.DiasEntregaMin}-{zona.DiasEntregaMax} días hábiles"
                };

                return ApiResponse<CotizacionEnvioDto>.Ok(cotizacion);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CotizacionEnvioDto>(9000);
            }
        }

        // This is just sample code; there is no actual implementation of the API.
        private decimal CalculateShippingCost(string estado, decimal peso)
        {
            var costoBase = estado switch
            {
                "Nuevo León" => 50m,
                "Ciudad de México" => 100m,
                "Jalisco" => 120m,
                _ => 150m
            };

            return costoBase + (peso * 15m); // $15 per kg
        }
    }
}
