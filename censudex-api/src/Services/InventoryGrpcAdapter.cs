using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using InventoryService.Grpc;

namespace censudex_api.src.Services
{
    /// <summary>
    /// Adaptador gRPC para el servicio de inventario.
    /// </summary>
    public class InventoryGrpcAdapter
    {
        /// <summary>
        /// Cliente gRPC para el servicio de inventario.
        /// </summary>
        private readonly Inventory.InventoryClient _client;

        /// <summary>
        /// Constructor del adaptador gRPC.
        /// </summary>
        /// <param name="client">Cliente gRPC para el servicio de inventario.</param>
        public InventoryGrpcAdapter(Inventory.InventoryClient client)
        {
            _client = client;
        }
        /// <summary>
        /// Agrega un nuevo producto al inventario.
        /// </summary>
        /// <param name="product">Producto a agregar.</param>
        /// <returns>Respuesta de la operación.</returns>
        public async Task<AddProductResponse> AddProductAsync(ProductMessage product)
        {
            var request = new AddProductRequest { Product = product };
            return await _client.AddProductAsync(request);
        }
        /// <summary>
        /// Obtiene la lista de productos en el inventario.
        /// </summary>
        /// <param name="request">Solicitud para obtener todos los productos.</param>
        /// <returns>Respuesta con la lista de productos.</returns>
        public async Task<GetAllProductsResponse> GetInventory(Empty request)
        {
            return await _client.GetAllProductsAsync(request);
        }
        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="request">Solicitud para obtener un producto por ID.</param>
        /// <returns>Respuesta con el producto encontrado.</returns>
        public async Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request)
        {
            return await _client.GetProductByIdAsync(request);
        }
        /// <summary>
        /// Actualiza la cantidad en stock de un producto.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <param name="amount">Cantidad a actualizar.</param>
        /// <returns>Respuesta de la operación.</returns>
        public async Task<UpdateStockResponse> UpdateStockAsync(string productId, int amount)
        {
            var request = new UpdateStockRequest
            {
                ProductId = productId,
                Amount = amount
            };

            return await _client.UpdateStockAsync(request);
        }
        /// <summary>
        /// Establece el stock mínimo de un producto.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <param name="minimumStock">Stock mínimo a establecer.</param>
        /// <returns>Respuesta de la operación.</returns>
        public async Task<SetMinimumStockResponse> SetMinimumStockAsync(string productId, int minimumStock)
        {
            var request = new SetMinimumStockRequest
            {
                ProductId = productId,
                MinimumStock = minimumStock
            };

            return await _client.SetMinimumStockAsync(request);
        }
        
    }
}