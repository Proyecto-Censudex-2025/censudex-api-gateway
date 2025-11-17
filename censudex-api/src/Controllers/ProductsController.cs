using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Google.Protobuf;

namespace censudex_api.src.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly Services.ProductsGrpcAdapter _productsGrpcAdapter;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(Services.ProductsGrpcAdapter productsGrpcAdapter, ILogger<ProductsController> logger)
        {
            _productsGrpcAdapter = productsGrpcAdapter;
            _logger = logger;
        }

        // GET /api/products
        [HttpGet]
        [HttpGet("get_all")]
        [HttpGet("/products/get_all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var resp = await _productsGrpcAdapter.GetProductsAsync();
                return Ok(resp.Products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new { message = "Error obteniendo productos" });
            }
        }

        // GET /api/products/{id}
        [HttpGet("{id}")]
        [HttpGet("find/{id}")]
        [HttpGet("/products/find/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var resp = await _productsGrpcAdapter.GetProductAsync(id);
                if (resp == null) return NotFound(new { message = "Producto no encontrado" });
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {id}", id);
                return StatusCode(500, new { message = "Error obteniendo producto" });
            }
        }

        // POST /api/products  (admin)
        [HttpPost]
        [HttpPost("create")]
        [HttpPost("/products/create")]
        [Authorize(Roles = "Admin")]
public async Task<IActionResult> Create([FromForm] CreateProductRequest dto, IFormFile image)
{
    try
    {
        byte[]? imageBuffer = null;
        if (image != null && image.Length > 0)
        {
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                imageBuffer = ms.ToArray();
            }
        }


        var grpcReq = new ProductService.Grpc.CreateProductRequest
        {
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Price = dto.Price,
            Category = dto.Category ?? "",
        };

        if (imageBuffer != null)
        {
            grpcReq.ImageBuffer = ByteString.CopyFrom(imageBuffer);
        }

        var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
            ? HttpContext.Request.Headers["Authorization"].ToString()
            : string.Empty;
        var resp = await _productsGrpcAdapter.CreateProductAsync(grpcReq, authHeader);
        
        if (resp == null)
        {
            _logger.LogWarning("Create product failed: backend service unavailable or returned no product");
            return StatusCode(500, new { message = "Error creando producto (servicio no disponible)" });
        }


        var productDto = new ProductDto
        {
            Id = resp.Id,
            Name = resp.Name,
            Description = resp.Description,
            Price = resp.Price,
            Category = resp.Category,
            ImageUrl = resp.ImageUrl, 
            IsActive = resp.IsActive,
            CreatedAt = resp.CreatedAt,
            UpdatedAt = resp.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = resp.Id }, productDto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating product");
        return StatusCode(500, new { message = "Error creando producto" });
    }
}

        // PUT /api/products/{id} (admin)
        [HttpPut("{id}")]
        [HttpPatch("edit/{id}")]
        [HttpPatch("/products/edit/{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateProductRequest dto, IFormFile? image)
        {
            try
            {
                byte[]? imageBuffer = null;
                if (image != null && image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await image.CopyToAsync(ms);
                        imageBuffer = ms.ToArray();
                    }
                }

                var grpcReq = new ProductService.Grpc.UpdateProductRequest
                {
                    Id = id,
                    Name = dto.Name ?? "",
                    Description = dto.Description ?? "",
                    Price = dto.Price ?? 0,
                    Category = dto.Category ?? "",
                };

                if (imageBuffer != null)
                {
                    grpcReq.ImageBuffer = ByteString.CopyFrom(imageBuffer);
                }

                var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
                    ? HttpContext.Request.Headers["Authorization"].ToString()
                    : string.Empty;
                var resp = await _productsGrpcAdapter.UpdateProductAsync(grpcReq, authHeader);
                if (resp == null) return NotFound(new { message = "Producto no encontrado" });
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {id}", id);
                return StatusCode(500, new { message = "Error actualizando producto" });
            }
        }

        // DELETE /api/products/{id} (admin) - soft delete
        [HttpDelete("{id}")]
        [HttpDelete("delete/{id}")]
        [HttpDelete("/products/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var authHeader = HttpContext.Request.Headers.ContainsKey("Authorization")
                    ? HttpContext.Request.Headers["Authorization"].ToString()
                    : string.Empty;
                var resp = await _productsGrpcAdapter.DeleteProductAsync(id, authHeader);
                if (resp == null) return NotFound(new { message = "Producto no encontrado" });
                return Ok(new { message = resp.Message, success = resp.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {id}", id);
                return StatusCode(500, new { message = "Error eliminando producto" });
            }
        }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public string Category { get; set; }
    }
}