using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ConfGlobalDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class PreferencesService : BaseService, IPreferencesService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<PreferencesService> _logger;

        // Preference key constants
        private const string KEY_THEME = "pref_theme";
        private const string KEY_LANGUAGE = "pref_language";
        private const string KEY_CURRENCY = "pref_currency";
        private const string KEY_EMAIL_NOTIFICATIONS = "pref_email_notifications";
        private const string KEY_PROMO_NOTIFICATIONS = "pref_promo_notifications";
        private const string KEY_STOCK_NOTIFICATIONS = "pref_stock_notifications";
        private const string KEY_PUBLIC_PROFILE = "pref_public_profile";
        private const string KEY_SHARE_DATA = "pref_share_data";

        public PreferencesService(
            TiPcComponentsContext context,
            ILogger<PreferencesService> logger,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PreferencesDto>> GetPreferencesAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Getting preferences for user {UsuarioId}", usuarioId);

                // Get all preference configurations for this user
                var configs = await _context.ConfiguracionGlobals
                    .Where(c => c.UsuarioId == usuarioId && c.Clave.StartsWith("pref_"))
                    .ToListAsync();

                // Build preferences DTO with defaults
                var preferences = new PreferencesDto
                {
                    Theme = GetConfigValue(configs, KEY_THEME, "dark"),
                    Language = GetConfigValue(configs, KEY_LANGUAGE, "es"),
                    Currency = GetConfigValue(configs, KEY_CURRENCY, "USD"),
                    EmailNotifications = GetConfigBoolValue(configs, KEY_EMAIL_NOTIFICATIONS, true),
                    PromoNotifications = GetConfigBoolValue(configs, KEY_PROMO_NOTIFICATIONS, false),
                    StockNotifications = GetConfigBoolValue(configs, KEY_STOCK_NOTIFICATIONS, true),
                    PublicProfile = GetConfigBoolValue(configs, KEY_PUBLIC_PROFILE, false),
                    ShareData = GetConfigBoolValue(configs, KEY_SHARE_DATA, false)
                };

                _logger.LogInformation("Successfully retrieved preferences for user {UsuarioId}", usuarioId);

                return ApiResponse<PreferencesDto>.Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for user {UsuarioId}", usuarioId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PreferencesDto>(9000);
            }
        }

        public async Task<ApiResponse<PreferencesDto>> UpdatePreferencesAsync(
            int usuarioId,
            UpdatePreferencesRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating preferences for user {UsuarioId}", usuarioId);

                // Verify user exists
                var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == usuarioId);
                if (!userExists)
                {
                    _logger.LogWarning("User {UsuarioId} not found", usuarioId);
                    return await ReturnErrorAsync<PreferencesDto>(203); // Usuario no encontrado
                }

                // Update each provided preference
                if (request.Theme != null)
                {
                    await UpsertConfigAsync(usuarioId, KEY_THEME, request.Theme, "string");
                }

                if (request.Language != null)
                {
                    await UpsertConfigAsync(usuarioId, KEY_LANGUAGE, request.Language, "string");
                }

                if (request.Currency != null)
                {
                    await UpsertConfigAsync(usuarioId, KEY_CURRENCY, request.Currency, "string");
                }

                if (request.EmailNotifications.HasValue)
                {
                    await UpsertConfigAsync(usuarioId, KEY_EMAIL_NOTIFICATIONS,
                        request.EmailNotifications.Value.ToString().ToLower(), "boolean");
                }

                if (request.PromoNotifications.HasValue)
                {
                    await UpsertConfigAsync(usuarioId, KEY_PROMO_NOTIFICATIONS,
                        request.PromoNotifications.Value.ToString().ToLower(), "boolean");
                }

                if (request.StockNotifications.HasValue)
                {
                    await UpsertConfigAsync(usuarioId, KEY_STOCK_NOTIFICATIONS,
                        request.StockNotifications.Value.ToString().ToLower(), "boolean");
                }

                if (request.PublicProfile.HasValue)
                {
                    await UpsertConfigAsync(usuarioId, KEY_PUBLIC_PROFILE,
                        request.PublicProfile.Value.ToString().ToLower(), "boolean");
                }

                if (request.ShareData.HasValue)
                {
                    await UpsertConfigAsync(usuarioId, KEY_SHARE_DATA,
                        request.ShareData.Value.ToString().ToLower(), "boolean");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("Preferences.Updated", new
                {
                    UsuarioId = usuarioId,
                    UpdatedFields = new
                    {
                        request.Theme,
                        request.Language,
                        request.Currency,
                        request.EmailNotifications,
                        request.PromoNotifications,
                        request.StockNotifications,
                        request.PublicProfile,
                        request.ShareData
                    }
                }, usuarioId);

                _logger.LogInformation("Successfully updated preferences for user {UsuarioId}", usuarioId);

                // Return updated preferences
                return await GetPreferencesAsync(usuarioId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating preferences for user {UsuarioId}", usuarioId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PreferencesDto>(9000);
            }
        }

        public async Task<ApiResponse<PreferencesDto>> ResetPreferencesAsync(int usuarioId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Resetting preferences for user {UsuarioId}", usuarioId);

                // Delete all user preferences
                var userConfigs = await _context.ConfiguracionGlobals
                    .Where(c => c.UsuarioId == usuarioId && c.Clave.StartsWith("pref_"))
                    .ToListAsync();

                if (userConfigs.Any())
                {
                    _context.ConfiguracionGlobals.RemoveRange(userConfigs);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Audit log
                await AuditAsync("Preferences.Reset", new
                {
                    UsuarioId = usuarioId,
                    DeletedCount = userConfigs.Count
                }, usuarioId);

                _logger.LogInformation("Successfully reset preferences for user {UsuarioId}", usuarioId);

                // Return default preferences
                return await GetPreferencesAsync(usuarioId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error resetting preferences for user {UsuarioId}", usuarioId);
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PreferencesDto>(9000);
            }
        }

        #region Private Helper Methods

        private string GetConfigValue(List<ConfiguracionGlobal> configs, string key, string defaultValue)
        {
            var config = configs.FirstOrDefault(c => c.Clave == key);
            return config?.Valor ?? defaultValue;
        }

        private bool GetConfigBoolValue(List<ConfiguracionGlobal> configs, string key, bool defaultValue)
        {
            var config = configs.FirstOrDefault(c => c.Clave == key);
            if (config == null) return defaultValue;

            return config.Valor?.ToLower() == "true";
        }

        private async Task UpsertConfigAsync(int usuarioId, string clave, string valor, string tipoDato)
        {
            var config = await _context.ConfiguracionGlobals
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Clave == clave);

            if (config == null)
            {
                // Insert new config
                config = new ConfiguracionGlobal
                {
                    UsuarioId = usuarioId,
                    Clave = clave,
                    Valor = valor,
                    TipoDato = tipoDato,
                    Descripcion = GetConfigDescription(clave),
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.ConfiguracionGlobals.Add(config);
            }
            else
            {
                // Update existing config
                config.Valor = valor;
                config.FechaActualizacion = DateTime.UtcNow;
            }
        }

        private string GetConfigDescription(string clave)
        {
            return clave switch
            {
                KEY_THEME => "Tema de la interfaz (light, dark, auto)",
                KEY_LANGUAGE => "Idioma preferido (es, en, fr)",
                KEY_CURRENCY => "Moneda para mostrar precios (USD, EUR, MXN)",
                KEY_EMAIL_NOTIFICATIONS => "Notificaciones por correo electrónico",
                KEY_PROMO_NOTIFICATIONS => "Notificaciones de promociones",
                KEY_STOCK_NOTIFICATIONS => "Alertas de stock disponible",
                KEY_PUBLIC_PROFILE => "Perfil visible para otros usuarios",
                KEY_SHARE_DATA => "Compartir datos de navegación",
                _ => "Preferencia de usuario"
            };
        }

        #endregion
    }
}