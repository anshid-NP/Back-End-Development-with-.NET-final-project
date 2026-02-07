using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagementApi.Middleware
{
    public class TokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenAuthenticationMiddleware> _logger;

        // For demo: a hardcoded valid token
        private const string ValidToken = "secret-token-123";

        public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Allow unauthenticated access to e.g. /health or /login if you want
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path == "/health" || path == "/login")
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                _logger.LogWarning("Missing Authorization header.");
                await WriteUnauthorizedAsync(context, "Missing Authorization header.");
                return;
            }

            var token = authHeader.ToString();

            // Expecting "Bearer <token>"
            if (!token.StartsWith("Bearer "))
            {
                _logger.LogWarning("Invalid Authorization scheme.");
                await WriteUnauthorizedAsync(context, "Invalid Authorization scheme.");
                return;
            }

            var rawToken = token.Substring("Bearer ".Length).Trim();

            if (rawToken != ValidToken)
            {
                _logger.LogWarning("Invalid token.");
                await WriteUnauthorizedAsync(context, "Invalid token.");
                return;
            }

            // Token is valid, continue
            await _next(context);
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = message
            });

            await context.Response.WriteAsync(json);
        }
    }
}