using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api.src.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly Services.InventoryGrpcAdapter _inventoryGrpcAdapter;
        public InventoryController(Services.InventoryGrpcAdapter inventoryGrpcAdapter)
        {
            _inventoryGrpcAdapter = inventoryGrpcAdapter;
        }
        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var response = await _inventoryGrpcAdapter.GetInventory(new Google.Protobuf.WellKnownTypes.Empty());
            return Ok(response.Products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] InventoryService.Grpc.ProductMessage product)
        {
            var response = await _inventoryGrpcAdapter.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { productId = response.Product.Id }, response);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            var request = new InventoryService.Grpc.GetProductByIdRequest { ProductId = productId };
            var response = await _inventoryGrpcAdapter.GetProductById(request);
            return Ok(response.Product);
        }
        [HttpPut("{productId}/stock")]
        public async Task<IActionResult> UpdateStock(string productId, [FromBody] int amount)
        {
            var response = await _inventoryGrpcAdapter.UpdateStockAsync(productId, amount);
            if (response.Product == null)
            {
                return NotFound(new { Message = "Product not found" });
            }
            return Ok(response.Product);
        }
        [HttpPut("{productId}/minimum-stock")]
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