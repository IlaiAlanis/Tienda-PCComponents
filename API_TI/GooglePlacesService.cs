using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using System.Text.Json;

namespace API_TI.Services.Implementations
{
    public class GooglePlacesService : BaseService, IGooglePlacesService
    {
        private readonly ILogger<GooglePlacesService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GooglePlacesService(
            ILogger<GooglePlacesService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = config["Google:Places:ApiKey"];
        }

        public async Task<ApiResponse<bool>> ValidatePlaceIdAsync(string placeId)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return await ReturnErrorAsync<bool>(9003, "Error validando dirección con Google");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var status = data.GetProperty("status").GetString();
                return status == "OK"
                    ? ApiResponse<bool>.Ok(true)
                    : await ReturnErrorAsync<bool>(902, "Dirección inválida");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<bool>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> GetAddressDetailsAsync(string placeId)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=address_components,geometry,formatted_address&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return await ReturnErrorAsync<DireccionDto>(9003);

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                if (data.GetProperty("status").GetString() != "OK")
                    return await ReturnErrorAsync<DireccionDto>(902);

                var result = data.GetProperty("result");
                var location = result.GetProperty("geometry").GetProperty("location");

                var dto = new DireccionDto
                {
                    Latitud = location.GetProperty("lat").GetDecimal(),
                    Longitud = location.GetProperty("lng").GetDecimal(),
                    DireccionCompleta = result.GetProperty("formatted_address").GetString()
                };

                // Parse address components
                foreach (var component in result.GetProperty("address_components").EnumerateArray())
                {
                    var types = component.GetProperty("types").EnumerateArray().Select(t => t.GetString()).ToList();
                    var longName = component.GetProperty("long_name").GetString();

                    if (types.Contains("country")) dto.PaisNombre = longName;
                    if (types.Contains("administrative_area_level_1")) dto.EstadoNombre = longName;
                    if (types.Contains("locality")) dto.CiudadNombre = longName;
                    if (types.Contains("postal_code")) dto.CodigoPostal = longName;
                    if (types.Contains("sublocality_level_1")) dto.Colonia = longName;
                    if (types.Contains("route")) dto.Calle = longName;
                    if (types.Contains("street_number")) dto.NumeroExterior = longName;
                }

                return ApiResponse<DireccionDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }

        public async Task<ApiResponse<DireccionDto>> SearchByPostalCodeAsync(string codigoPostal)
        {
            try
            {
                // Option 1: Use Google Geocoding API
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={codigoPostal},Mexico&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return await ReturnErrorAsync<DireccionDto>(9003);

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                if (data.GetProperty("status").GetString() != "OK")
                    return await ReturnErrorAsync<DireccionDto>(902, "Código postal no encontrado");

                var result = data.GetProperty("results").EnumerateArray().First();
                var location = result.GetProperty("geometry").GetProperty("location");

                var dto = new DireccionDto
                {
                    CodigoPostal = codigoPostal,
                    Latitud = location.GetProperty("lat").GetDecimal(),
                    Longitud = location.GetProperty("lng").GetDecimal()
                };

                // Parse address components to get city, state, country
                foreach (var component in result.GetProperty("address_components").EnumerateArray())
                {
                    var types = component.GetProperty("types").EnumerateArray()
                        .Select(t => t.GetString()).ToList();
                    var longName = component.GetProperty("long_name").GetString();

                    if (types.Contains("country")) dto.PaisNombre = longName;
                    if (types.Contains("administrative_area_level_1")) dto.EstadoNombre = longName;
                    if (types.Contains("locality")) dto.CiudadNombre = longName;
                }

                return ApiResponse<DireccionDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DireccionDto>(9000);
            }
        }
    }
}
