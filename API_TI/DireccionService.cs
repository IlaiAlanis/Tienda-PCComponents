using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class DireccionService : BaseService, IDireccionService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<DireccionService> _logger;

        private const int DEFAULT_PAIS_ID = 1; // Mexico

        public DireccionService(
            TiPcComponentsContext context,
            ILogger<DireccionService> logger,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<IList<DireccionDto>>> GetUserAddressesAsync(int userId)
        {
            try
            {
                var addresses = await _context.Direccions
                    .Include(x => x.Pais)
                    .Include(x => x.Estado)
                    .Include(x => x.Ciudad)
                    .Where(x => x.UsuarioId == userId)
                    .OrderByDescending(x => x.EsDefault)
                    .ThenByDescending(x => x.FechaCreacion)
                    .ToListAsync();

                var dtos = addresses.Select(MapToDireccionDto).ToList();
                return ApiResponse<IList<DireccionDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<IList<DireccionDto>>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> GetAddressByIdAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Direccions
                    .Include(x => x.Pais)
                    .Include(x => x.Estado)
                    .Include(x => x.Ciudad)
                    .FirstOrDefaultAsync(x => x.IdDireccion == addressId && x.UsuarioId == userId);

                if (address == null)
                    return await ReturnErrorAsync<DireccionDto>(902, "Dirección no encontrada");

                return ApiResponse<DireccionDto>.Ok(MapToDireccionDto(address));
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> CreateAddressAsync(int userId, CreateDireccionRequest request)
        {
            try
            {
                // STEP 1: Validate that we have location data (either IDs or names)
                if (!request.HasValidLocation)
                {
                    return await ReturnErrorAsync<DireccionDto>(902,
                        "Debe proporcionar la ubicación (IDs o nombres de país, estado y ciudad)");
                }

                // STEP 2: Resolve location IDs
                // Priority: 1) Provided IDs, 2) Names lookup, 3) Error
                var (paisId, estadoId, ciudadId) = await ResolveLocationIdsAsync(
                    request.PaisId,
                    request.EstadoId,
                    request.CiudadId,
                    request.PaisNombre,
                    request.EstadoNombre,
                    request.CiudadNombre
                );

                if (ciudadId == 0)
                {
                    return await ReturnErrorAsync<DireccionDto>(902,
                        "No se pudo determinar la ubicación. Verifique que ciudad, estado y país sean correctos.");
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Handle default address logic
                    if (request.EsDefault)
                    {
                        await UnsetAllDefaultsAsync(userId);
                    }
                    else
                    {
                        var hasAddresses = await _context.Direccions.AnyAsync(d => d.UsuarioId == userId);
                        if (!hasAddresses)
                            request.EsDefault = true;
                    }

                    // STEP 3: Create address entity
                    var address = new Direccion
                    {
                        UsuarioId = userId,

                        // Location
                        PaisId = paisId,
                        EstadoId = estadoId,
                        CiudadId = ciudadId,

                        // Address details
                        CodigoPostal = request.CodigoPostal,
                        Colonia = request.Colonia,
                        Calle = request.Calle,
                        NumeroInterior = request.NumeroInterior,
                        NumeroExterior = request.NumeroExterior,
                        Telefono = request.Telefono,

                        // Google Places data
                        GooglePlaceId = request.PlaceId,
                        DireccionCompleta = request.DireccionCompleta,
                        Latitud = request.Latitud,
                        Longitud = request.Longitud,

                        // Additional info
                        Referencia = request.Referencia,
                        Notas = request.Notas,

                        // Metadata
                        Etiqueta = request.Etiqueta,
                        EsDefault = request.EsDefault,
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.Direccions.Add(address);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await AuditAsync("User.Address.Created", new
                    {
                        UserId = userId,
                        AddressId = address.IdDireccion,
                        HasCoordinates = request.Latitud.HasValue && request.Longitud.HasValue
                    }, userId);

                    // Load navigation properties
                    await _context.Entry(address).Reference(x => x.Pais).LoadAsync();
                    await _context.Entry(address).Reference(x => x.Estado).LoadAsync();
                    await _context.Entry(address).Reference(x => x.Ciudad).LoadAsync();

                    return ApiResponse<DireccionDto>.Ok(MapToDireccionDto(address), "Dirección creada exitosamente");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> UpdateAddressAsync(int userId, int addressId, UpdateDireccionRequest request)
        {
            try
            {
                var address = await _context.Direccions
                    .FirstOrDefaultAsync(x => x.IdDireccion == addressId && x.UsuarioId == userId);

                if (address == null)
                    return await ReturnErrorAsync<DireccionDto>(902, "Dirección no encontrada");

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (request.EsDefault == true)
                        await UnsetAllDefaultsAsync(userId);

                    // Update location if provided
                    if (request.HasLocationUpdate)
                    {
                        var (paisId, estadoId, ciudadId) = await ResolveLocationIdsAsync(
                            request.PaisId ?? address.PaisId,
                            request.EstadoId ?? address.EstadoId,
                            request.CiudadId ?? address.CiudadId,
                            request.PaisNombre,
                            request.EstadoNombre,
                            request.CiudadNombre
                        );

                        if (ciudadId != 0)
                        {
                            address.PaisId = paisId;
                            address.EstadoId = estadoId;
                            address.CiudadId = ciudadId;
                        }
                    }

                    // Update address fields
                    if (request.CodigoPostal != null) address.CodigoPostal = request.CodigoPostal;
                    if (request.Colonia != null) address.Colonia = request.Colonia;
                    if (request.Calle != null) address.Calle = request.Calle;
                    if (request.NumeroInterior != null) address.NumeroInterior = request.NumeroInterior;
                    if (request.NumeroExterior != null) address.NumeroExterior = request.NumeroExterior;
                    if (request.Telefono != null) address.Telefono = request.Telefono;
                    if (request.Etiqueta != null) address.Etiqueta = request.Etiqueta;

                    // Update Google Places data
                    if (request.GooglePlaceId != null) address.GooglePlaceId = request.GooglePlaceId;
                    if (request.DireccionCompleta != null) address.DireccionCompleta = request.DireccionCompleta;
                    if (request.Latitud.HasValue) address.Latitud = request.Latitud;
                    if (request.Longitud.HasValue) address.Longitud = request.Longitud;

                    // Update additional info
                    if (request.Referencia != null) address.Referencia = request.Referencia;
                    if (request.Notas != null) address.Notas = request.Notas;

                    if (request.EsDefault.HasValue) address.EsDefault = request.EsDefault.Value;

                    address.FechaActualizacion = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await AuditAsync("User.Address.Updated", new { UserId = userId, AddressId = addressId }, userId);

                    await _context.Entry(address).Reference(x => x.Pais).LoadAsync();
                    await _context.Entry(address).Reference(x => x.Estado).LoadAsync();
                    await _context.Entry(address).Reference(x => x.Ciudad).LoadAsync();

                    return ApiResponse<DireccionDto>.Ok(MapToDireccionDto(address), "Dirección actualizada exitosamente");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Direccions
                    .FirstOrDefaultAsync(x => x.IdDireccion == addressId && x.UsuarioId == userId);

                if (address == null)
                    return await ReturnErrorAsync<object>(902, "Dirección no encontrada");

                if (address.EsDefault)
                {
                    var otherAddresses = await _context.Direccions
                        .AnyAsync(x => x.UsuarioId == userId && x.IdDireccion != addressId);

                    if (otherAddresses)
                        return await ReturnErrorAsync<object>(5, "Debe establecer otra dirección como predeterminada primero");
                }

                _context.Direccions.Remove(address);
                await _context.SaveChangesAsync();

                await AuditAsync("User.Address.Deleted", new { UserId = userId, AddressId = addressId }, userId);

                return ApiResponse<object>.Ok(null, "Dirección eliminada exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> SetDefaultAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Direccions
                    .Include(x => x.Pais)
                    .Include(x => x.Estado)
                    .Include(x => x.Ciudad)
                    .FirstOrDefaultAsync(d => d.IdDireccion == addressId && d.UsuarioId == userId);

                if (address == null)
                    return await ReturnErrorAsync<DireccionDto>(902, "Dirección no encontrada");

                if (address.EsDefault)
                    return ApiResponse<DireccionDto>.Ok(MapToDireccionDto(address), "Ya es la dirección predeterminada");

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await UnsetAllDefaultsAsync(userId);
                    address.EsDefault = true;
                    address.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await AuditAsync("User.Address.SetDefault", new { UserId = userId, AddressId = addressId }, userId);

                    return ApiResponse<DireccionDto>.Ok(MapToDireccionDto(address), "Dirección predeterminada actualizada");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }

        // ============= PRIVATE HELPER METHODS =============

        private async Task UnsetAllDefaultsAsync(int userId)
        {
            var defaults = await _context.Direccions
                .Where(x => x.UsuarioId == userId && x.EsDefault)
                .ToListAsync();

            foreach (var d in defaults)
            {
                d.EsDefault = false;
                d.FechaActualizacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Resolve location IDs from either provided IDs or location name strings
        /// PRIORITY: 1) Use provided IDs if all present, 2) Look up by names, 3) Return error (0,0,0)
        /// </summary>
        private async Task<(int paisId, int estadoId, int ciudadId)> ResolveLocationIdsAsync(
            int? providedPaisId,
            int? providedEstadoId,
            int? providedCiudadId,
            string? paisNombre,
            string? estadoNombre,
            string? ciudadNombre)
        {
            // SCENARIO 1: All IDs provided (from dropdowns) - USE DIRECTLY
            if (providedCiudadId.HasValue && providedEstadoId.HasValue && providedPaisId.HasValue)
            {
                _logger.LogDebug("Using provided location IDs: Pais={PaisId}, Estado={EstadoId}, Ciudad={CiudadId}",
                    providedPaisId, providedEstadoId, providedCiudadId);
                return (providedPaisId.Value, providedEstadoId.Value, providedCiudadId.Value);
            }

            // SCENARIO 2: Names provided (from Google Places or manual entry) - LOOKUP IN DATABASE
            if (!string.IsNullOrWhiteSpace(ciudadNombre))
            {
                _logger.LogDebug("Looking up location by names: Ciudad={Ciudad}, Estado={Estado}, Pais={Pais}",
                    ciudadNombre, estadoNombre, paisNombre);

                // Try exact match first
                var ciudad = await _context.Ciudads
                    .Include(c => c.Estado)
                    .ThenInclude(e => e.Pais)
                    .FirstOrDefaultAsync(c =>
                        c.NombreCiudad.ToLower() == ciudadNombre.ToLower() &&
                        (string.IsNullOrWhiteSpace(estadoNombre) ||
                         c.Estado.NombreEstado.ToLower() == estadoNombre.ToLower()) &&
                        (string.IsNullOrWhiteSpace(paisNombre) ||
                         c.Estado.Pais.NombrePais.ToLower() == paisNombre.ToLower()));

                if (ciudad != null)
                {
                    _logger.LogDebug("Found location: Pais={PaisId}, Estado={EstadoId}, Ciudad={CiudadId}",
                        ciudad.Estado.PaisId, ciudad.EstadoId, ciudad.IdCiudad);
                    return (ciudad.Estado.PaisId, ciudad.EstadoId, ciudad.IdCiudad);
                }

                // Try partial match (in case of accents, abbreviations, etc.)
                ciudad = await _context.Ciudads
                    .Include(c => c.Estado)
                    .ThenInclude(e => e.Pais)
                    .FirstOrDefaultAsync(c =>
                        c.NombreCiudad.ToLower().Contains(ciudadNombre.ToLower()) &&
                        (string.IsNullOrWhiteSpace(estadoNombre) ||
                         c.Estado.NombreEstado.ToLower().Contains(estadoNombre.ToLower())));

                if (ciudad != null)
                {
                    _logger.LogInformation("Found location with partial match: {Ciudad} -> {CiudadDB}",
                        ciudadNombre, ciudad.NombreCiudad);
                    return (ciudad.Estado.PaisId, ciudad.EstadoId, ciudad.IdCiudad);
                }

                _logger.LogWarning("Could not find location for: Ciudad={Ciudad}, Estado={Estado}",
                    ciudadNombre, estadoNombre);
            }

            // SCENARIO 3: No valid location data provided
            return (0, 0, 0);
        }

        /// <summary>
        /// Map Direccion entity to DireccionDto
        /// </summary>
        private DireccionDto MapToDireccionDto(Direccion address)
        {
            return new DireccionDto
            {
                IdDireccion = address.IdDireccion,
                Nombre = address.Etiqueta ?? "Sin etiqueta",
                Telefono = address.Telefono,

                // Address details
                Calle = address.Calle,
                NumeroExterior = address.NumeroExterior,
                NumeroInterior = address.NumeroInterior,
                Colonia = address.Colonia,
                CodigoPostal = address.CodigoPostal,

                // Location strings
                CiudadNombre = address.Ciudad?.NombreCiudad ?? "N/A",
                EstadoNombre = address.Estado?.NombreEstado ?? "N/A",
                PaisNombre = address.Pais?.NombrePais ?? "N/A",

                // Location IDs
                CiudadId = address.CiudadId,
                EstadoId = address.EstadoId,
                PaisId = address.PaisId,

                // Coordinates
                Latitud = address.Latitud,
                Longitud = address.Longitud,

                // Google Places
                GooglePlaceId = address.GooglePlaceId,
                DireccionCompleta = address.DireccionCompleta,

                // Additional info
                Referencia = address.Referencia,
                Notas = address.Notas,

                // Flags
                EsPrincipal = address.EsDefault,
                FechaCreacion = address.FechaCreacion,
                FechaActualizacion = address.FechaActualizacion
            };
        }
    }
}