using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using censudex_api.src.Models;

namespace censudex_api.src.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IHttpClientFactory httpClientFactory,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Login endpoint - Validates credentials and returns JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request", Errors = ModelState });
            }

            try
            {
                // Create HTTP client for Auth Service
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var authServiceUrl = _configuration["Services:AuthService:BaseUrl"];

                // Forward login request to Auth Service
                var response = await authClient.PostAsJsonAsync(
                    $"{authServiceUrl}/api/Auth/login",
                    request
                );

                // Handle different response status codes
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return Unauthorized(new { Message = "Invalid credentials" });
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        return StatusCode(503, new { Message = "Authentication service is unavailable" });
                    }
                    
                    return StatusCode((int)response.StatusCode, new { Message = "Login failed", Details = errorContent });
                }

                // Read and return the successful login response
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                
                if (loginResponse == null)
                {
                    return StatusCode(500, new { Message = "Invalid response from authentication service" });
                }

                _logger.LogInformation("User {Email} logged in successfully", request.Email ?? request.Username);

                return Ok(loginResponse);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Auth Service");
                return StatusCode(503, new { Message = "Authentication service is unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Logout endpoint - Invalidates the current JWT token
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Extract token from Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { Message = "No token provided" });
                }

                // Create HTTP client for Auth Service
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var authServiceUrl = _configuration["Services:AuthService:BaseUrl"];

                // Forward logout request to Auth Service with the token
                var request = new HttpRequestMessage(HttpMethod.Post, $"{authServiceUrl}/api/Auth/logout");
                request.Headers.Add("Authorization", authHeader);

                var response = await authClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { Message = "Logout failed", Details = errorContent });
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                _logger.LogInformation("User logged out successfully");

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Auth Service");
                return StatusCode(503, new { Message = "Authentication service is unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Validate token endpoint (optional - for client-side token validation)
        /// </summary>
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                // Extract token from Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(authHeader))
                {
                    return BadRequest(new { IsValid = false, Message = "No token provided" });
                }

                // Create HTTP client for Auth Service
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var authServiceUrl = _configuration["Services:AuthService:BaseUrl"];

                // Forward validation request to Auth Service
                var request = new HttpRequestMessage(HttpMethod.Get, $"{authServiceUrl}/api/Auth/validate");
                request.Headers.Add("Authorization", authHeader);

                var response = await authClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Auth Service");
                return StatusCode(503, new { IsValid = false, Message = "Authentication service is unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return StatusCode(500, new { IsValid = false, Message = "An unexpected error occurred" });
            }
        }
    }
}