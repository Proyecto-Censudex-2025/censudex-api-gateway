using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using InventoryService.Grpc;

namespace censudex_api.src.Services
{
    public class InventoryGrpcAdapter
    {
        private readonly Inventory.InventoryClient _client;

        public InventoryGrpcAdapter(Inventory.InventoryClient client)
        {
            _client = client;
        }
        public async Task<AddProductResponse> AddProductAsync(ProductMessage product)
        {
            var request = new AddProductRequest { Product = product };
            return await _client.AddProductAsync(request);
        }
        public async Task<GetAllProductsResponse> GetInventory(Empty request)
        {
            return await _client.GetAllProductsAsync(request);
        }

        public async Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request)
        {
            return await _client.GetProductByIdAsync(request);
        }

        public async Task<UpdateStockResponse> UpdateStockAsync(string productId, int amount)
        {
            var request = new UpdateStockRequest
            {
                ProductId = productId,
                Amount = amount
            };

            return await _client.UpdateStockAsync(request);
        }

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