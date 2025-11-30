
using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Azure.Core;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace API_TI.Services.Implementations
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly TiPcComponentsContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly HttpClient _httpClient;

        private const int ERR_INVALID_CREDENTIALS = 102; // AUTH.CREDENCIALES_INVALIDAS
        private const int ERR_USER_DUPLICATE = 201;      // USUARIO.DUPLICADO
        private const int ERR_USER_INACTIVE = 202;       // USUARIO.INACTIVO
        private const int ERR_REFRESH_INVALID = 106;     // AUTH.REFRESH_TOKEN_INVALIDO
        private const int ERR_TOKEN_REVOKED = 107;       // AUTH.TOKEN_REVOKED
        private const int ERR_INTERNAL = 9000;           // SISTEMA.ERROR_SQL / generic


        public AuthService(
            ILogger<AuthService> logger,
            TiPcComponentsContext context,
            IJwtService jwtService,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            ITokenService tokenService,
            IAuditService auditService,
            IEmailService emailService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _logger = logger;
            _context = context;
            _jwtService = jwtService;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _auditService = auditService;
            _emailService = emailService;
        }
        
        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequestAuth request)
        {
            try
            {
                // Validate catcha
                //if (!await VerifyRecaptchaAsync(request.RecaptchaToken))
                //    return await ReturnErrorAsync<AuthResponse>(110, "Verificación de seguridad fallida");

                // Validate input minimally
                if (string.IsNullOrWhiteSpace(request?.Correo) || string.IsNullOrWhiteSpace(request?.Contrasena))
                    return await ReturnErrorAsync<AuthResponse>(2, "Correo o contraseña requeridos"); // 2 = PARAMETRO_INVALIDO

                // Look up user by email (Correo)
                var user = await _context.Usuarios
                    .Include(x => x.Rol)
                    .FirstOrDefaultAsync(x => x.Correo == request.Correo);


                // Check if account is temporarily locked
                if (user != null && user.FechaBloqueoLogin.HasValue)
                {
                    var lockoutDuration = TimeSpan.FromMinutes(15); // 15 minutes lockout
                    if (DateTime.UtcNow < user.FechaBloqueoLogin.Value.Add(lockoutDuration))
                    {
                        var remainingTime = user.FechaBloqueoLogin.Value.Add(lockoutDuration) - DateTime.UtcNow;

                        _logger.LogWarning("Auth.Login.Failed - Account locked {@Meta}", new
                        {
                            UserId = user.IdUsuario,
                            Email = user.Correo,
                            RemainingMinutes = remainingTime.TotalMinutes
                        });

                        await _auditService.LogAsync("Auth.Login.AccountLocked", new
                        {
                            UserId = user.IdUsuario,
                            Email = user.Correo
                        }, user.IdUsuario);

                        return await ReturnErrorAsync<AuthResponse>(103,
                            $"Cuenta bloqueada temporalmente. Intente en {Math.Ceiling(remainingTime.TotalMinutes)} minutos.");
                    }
                    else
                    {
                        // Lockout period expired, reset
                        user.FechaBloqueoLogin = null;
                        user.IntentosFallidosLogin = 0;
                        await _context.SaveChangesAsync();
                    }
                }



                if (user == null || !VerifyPassword(request.Contrasena, user.ContrasenaHash))
                {
                    // Increment failed login attempts
                    if (user != null)
                    {
                        user.IntentosFallidosLogin++;

                        // Lock account after 5 failed attempts
                        if (user.IntentosFallidosLogin >= 5)
                        {
                            user.FechaBloqueoLogin = DateTime.UtcNow;
                            await _context.SaveChangesAsync();

                            _logger.LogWarning("Auth.Login.AccountLocked - Too many failed attempts {@Meta}", new
                            {
                                UserId = user.IdUsuario,
                                Email = user.Correo,
                                Attempts = user.IntentosFallidosLogin
                            });

                            await _auditService.LogAsync("Auth.Login.AccountLocked", new
                            {
                                UserId = user.IdUsuario,
                                Attempts = user.IntentosFallidosLogin
                            }, user.IdUsuario);

                            return await ReturnErrorAsync<AuthResponse>(103,
                                "Demasiados intentos fallidos. Cuenta bloqueada por 15 minutos.");
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Log attempt
                    _logger.LogWarning("Auth.Login.Failed {@Meta}", new
                    {
                        Email = request.Correo,
                        Attempts = user?.IntentosFallidosLogin ?? 0,
                        Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    });

                    await _auditService.LogAsync("Auth.Login.Failed", new
                    {
                        Email = request.Correo
                    });

                    return await ReturnErrorAsync<AuthResponse>(ERR_INVALID_CREDENTIALS);
                }

                if (!user.EstaActivo)
                {
                    _logger.LogWarning("Auth.Login.Failed - Inactive user {@Meta}", new
                    {
                        UserId = user.IdUsuario,
                        Email = user.Correo,
                        Endpoint = _httpContextAccessor.HttpContext?.Request?.Path
                    });

                    await _auditService.LogAsync("Auth.Login.Failed", new
                    {
                        UserId = user.IdUsuario,
                        Reason = "Inactive"
                    });

                    return await ReturnErrorAsync<AuthResponse>(ERR_USER_INACTIVE);
                }

                // Successful login - reset failed attempts
                user.IntentosFallidosLogin = 0;
                user.FechaBloqueoLogin = null;
                user.UltimoLoginUsuario = DateTime.UtcNow;
                user.FechaActualizacion = DateTime.UtcNow;

                // Generate JWT and refresh token
                // Ensure role loaded
                if (user.Rol == null)
                {
                    await _context.Entry(user).Reference(x => x.Rol).LoadAsync();
                }

                var jwt = _jwtService.GenerateToken(user, user.Rol?.NombreRol ?? "User");
                var refreshPlain = await _tokenService.CreateAndStoreRefreshTokenAsync(user, _httpContextAccessor.HttpContext);
            
                await _context.SaveChangesAsync();

                var dtoUser = Mapper.ToUsuarioDto(user);

                // Log success and audit
                _logger.LogInformation("Auth.Login.Success {@Meta}", new
                {
                    UserId = user.IdUsuario,
                    Email = user.Correo,
                    Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Endpoint = _httpContextAccessor.HttpContext?.Request?.Path,
                    CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                });

                await _auditService.LogAsync("Auth.Login.Success", new
                {
                    UserId = user.IdUsuario,
                    Email = user.Correo,
                }, user.IdUsuario);

                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    RefreshToken = refreshPlain,
                    ExpiraEn = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = dtoUser
                };

                return ApiResponse<AuthResponse>.Ok(authResponse, "Login exitoso");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Auth.Login.Exception {@Meta}", new
                {
                    Email = request?.Correo,
                    CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                });

                // Let middleware record tech details (do not send to client)
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);

                return await ReturnErrorAsync<AuthResponse>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequestAuth request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Correo) || string.IsNullOrWhiteSpace(request?.Contrasena))
                    return await ReturnErrorAsync<AuthResponse>(2, "Correo y contraseña son requeridos");

                var validation = ValidatePasswordStrength(request.Contrasena);
                if (validation != null)
                    return await ReturnErrorAsync<AuthResponse>(validation.Error.Code, validation.Error.Message);

                var exists = await _context.Usuarios.AsNoTracking().AnyAsync(u => u.Correo == request.Correo);
                if (exists)
                    return await ReturnErrorAsync<AuthResponse>(ERR_USER_DUPLICATE);

                // Create password hash & salt
                var contrasenaHash = CreatePasswordHash(request.Contrasena);


                var newUser = new Usuario
                {
                    Nombre = request.Nombre,
                    ApellidoPaterno = request.ApellidoPaterno,
                    ApellidoMaterno = request.ApellidoMaterno,
                    NombreUsuario = request.NombreUsuario,
                    Correo = request.Correo,
                    FechaNacimiento = request.FechaNacimiento,
                    ContrasenaHash = contrasenaHash,
                    RolId = request.RolId == 0 ? 2 : request.RolId, // default to user role id 2
                    AutenticacionProveedorId = 1, // local
                    EstaActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _context.Usuarios.AddAsync(newUser);
                await _context.SaveChangesAsync();

                var jwt = _jwtService.GenerateToken(newUser, "User");
                var refreshPlain = await _tokenService.CreateAndStoreRefreshTokenAsync(newUser, _httpContextAccessor.HttpContext);

                _logger.LogInformation("Auth.Register.Success {@Meta}", new
                {
                    UserId = newUser.IdUsuario,
                    Email = newUser.Correo,
                    CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                });

                await _auditService.LogAsync("Auth.Register", new
                {
                    UserId = newUser.IdUsuario,
                    Email = newUser.Correo
                }, newUser.IdUsuario);

                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    RefreshToken = refreshPlain,
                    ExpiraEn = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = Mapper.ToUsuarioDto(newUser)
                };

                return ApiResponse<AuthResponse>.Ok(authResponse, "Registro exitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth.Register.Exception {@Meta}", new { Email = request?.Correo });
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<AuthResponse>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<AuthResponse>> LoginWithGoogleAsync(GoogleAuthRequestAuth request)
        {
            try
            {
                var expectedClientId = _config["Google:ClientId"];
                if (string.IsNullOrWhiteSpace(expectedClientId))
                {
                    _logger.LogError("Google ClientId not configured");
                    return await ReturnErrorAsync<AuthResponse>(9004); // ERROR_CONFIGURACION
                }

                // Verify Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { expectedClientId }
                });

                if (payload == null)
                {
                    _logger.LogWarning("Auth.Google.Login - invalid token");
                    return await ReturnErrorAsync<AuthResponse>(109); // OAUTH_ERROR or similar
                }
               
                var user = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Correo == payload.Email);

                // Check/create OAuth mapping
               

                if (user == null)
                {
                    user = new Usuario
                    {
                        NombreUsuario = payload.Name ?? payload.Email,
                        Correo = payload.Email,
                        CorreoVerificado = payload.EmailVerified,
                        AutenticacionProveedorId = 2, // google
                        EstaActivo = true,
                        FechaCreacion = DateTime.UtcNow,
                        RolId = 2, // default user
                        UltimoLoginUsuario = DateTime.UtcNow
                    };

                    await _context.Usuarios.AddAsync(user);
                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync("Auth.Google.Register", new
                    {
                        Email = user.Correo
                    }, user.IdUsuario);
                }
                else
                {
                    // Update last login for existing users
                    user.UltimoLoginUsuario = DateTime.UtcNow;
                    user.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                if (user != null)
                {
                    var oauthMapping = await _context.UsuarioOauthProveedors
                        .FirstOrDefaultAsync(o => o.UsuarioId == user.IdUsuario &&
                                                  o.AutenticacionProveedorId == 2);

                    if (oauthMapping == null)
                    {
                        oauthMapping = new UsuarioOauthProveedor
                        {
                            UsuarioId = user.IdUsuario,
                            AutenticacionProveedorId = 2, // Google
                            IdUsuarioExterno = payload.Subject, // Google's user ID
                            FechaCreacion = DateTime.UtcNow
                        };
                        _context.UsuarioOauthProveedors.Add(oauthMapping);
                        await _context.SaveChangesAsync();
                    }
                }

                if (user.Rol == null) await _context.Entry(user).Reference(u => u.Rol).LoadAsync();

                var jwt = _jwtService.GenerateToken(user, user.Rol.NombreRol);
                var refreshPlain = await _tokenService.CreateAndStoreRefreshTokenAsync(user, _httpContextAccessor.HttpContext);

                _logger.LogInformation("Auth.Google.Success {@Meta}", new { UserId = user.IdUsuario, Email = user.Correo });
                await _auditService.LogAsync("Auth.Google.Success", new { UserId = user.IdUsuario, Email = user.Correo }, user.IdUsuario);

                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    RefreshToken = refreshPlain,
                    ExpiraEn = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = Mapper.ToUsuarioDto(user)
                };

                return ApiResponse<AuthResponse>.Ok(authResponse, "Login con Google exitoso");
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Auth.Google.InvalidToken");
                await _auditService.LogAsync("Auth.Google.Failed", new { Error = ex.Message });
                return await ReturnErrorAsync<AuthResponse>(109);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth.Google.Exception");
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<AuthResponse>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshTokenPlain)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshTokenPlain))
                    return await ReturnErrorAsync<AuthResponse>(ERR_REFRESH_INVALID);

                // Find token record by hash (includes Usuario navigation)
                var tokenRecord = await _tokenService.ValidateRefreshTokenByHashAsync(refreshTokenPlain);

                if (tokenRecord == null)
                {
                    _logger.LogWarning("Auth.Refresh.Failed - invalid token {@Meta}", new
                    {
                        Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        Endpoint = _httpContextAccessor.HttpContext?.Request?.Path,
                        CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                    });

                    await _auditService.LogAsync("Auth.Refresh.Failed", new { Reason = "Invalid refresh token" });

                    return await ReturnErrorAsync<AuthResponse>(ERR_REFRESH_INVALID);
                }

                // Reuse detection: token already used or revoked -> revoke all tokens & alert
                if (tokenRecord.Usado || tokenRecord.Revoked)
                {
                    _logger.LogError("Auth.Refresh.ReuseDetected {@Meta}", new
                    {
                        UserId = tokenRecord.UsuarioId,
                        TokenId = tokenRecord.IdToken,
                        Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                    });

                    // Revoke all tokens for this user (security response)
                    await _tokenService.RevokeAllUserTokensAsync(tokenRecord.UsuarioId, "Reuse detected");

                    await _auditService.LogAsync("Auth.Refresh.ReuseDetected", new
                    {
                        UserId = tokenRecord.UsuarioId,
                        TokenId = tokenRecord.IdToken
                    }, tokenRecord.UsuarioId);

                    // Return token revoked / compromised
                    return await ReturnErrorAsync<AuthResponse>(ERR_TOKEN_REVOKED);
                }

                // Expiration check
                if (tokenRecord.FechaExpiracion < DateTime.UtcNow)
                {
                    _logger.LogWarning("Auth.Refresh.Failed - expired {@Meta}", new { TokenId = tokenRecord.IdToken });
                    await _auditService.LogAsync("Auth.Refresh.Failed", new { UserId = tokenRecord.UsuarioId, Reason = "Expired" }, tokenRecord.UsuarioId);
                    return await ReturnErrorAsync<AuthResponse>(ERR_REFRESH_INVALID);
                }

                var user = tokenRecord.Usuario;
                if (user == null)
                {
                    _logger.LogWarning("Auth.Refresh.Failed - user missing {@Meta}", new { TokenId = tokenRecord.IdToken });
                    return await ReturnErrorAsync<AuthResponse>(ERR_REFRESH_INVALID);
                }

                if (!user.EstaActivo)
                {
                    await _auditService.LogAsync("Auth.Refresh.Failed", new { UserId = user.IdUsuario, Reason = "Inactive" }, user.IdUsuario);
                    return await ReturnErrorAsync<AuthResponse>(ERR_USER_INACTIVE);
                }

                if (user.Rol == null) await _context.Entry(user).Reference(u => u.Rol).LoadAsync();

                // Rotate tokens
                var newRefreshPlain = await _tokenService.RotateRefreshTokenAsync(user, tokenRecord, _httpContextAccessor.HttpContext);

                var jwt = _jwtService.GenerateToken(user, user.Rol.NombreRol);

                await _auditService.LogAsync("Auth.Refresh.Success", new { UserId = user.IdUsuario, OldTokenId = tokenRecord.IdToken }, user.IdUsuario);

                _logger.LogInformation("Auth.Refresh.Success {@Meta}", new { UserId = user.IdUsuario, OldTokenId = tokenRecord.IdToken });

                var resp = new AuthResponse
                {
                    Token = jwt,
                    RefreshToken = newRefreshPlain,
                    ExpiraEn = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = Mapper.ToUsuarioDto(user)
                };

                return ApiResponse<AuthResponse>.Ok(resp, "Token refrescado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth.Refresh.Exception {@Meta}", new { CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier });
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<AuthResponse>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<object>> LogoutAsync(string refreshTokenPlain)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshTokenPlain))
                    return await ReturnErrorAsync<object>(ERR_REFRESH_INVALID);

                // Revoke the specific refresh token
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenPlain, "User logout");

                // Audit + log
                _logger.LogInformation("Auth.Logout {@Meta}", new
                {
                    Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                });

                await _auditService.LogAsync("Auth.Logout", new { Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() });

                return ApiResponse<object>.Ok(null, "Sesión cerrada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth.Logout.Exception {@Meta}");
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<object>(ERR_INTERNAL);
            }
            
        }

        public async Task<ApiResponse<object>> SendEmailVerificationAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    return await ReturnErrorAsync<object>(200); // USER_NOT_FOUND

                if (user.CorreoVerificado)
                    return await ReturnErrorAsync<object>(5, "El correo ya está verificado");

                // Generate 6-digit code
                var code = Random.Shared.Next(100000, 999999).ToString();

                var verification = new CorreoVerificacion
                {
                    UsuarioId = userId,
                    Codigo = code,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddHours(1),
                    Usado = false
                };

                _context.CorreoVerificacions.Add(verification);
                await _context.SaveChangesAsync();

                // Send email (implement IEmailService)
                await _emailService.SendEmailVerificationAsync(user.Correo, code);

                await _auditService.LogAsync("Auth.EmailVerification.Sent", new
                {
                    UserId = userId,
                    Email = user.Correo
                }, userId);

                return ApiResponse<object>.Ok(null, "Código de verificación enviado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<object>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                var verification = await _context.CorreoVerificacions
                    .FirstOrDefaultAsync(v => v.Correo == request.Email && v.Codigo == request.Code);

                if (verification == null)
                    return await ReturnErrorAsync<object>(206, "Código inválido");

                if (verification.FechaExpiracion < DateTime.UtcNow)
                    return await ReturnErrorAsync<object>(207, "Código expirado");

                if (verification.Usado)
                    return await ReturnErrorAsync<object>(208, "Código ya usado");

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Email);
                if (usuario == null)
                    return await ReturnErrorAsync<object>(200);

                usuario.CorreoVerificado = true;
                verification.Usado = true;
                verification.FechaUso = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await AuditAsync("Auth.EmailVerified", new { Email = request.Email });

                return ApiResponse<object>.Ok(null, "Email verificado exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(x => x.Correo == email);

                // SECURITY: Always return success even if user doesn't exist
                // This prevents email enumeration attacks
                if (user == null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return ApiResponse<object>.Ok(null, "Si el correo existe, recibirás un código");
                }

                if (!user.EstaActivo)
                {
                    _logger.LogWarning("Password reset requested for inactive user: {UserId}", user.IdUsuario);
                    return ApiResponse<object>.Ok(null, "Si el correo existe, recibirás un código");
                }

                // Generate 6-digit code
                var code = Random.Shared.Next(100000, 999999).ToString();

                var reset = new ContrasenaReseteo
                {
                    UsuarioId = user.IdUsuario,
                    Codigo = code,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddMinutes(15), // Short expiration for security
                    Usado = false
                };

                _context.ContrasenaReseteos.Add(reset);
                await _context.SaveChangesAsync();

                // Send email
                await _emailService.SendPasswordResetAsync(user.Correo, code);

                await _auditService.LogAsync("Auth.PasswordReset.Requested", new
                {
                    UserId = user.IdUsuario,
                    Email = user.Correo
                }, user.IdUsuario);

                return ApiResponse<object>.Ok(null, "Si el correo existe, recibirás un código");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(ERR_INTERNAL, ex);
                return await ReturnErrorAsync<object>(ERR_INTERNAL);
            }
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestAuth request)
        {
            try
            {
                var reset = await _context.ContrasenaReseteos
                    .FirstOrDefaultAsync(r => r.Correo == request.Email && r.Codigo == request.Code);

                if (reset == null)
                    return await ReturnErrorAsync<object>(209, "Código inválido");

                if (reset.FechaExpiracion < DateTime.UtcNow)
                    return await ReturnErrorAsync<object>(210, "Código expirado");

                if (reset.Usado)
                    return await ReturnErrorAsync<object>(211, "Código ya usado");

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Email);
                if (usuario == null)
                    return await ReturnErrorAsync<object>(200);

                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                reset.Usado = true;
                reset.FechaUso = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await AuditAsync("Auth.PasswordReset", new { Email = request.Email });

                return ApiResponse<object>.Ok(null, "Contraseña actualizada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }
        
        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("reCAPTCHA token missing - allowing request");
                return true; // Allow but log
            }

            var secret = _config["Google:RecaptchaSecret"];

            try
            {
                var response = await _httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                    null);
                var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

                // v3 score check (0.0 = bot, 1.0 = human)
                if (result.success && result.score >= 0.3)
                    return true;

                _logger.LogWarning("reCAPTCHA failed - Score: {Score}", result.score);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "reCAPTCHA verification error");
                return true; // Allow on service failure
            }
        }

        private string BuildFullName(string nombre, string? apellidoPaterno, string? apellidoMaterno)
        {
            // Start with the main name field
            var fullName = nombre?.Trim() ?? string.Empty;

            // If apellidos are provided separately, append them
            if (!string.IsNullOrWhiteSpace(apellidoPaterno))
            {
                fullName += " " + apellidoPaterno.Trim();
            }

            if (!string.IsNullOrWhiteSpace(apellidoMaterno))
            {
                fullName += " " + apellidoMaterno.Trim();
            }

            return fullName.Trim();
        }

        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private ApiResponse<object> ValidatePasswordStrength(string password)
        {
            if (password.Length < 8)
                return ApiResponse<object>.Fail(3, "Contraseña debe tener al menos 8 caracteres", 2);

            if (password.Length > 128)
                return ApiResponse<object>.Fail(3, "Contraseña muy larga", 2);

            if (!password.Any(char.IsUpper))
                return ApiResponse<object>.Fail(3, "Debe contener al menos una mayúscula", 2);

            if (!password.Any(char.IsLower))
                return ApiResponse<object>.Fail(3, "Debe contener al menos una minúscula", 2);

            if (!password.Any(char.IsDigit))
                return ApiResponse<object>.Fail(3, "Debe contener al menos un número", 2);

            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
                return ApiResponse<object>.Fail(3, "Debe contener un carácter especial", 2);

            return null; // Valid
        }
    }
}
