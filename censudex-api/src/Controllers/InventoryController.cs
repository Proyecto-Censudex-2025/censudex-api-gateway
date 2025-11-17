using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api.src.Controllers
{
    /// <summary>
    /// Controlador para la gestión del inventario.
    /// </summary>
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        /// <summary>
        /// Adaptador gRPC para el servicio de inventario.  
        /// </summary>
        private readonly Services.InventoryGrpcAdapter _inventoryGrpcAdapter;
        /// <summary>
        /// Logger para el controlador.
        /// </summary>
        private readonly ILogger<InventoryController> _logger;
        /// <summary>
        /// Constructor del controlador de inventario.
        /// </summary>
        /// <param name="inventoryGrpcAdapter">Adaptador gRPC para el servicio de inventario.</param>
        /// <param name="logger">Logger para el controlador.</param>
        public InventoryController(Services.InventoryGrpcAdapter inventoryGrpcAdapter, ILogger<InventoryController> logger)
        {
            _inventoryGrpcAdapter = inventoryGrpcAdapter;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de productos en el inventario.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetInventory()
        {
            try
            {
                var response = await _inventoryGrpcAdapter.GetInventory(new Google.Protobuf.WellKnownTypes.Empty());
                return Ok(response.Products);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error getting inventory");
                return StatusCode(503, new { message = "Error communicating with the Inventory Service", details = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting inventory");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        /// <summary>
        /// Agrega un nuevo producto al inventario.
        /// </summary>
        /// <param name="product">Producto a agregar.</param>
        /// <returns>Producto agregado.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProduct([FromBody] InventoryService.Grpc.ProductMessage product)
        {
            try
            {
                var response = await _inventoryGrpcAdapter.AddProductAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { productId = response.Product.Id }, response.Product);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error adding product");

                if (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
                {
                    return BadRequest(new { message = ex.Status.Detail });
                }
                
                return StatusCode(503, new { message = "Error communicating with the Inventory Service", details = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding product");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <returns>Producto encontrado.</returns>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            try
            {
                var request = new InventoryService.Grpc.GetProductByIdRequest { ProductId = productId };
                var response = await _inventoryGrpcAdapter.GetProductById(request);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error getting product {productId}", productId);
                
                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    return NotFound(new { message = ex.Status.Detail });
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
                {
                    return BadRequest(new { message = ex.Status.Detail });
                }
                return StatusCode(503, new { message = "Error communicating with the Inventory Service", details = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting product {productId}", productId);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        /// <summary>
        /// Actualiza la cantidad en stock de un producto.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <param name="amount">Cantidad a actualizar.</param>
        /// <returns>Producto actualizado.</returns>
        [HttpPatch("{productId}/stock")]
        public async Task<IActionResult> UpdateStock(string productId, [FromBody] int amount)
        {
            try
            {
                var response = await _inventoryGrpcAdapter.UpdateStockAsync(productId, amount);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error updating stock {productId}", productId);

                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    return NotFound(new { message = ex.Status.Detail });
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
                {
                    return BadRequest(new { message = ex.Status.Detail });
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.FailedPrecondition)
                {
                    return Conflict(new { message = ex.Status.Detail }); 
                }
                return StatusCode(503, new { message = "Error communicating with the Inventory Service", details = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating stock {productId}", productId);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        /// <summary>
        /// Establece el stock mínimo de un producto.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <param name="minimumStock">Stock mínimo a establecer.</param>
        /// <returns>Producto actualizado.</returns>
        [HttpPatch("{productId}/minimum-stock")]
        public async Task<IActionResult> SetMinimumStock(string productId, [FromBody] int minimumStock)
        {
            try
            {
                var response = await _inventoryGrpcAdapter.SetMinimumStockAsync(productId, minimumStock);
                return Ok(response.Product);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error setting minimum stock {productId}", productId);
                
                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    return NotFound(new { message = ex.Status.Detail });
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
                {
                    return BadRequest(new { message = ex.Status.Detail });
                }

                return StatusCode(503, new { message = "Error communicating with the Inventory Service", details = ex.Status.Detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error setting minimum stock {productId}", productId);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        
    }
}