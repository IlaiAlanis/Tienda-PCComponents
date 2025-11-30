using API_TI.Attributes;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace API_TI.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitAttribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

            if (rateLimitAttribute != null)
            {
                var key = GetClientKey(context);
                var requests = GetRequestCount(key);

                if (requests >= rateLimitAttribute.MaxRequests)
                {
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = new { code = 429, message = "Demasiadas solicitudes. Intente más tarde." }
                    });
                    return;
                }

                IncrementRequestCount(key, rateLimitAttribute.WindowSeconds);
            }

            await _next(context);
        }

        private string GetClientKey(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var endpoint = context.Request.Path;
            return $"{ip}:{userId}:{endpoint}";
        }

        private int GetRequestCount(string key)
        {
            return _cache.Get<int>(key);
        }

        private void IncrementRequestCount(string key, int windowSeconds)
        {
            _semaphore.Wait();
            try
            {
                var count = _cache.Get<int>(key);
                _cache.Set(key, count + 1, TimeSpan.FromSeconds(windowSeconds));
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
