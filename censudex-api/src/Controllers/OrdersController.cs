using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using censudex_api.src.Services;
using OrdersService.Grpc;
using Grpc.Core;
using System.Linq;
using System.Security.Claims;
using censudex_api.src.Dto;

/// <summary>
/// Controlador de órdenes.
/// </summary>
namespace censudex_api.src.Controllers
{
    /// <summary>
    /// Controlador para la gestión de órdenes.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        /// <summary>
        /// Adaptador gRPC para órdenes.
        /// </summary>
        private readonly OrdersGrpcAdapter _ordersAdapter;
        /// <summary>
        /// Logger para el controlador de órdenes.
        /// </summary>
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Constructor del controlador de órdenes.
        /// </summary>
        /// <param name="ordersAdapter">Adaptador gRPC para órdenes.</param>
        /// <param name="logger">Logger para el controlador de órdenes.</param>
        public OrdersController(OrdersGrpcAdapter ordersAdapter, ILogger<OrdersController> logger)
        {
            _ordersAdapter = ordersAdapter;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la metadata del usuario desde los claims y headers.
        /// </summary>
        /// <returns>Metadata con información del usuario.</returns>
        private Metadata GetUserMetadata()
        {
            // SOLO PARA PROBAR
            var meta = new Metadata();
            var userId = User?.Claims.FirstOrDefault(c => 
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var userRole = User?.Claims.FirstOrDefault(c => 
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var userEmail = User?.Claims.FirstOrDefault(c => 
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            //Cambio de rol "User" a "client" para el servicio de órdenes
            string nestedUserRole = null;
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                if (userRole == "User")
                {
                    nestedUserRole = "client";
                }
                else if (userRole == "Admin")
                {
                    nestedUserRole = "admin";
                }
                else
                {
                    nestedUserRole = userRole;
                }
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                meta.Add("x-user-id", userId);
            }

            if (!string.IsNullOrWhiteSpace(nestedUserRole))
            {
                meta.Add("x-user-role", nestedUserRole);
            }

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                meta.Add("x-user-email", userEmail);
            }

            var authHeader = HttpContext?.Request?.Headers.ContainsKey("Authorization") == true
                ? HttpContext.Request.Headers["Authorization"].ToString()
                : string.Empty;
            
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                meta.Add("authorization", authHeader);
            }

            return meta;
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="dto">Datos para crear la orden.</param>
        /// <returns>Resultado de la creación de la orden.</returns>
        /// <response code="201">Orden creada exitosamente.</response>
        /// <response code="400">Solicitud inválida.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <response code="GrpcException">Error gRPC específico.</response>
        [HttpPost]
        [Authorize(Policy = "ClientOrAbove")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderHttpDto dto) 
        {
            try
            {
                var meta = GetUserMetadata();

                var grpcRequest = new OrdersService.Grpc.CreateOrderRequest
                {
                    ClientId = dto.ClientId,
                    ClientEmail = dto.ClientEmail,
                    ClientName = dto.ClientName,
                    ShippingAddress = dto.ShippingAddress
                };

                
                foreach (var itemDto in dto.Items)
                {
                    grpcRequest.Items.Add(new OrdersService.Grpc.CreateOrderItemRequest
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity
                    });
                }
                
                var resp = await _ordersAdapter.CreateOrderAsync(grpcRequest, meta); 
                return CreatedAtAction(nameof(GetOrderById), new { id = resp.Id }, resp);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error gRPC al crear pedido");
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido");
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        /// <summary>
        /// Obtiene una lista de órdenes según los parámetros de consulta.
        /// </summary>
        /// <param name="dto">Parámetros para filtrar y paginar las órdenes.</param>
        /// <returns>Lista de órdenes que cumplen con los criterios.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders([FromQuery] QueryOrderRequest dto)
        {
            try
            {
                var meta = GetUserMetadata();
                var resp = await _ordersAdapter.FindAllOrdersAsync(dto, meta);
                return Ok(resp.Orders);
            }
            catch (RpcException ex)
            {
                 _logger.LogError(ex, "Error gRPC al listar pedidos");
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando pedidos");
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        /// <summary>
        /// Obtiene una orden por su ID.
        /// </summary>
        /// <param name="id">ID de la orden.</param>
        /// <returns>Orden correspondiente al ID proporcionado.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var meta = GetUserMetadata();
                var req = new FindOneOrderRequest { Id = id };
                var resp = await _ordersAdapter.FindOneOrderAsync(req, meta);
                return Ok(resp);
            }
            catch (RpcException ex)
            {
                 _logger.LogError(ex, "Error gRPC al buscar pedido {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando pedido {id}", id);
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        /// <summary>
        /// Actualiza el estado de una orden específica.
        /// </summary>
        /// <param name="id">ID de la orden.</param>
        /// <param name="dto">Datos para actualizar el estado de la orden.</param>
        /// <returns>Resultado de la actualización del estado.</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusRequest dto)
        {
            try
            {
                var meta = GetUserMetadata();
                dto.Id = id; // Asegura que el ID de la ruta se use
                var resp = await _ordersAdapter.UpdateOrderStatusAsync(dto, meta);
                return Ok(resp);
            }
            catch (RpcException ex)
            {
                 _logger.LogError(ex, "Error gRPC al actualizar estado {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado {id}", id);
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        /// <summary>
        /// Cancela una orden específica.
        /// </summary>
        /// <param name="id">ID de la orden.</param>
        /// <param name="dto">Datos para cancelar la orden.</param>
        /// <returns>Resultado de la cancelación.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest dto)
        {
            try
            {
                var meta = GetUserMetadata();
                dto.Id = id; // Asegura que el ID de la ruta se use
                var resp = await _ordersAdapter.CancelOrderAsync(dto, meta);
                return Ok(resp);
            }
            catch (RpcException ex)
            {
                 _logger.LogError(ex, "Error gRPC al cancelar pedido {id}", id);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando pedido {id}", id);
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        /// <summary>
        /// Obtiene el historial de órdenes de un cliente específico.
        /// </summary>
        /// <param name="clientId">ID del cliente.</param>
        /// <returns>Lista de órdenes del cliente.</returns>
        [HttpGet("history/{clientId}")]
        [Authorize]
        public async Task<IActionResult> GetHistory(string clientId)
        {
            try
            {
                var meta = GetUserMetadata();
                var req = new GetClientHistoryRequest { ClientId = clientId };
                var resp = await _ordersAdapter.GetClientHistoryAsync(req, meta);
                return Ok(resp.Orders);
            }
            catch (RpcException ex)
            {
                 _logger.LogError(ex, "Error gRPC al obtener historial {clientId}", clientId);
                return StatusCode((int)ex.StatusCode, new { message = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial {clientId}", clientId);
                return StatusCode(500, new { message = "Error interno" });
            }
        }
    }
}