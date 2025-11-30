using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.CiudadDTOs;
using API_TI.Models.DTOs.EstadoDTOs;
using API_TI.Models.DTOs.PaisDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SendGrid.Helpers.Mail;

namespace API_TI.Services.Implementations
{
    public class GeografiaService : BaseService, IGeografiaService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

        public GeografiaService(
            TiPcComponentsContext context,
            IMemoryCache cache,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResponse<IList<PaisDto>>> GetCountriesAsync()
        {
            try
            {
                var cacheKey = "countries_all";

                if (!_cache.TryGetValue(cacheKey, out IList<PaisDto> countries))
                {
                    countries = await _context.Pais
                        .AsNoTracking()
                        .OrderBy(p => p.NombrePais)
                        .Select(p => new PaisDto
                        {
                            IdPais = p.IdPais,
                            NombrePais = p.NombrePais,
                            Codigo = p.Codigo
                        })
                        .ToListAsync();

                    _cache.Set(cacheKey, countries, _cacheExpiration);
                }

                return ApiResponse<IList<PaisDto>>.Ok(countries);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<PaisDto>>(9000);
            }
        }

        public async Task<ApiResponse<IList<EstadoDto>>> GetStatesByCountryAsync(int paisId)
        {
            try
            {
                var cacheKey = $"states_country_{paisId}";

                if (!_cache.TryGetValue(cacheKey, out IList<EstadoDto> states))
                {
                    states = await _context.Estados
                        .AsNoTracking()
                        .Where(e => e.PaisId == paisId)
                        .OrderBy(e => e.NombreEstado)
                        .Select(e => new EstadoDto
                        {
                            IdEstado = e.IdEstado,
                            PaisId = e.PaisId,
                            NombreEstado = e.NombreEstado
                        })
                        .ToListAsync();

                    _cache.Set(cacheKey, states, _cacheExpiration);
                }

                return ApiResponse<IList<EstadoDto>>.Ok(states);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<EstadoDto>>(9000);
            }
        }

        public async Task<ApiResponse<IList<CiudadDto>>> GetCitiesByStateAsync(int estadoId)
        {
            try
            {
                var cacheKey = $"cities_state_{estadoId}";

                if (!_cache.TryGetValue(cacheKey, out IList<CiudadDto> cities))
                {
                    cities = await _context.Ciudads
                        .AsNoTracking()
                        .Where(c => c.EstadoId == estadoId)
                        .OrderBy(c => c.NombreCiudad)
                        .Select(c => new CiudadDto
                        {
                            IdCiudad = c.IdCiudad,
                            EstadoId = c.EstadoId,
                            NombreCiudad = c.NombreCiudad
                        })
                        .ToListAsync();

                    _cache.Set(cacheKey, cities, _cacheExpiration);
                }

                return ApiResponse<IList<CiudadDto>>.Ok(cities);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<CiudadDto>>(9000);
            }
        }

    }

}
