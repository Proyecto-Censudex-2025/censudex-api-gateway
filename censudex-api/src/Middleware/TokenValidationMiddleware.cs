using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using censudex_api.src.Models;
using censudex_api.src.Dto;

namespace censudex_api.src.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        // Endpoints that don't require token validation
        private static readonly string[] PublicEndpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/logout",
            "/api/auth/validate",
            "/health",
            "/swagger",
            "/openapi"
        };

        public TokenValidationMiddleware(
            RequestDelegate next,
            ILogger<TokenValidationMiddleware> logger,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method.ToUpper();

            // Check if this is a public endpoint
            if (IsPublicEndpoint(path, method))
            {
                await _next(context);
                return;
            }

            // Extract token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                // No token provided, let the authentication middleware handle it
                await _next(context);
                return;
            }

            var token = authHeader.Substring(7); // Remove "Bearer "

            // Check cache first
            var cacheKey = $"token_valid_{token}";
            
            if (_cache.TryGetValue(cacheKey, out bool isValidCached))
            {
                if (!isValidCached)
                {
                    _logger.LogWarning("Token from cache is invalid/revoked");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked" });
                    return;
                }
                
                // Token is valid in cache, continue
                await _next(context);
                return;
            }

            // Validate with Auth Service
            try
            {
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/validate");
                request.Headers.Add("Authorization", authHeader);

                var response = await authClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Token validation failed with status: {StatusCode}", response.StatusCode);
                    
                    // Cache the invalid token for a short time to reduce Auth Service calls
                    _cache.Set(cacheKey, false, TimeSpan.FromMinutes(1));
                    
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "Invalid or revoked token" });
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();

                if (result == null || !result.IsValid)
                {
                    _logger.LogWarning("Token validation returned invalid: {Message}", result?.Message);
                    
                    // Cache the invalid token
                    _cache.Set(cacheKey, false, TimeSpan.FromMinutes(1));
                    
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = result?.Message ?? "Invalid token" });
                    return;
                }

                // Cache valid token for a short time (10 seconds for better security)
                _cache.Set(cacheKey, true, TimeSpan.FromSeconds(10));
                
                _logger.LogDebug("Token validated successfully");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Auth Service");
                // Don't block the request if Auth Service is down, let JWT validation handle it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
            }

            // Continue to the next middleware
            await _next(context);
        }

        private bool IsPublicEndpoint(string path, string method)
        {
            // Allow POST to /api/clients (registration)
            if (path.StartsWith("/api/clients") && method == "POST")
            {
                return true;
            }

            // Allow GET to /api/clients/{id} (get by id)
            if (path.StartsWith("/api/clients/") && method == "GET")
            {
                return true;
            }

            // Check other public endpoints
            foreach (var endpoint in PublicEndpoints)
            {
                if (path.StartsWith(endpoint.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}