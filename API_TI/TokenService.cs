using API_TI.Data;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.MarcaDTOs;
using API_TI.Models.DTOs.UsuarioDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace API_TI.Services.Implementations
{
    public class TokenService : BaseService, ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly TiPcComponentsContext _context;
        private readonly IConfiguration _config;

        private readonly int _refreshDays = 30;
        private readonly byte[] _hmacKey;

        public TokenService(
            ILogger<TokenService> logger,
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _logger = logger;
            _context = context;
            _config = config;

            // Load HMAC key from configuration (store in secure config, not appsettings.json)
            var hmacKeyString = _config["Security:TokenHmacKey"];
            if (string.IsNullOrWhiteSpace(hmacKeyString) || hmacKeyString.Length < 32)
                throw new Exception("TokenHmacKey must be at least 32 characters");

            _hmacKey = Encoding.UTF8.GetBytes(hmacKeyString);
        }
        

        public async Task<string> CreateAndStoreRefreshTokenAsync(Usuario user, HttpContext? ctx = null)
        {

            var plain = GenerateRefreshTokenPlain();
            var hash = HashToken(plain);

            var ip = ctx?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = ctx?.Request?.Headers["User-Agent"].ToString();

            // Generate device fingerprint (combine IP + UserAgent + Accept headers)
            var deviceFingerprint = GenerateDeviceFingerprint(ctx);
            var now = DateTime.UtcNow;

            var userToken = new UsuarioToken
            {
                UsuarioId = user.IdUsuario,
                Tipo = "API_AUTH",
                TokenHash = hash,
                CreatedByIp = ip,
                UserAgent = userAgent,
                DeviceFingerprint = deviceFingerprint,
                FechaCreacion = now,
                FechaExpiracion = now.AddDays(_refreshDays),
                Usado = false,
                Revoked = false
            };

            try
            {
                _context.UsuarioTokens.Add(userToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                await LogTechnicalErrorAsync(9000, ex);
                return null;
            }

            await AuditAsync("Auth.RefreshToken.Created", new
            {
                UserId = user.IdUsuario,
                DeviceFingerprint = deviceFingerprint,
                Ip = ip
            });
            return plain; // return plain token to send to client (store only hash in DB)
        }

        public async Task<UsuarioToken?> ValidateRefreshTokenAsync(int userId, string refreshTokenPlain)
        {
            var hash = HashToken(refreshTokenPlain);

            var token = await _context.UsuarioTokens
                .Include(x => x.Usuario)
                .ThenInclude(x => x.Rol)
                .Where(x => x.UsuarioId == userId && x.TokenHash == hash && !x.Revoked && !x.Usado)
                .OrderByDescending(t => t.FechaCreacion)
                .FirstOrDefaultAsync();


            if (token == null) return null;

            if (token.Usado || token.Revoked)
            {
                await AuditAsync("Auth.RefreshToken.ReuseDetected", new
                {
                    UserId = userId,
                    TokenId = token.IdToken,
                    ReplacedBy = token.ReplacedByTokenHash
                });

                // Revoke entire token chain
                await RevokeTokenChainAsync(token);
                return null;
            }

            if (token.FechaExpiracion < DateTime.UtcNow) return null;
           
            return token;
        }

        public async Task<UsuarioToken?> ValidateRefreshTokenByHashAsync(string refreshTokenPlain)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPlain)) 
                return null;

            var hash = HashToken(refreshTokenPlain);

            var token = await _context.UsuarioTokens
                .Include(x => x.Usuario)
                .ThenInclude(x => x.Rol)
                .Where(x => x.TokenHash == hash && !x.Revoked && !x.Usado)
                .OrderByDescending(x => x.FechaCreacion)
                .FirstOrDefaultAsync();

            if (token == null) return null;
            if (token.FechaExpiracion < DateTime.UtcNow) return null;
            
            return token;
        }

        // Rotate: mark old as used/revoked and create new one, return new plain token
        public async Task<string> RotateRefreshTokenAsync(Usuario user, UsuarioToken existingToken, HttpContext? ctx = null)
        {
            if (user == null || existingToken == null) return null;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                existingToken.Usado = true;
                existingToken.Revoked = true;

                var newPlain = GenerateRefreshTokenPlain();
                var newHash = HashToken(newPlain);

                var newToken = new UsuarioToken
                {
                    UsuarioId = user.IdUsuario,
                    Tipo = existingToken.Tipo,
                    TokenHash = newHash,
                    CreatedByIp = ctx?.Connection?.RemoteIpAddress?.ToString(),
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddDays(_refreshDays),
                    Usado = false,
                    Revoked = false
                };

                existingToken.ReplacedByTokenHash = newHash;

                _context.UsuarioTokens.Update(existingToken);
                _context.UsuarioTokens.Add(newToken);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await AuditAsync("Auth.RefreshToken.Rotated", new
                {
                    UserId = user.IdUsuario,
                    OldTokenId = existingToken.IdToken,
                    Ip = ctx?.Connection?.RemoteIpAddress?.ToString()
                });

                return newPlain;
            } 
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return null;
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshTokenPlain, string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPlain)) return;

            var hash = HashToken(refreshTokenPlain);

            var token = await _context.UsuarioTokens
                .FirstOrDefaultAsync(x => x.TokenHash == hash);

            if (token == null) return;

            token.Revoked = true;
            token.FechaExpiracion = DateTime.UtcNow;

            // optionally store reason into token.Details if you have such a column
            await _context.SaveChangesAsync();

            await AuditAsync("Auth.RefreshToken.Revoked", new
            {
                Reason = reason ?? "Manual",
                TokenId = token.IdToken
            });
        }

        public async Task RevokeAllUserTokensAsync(int userId, string reason = "Security")
        {
            var tokens = await _context.UsuarioTokens
                .Where(t => t.UsuarioId == userId && !t.Revoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.Revoked = true;
                t.Usado = true;
                t.FechaExpiracion = DateTime.UtcNow;
                t.ReplacedByTokenHash = null;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RevokeTokenChainAsync(UsuarioToken compromisedToken)
        {
            // Revoke the compromised token and any tokens it was replaced by
            var tokensToRevoke = new List<UsuarioToken> { compromisedToken };

            // Follow the chain forward
            var currentHash = compromisedToken.ReplacedByTokenHash;
            while (!string.IsNullOrEmpty(currentHash))
            {
                var next = await _context.UsuarioTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == currentHash);
                if (next == null) break;

                tokensToRevoke.Add(next);
                currentHash = next.ReplacedByTokenHash;
            }

            foreach (var token in tokensToRevoke)
            {
                token.Revoked = true;
                token.FechaExpiracion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IList<UsuarioTokenDto>> ListActiveSessionsAsync(int userId, string? currentRefreshPlain = null)
        {
            var tokens = await _context.UsuarioTokens
                .Where(t => t.UsuarioId == userId)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            string? currentHash = null;
            if (!string.IsNullOrWhiteSpace(currentRefreshPlain))
                currentHash = HashToken(currentRefreshPlain);

            var result = tokens.Select(t => new UsuarioTokenDto
            {
                IdToken = t.IdToken,
                FechaCreacion = t.FechaCreacion,
                FechaExpiracion = t.FechaExpiracion,
                Revoked = t.Revoked,
                Usado = t.Usado,
                DeviceName = t.DeviceName,
                UserAgentShort = t.UserAgent != null && t.UserAgent.Length > 200 ? t.UserAgent.Substring(0, 200) : t.UserAgent,
                CreatedByIp = t.CreatedByIp,
                IsCurrent = currentHash != null && t.TokenHash == currentHash
            }).ToList();

            return result;
        }

        public async Task<bool> RevokeSessionAsync(int userId, int tokenId)
        {
            var token = await _context.UsuarioTokens.FirstOrDefaultAsync(t => t.IdToken == tokenId && t.UsuarioId == userId);
            
            if (token == null) return false;

            token.Revoked = true;
            token.FechaExpiracion = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return true;
        } 
        
        public string GenerateRefreshTokenPlain()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public string HashToken(string tokenPlain)
        {
            using var hmac = new HMACSHA256(_hmacKey);
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(tokenPlain));
            return Convert.ToBase64String(bytes);
        }

        private string? GenerateDeviceFingerprint(HttpContext? ctx)
        {
            if (ctx == null) return null;
            var components = new[]
            {
                ctx.Connection?.RemoteIpAddress?.ToString() ?? "",
                ctx.Request?.Headers["User-Agent"].ToString() ?? "",
                ctx.Request?.Headers["Accept-Language"].ToString() ?? "",
                ctx.Request?.Headers["Accept-Encoding"].ToString() ?? ""
            };

            var combined = string.Join("|", components);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(bytes);
        }

    }
}
