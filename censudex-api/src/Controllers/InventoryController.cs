using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// Constructor del controlador de inventario.
        /// </summary>
        /// <param name="inventoryGrpcAdapter">Adaptador gRPC para el servicio de inventario.</param>
        public InventoryController(Services.InventoryGrpcAdapter inventoryGrpcAdapter)
        {
            _inventoryGrpcAdapter = inventoryGrpcAdapter;
        }

        /// <summary>
        /// Obtiene la lista de productos en el inventario.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetInventory()
        {
            var response = await _inventoryGrpcAdapter.GetInventory(new Google.Protobuf.WellKnownTypes.Empty());
            return Ok(response.Products);
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
            var response = await _inventoryGrpcAdapter.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { productId = response.Product.Id }, response);
        }
        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <returns>Producto encontrado.</returns>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            var request = new InventoryService.Grpc.GetProductByIdRequest { ProductId = productId };
            var response = await _inventoryGrpcAdapter.GetProductById(request);
            return Ok(response.Product);
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
            var response = await _inventoryGrpcAdapter.UpdateStockAsync(productId, amount);
            if (response.Product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }
            return Ok(response.Product);
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
            var response = await _inventoryGrpcAdapter.SetMinimumStockAsync(productId, minimumStock);
            if (response.Product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }
            return Ok(response.Product);
        }
        
    }
}