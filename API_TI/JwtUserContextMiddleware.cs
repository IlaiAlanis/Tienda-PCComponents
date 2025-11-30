using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace API_TI.Middlewares
{
    public class JwtUserContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public JwtUserContextMiddleware(
            RequestDelegate next,
            IMemoryCache cache
        )
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context, TiPcComponentsContext dbContext) 
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("id")?.Value
                             ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    var cacheKey = $"user:{userId}";

                    if (!_cache.TryGetValue(cacheKey, out Usuario usuario)) 
                    {
                        // Cache miss - query database
                        usuario = await dbContext.Usuarios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.IdUsuario == userId);

                        if (usuario == null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = ApiResponse<object>.Fail(104, "Token inválido - usuario no existe", 3);
                            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                            return;
                        }

                        // Cache user for 5 minutes
                        _cache.Set(cacheKey, usuario, _cacheExpiration);
                    }
                   

                    if (!usuario.EstaActivo)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<object>.Fail(202, "El usuario está inactivo", 2);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return;
                    }

                    context.Items["UserId"] = usuario.IdUsuario;
                    context.Items["User"] = usuario;
                }
            }

            await _next(context);

        }
    }
}
