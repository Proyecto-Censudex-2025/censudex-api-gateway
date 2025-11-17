using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using censudex_api.src.Services;
using Grpc.Core;
using ClientsService.Grcp;
using censudex_api.src.Dto;

namespace censudex_api.src.Controllers
{
    [ApiController]
    [Route("clients")]
    public class ClientsController : ControllerBase
    {
        private readonly ClientsGrpcAdapter _clientsAdapter;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ClientsGrpcAdapter clientsAdapter, ILogger<ClientsController> logger)
        {
            _clientsAdapter = clientsAdapter;
            _logger = logger;
        }

        /// <summary>
        /// Get all clients or filtered clients (Admin only)
        /// Query params: name, email, isActive, username
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClients(
            [FromQuery] string? name = null,
            [FromQuery] string? email = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? username = null)
        {
            try
            {
                var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
                    ? HttpContext.Request.Headers["Authorization"].ToString()
                    : string.Empty;

                // If no filters provided, get all clients
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email) && 
                    !isActive.HasValue && string.IsNullOrEmpty(username))
                {
                    var allResp = await _clientsAdapter.GetAllClientsAsync(authHeader);
                    return Ok(allResp.Clients);
                }

                // Otherwise, get filtered clients
                var req = new GetClientsFilteredRequest
                {
                    Name = name ?? string.Empty,
                    Email = email ?? string.Empty,
                    Username = username ?? string.Empty
                };

                if (isActive.HasValue)
                {
                    req.IsActive = isActive.Value;
                }

                var resp = await _clientsAdapter.GetClientsFilteredAsync(req, authHeader);
                return Ok(resp.Clients);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error getting clients");
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return StatusCode(500, new { message = "Error obteniendo clientes" });
            }
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClientById(string id)
        {
            try
            {
                var resp = await _clientsAdapter.GetClientAsync(id);
                
                if (!resp.Found)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                return Ok(resp.Client);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error getting client {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {id}", id);
                return StatusCode(500, new { message = "Error obteniendo cliente" });
            }
        }

        /// <summary>
        /// Register a new client
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterClient([FromBody] RegisterClientDto dto)
        {
            try
            {
                var req = new RegisterClientRequest
                {
                    Name = dto.Name,
                    Surename = dto.Surename,
                    Email = dto.Email,
                    Username = dto.Username,
                    Birthdate = dto.Birthdate,
                    Address = dto.Address,
                    TelephoneNumber = dto.TelephoneNumber,
                    Password = dto.Password
                };

                var resp = await _clientsAdapter.RegisterClientAsync(req);
                
                if (!resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }

                return CreatedAtAction(nameof(GetClientById), new { id = resp.Client.Id }, resp.Client);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error registering client");
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering client");
                return StatusCode(500, new { message = "Error registrando cliente" });
            }
        }

        /// <summary>
        /// Update client information (authenticated user)
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateClient(string id, [FromBody] UpdateClientDto dto)
        {
            try
            {
                var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
                    ? HttpContext.Request.Headers["Authorization"].ToString()
                    : string.Empty;

                var req = new UpdateClientRequest
                {
                    Name = dto.Name ?? string.Empty,
                    Surename = dto.Surename ?? string.Empty,
                    Email = dto.Email ?? string.Empty,
                    Username = dto.Username ?? string.Empty,
                    Birthdate = dto.Birthdate ?? string.Empty,
                    Address = dto.Address ?? string.Empty,
                    TelephoneNumber = dto.TelephoneNumber ?? string.Empty,
                    Password = dto.Password ?? string.Empty,
                    Token = authHeader
                };

                var resp = await _clientsAdapter.UpdateClientAsync(req, authHeader);
                
                if (!resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }

                return Ok(resp.Client);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error updating client {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {id}", id);
                return StatusCode(500, new { message = "Error actualizando cliente" });
            }
        }

        /// <summary>
        /// Enable/Disable client (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnableDisableClient(string id)
        {
            try
            {
                var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
                    ? HttpContext.Request.Headers["Authorization"].ToString()
                    : string.Empty;

                var resp = await _clientsAdapter.EnableDisableClientAsync(id, authHeader);
                
                if (!resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }

                return Ok(new { message = resp.Message, success = resp.Success });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error toggling client status {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling client status {id}", id);
                return StatusCode(500, new { message = "Error cambiando estado del cliente" });
            }
        }
    }

}