using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Core;
using ProductService.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace censudex_api.src.Services
{
    public class ProductsGrpcAdapter
    {
        private readonly string _grpcAddress;
        private readonly ILogger<ProductsGrpcAdapter> _logger;

        public ProductsGrpcAdapter(IConfiguration configuration, ILogger<ProductsGrpcAdapter> logger)
        {
            _grpcAddress = configuration["GrpcServices:ProductsService"] ?? "http://localhost:50051";
            _logger = logger;
        }

        private ProductsService.ProductsServiceClient CreateClient()
        {
            var channel = GrpcChannel.ForAddress(_grpcAddress);
            return new ProductsService.ProductsServiceClient(channel);
        }

        private Metadata BuildAuthMetadata(string authHeader)
        {
            var meta = new Metadata();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                meta.Add("authorization", authHeader.ToString());
            }
            return meta;
        }

        public async Task<ProductService.Grpc.GetProductsResponse> GetProductsAsync()
        {
            try
            {
                var client = CreateClient();
                // Call GetProducts with an empty request (backend expects GetProductsRequest)
                var req = new ProductService.Grpc.GetProductsRequest();
                return await client.GetProductsAsync(req);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented || ex.Status.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Products service not implemented on target: {Addr}", _grpcAddress);
                    return new ProductService.Grpc.GetProductsResponse();
                }
                throw;
            }
        }

        public async Task<ProductService.Grpc.Product> GetProductAsync(string id)
        {
            try
            {
                var client = CreateClient();
                var req = new ProductService.Grpc.GetProductRequest { Id = id };
                return await client.GetProductAsync(req);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented || ex.Status.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Products service not implemented on target: {Addr}", _grpcAddress);
                    return null;
                }
                throw;
            }
        }

        public async Task<ProductService.Grpc.Product> CreateProductAsync(ProductService.Grpc.CreateProductRequest req, string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                return await client.CreateProductAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented || ex.Status.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Products service not implemented on target: {Addr}", _grpcAddress);
                    return null;
                }
                throw;
            }
        }

        public async Task<ProductService.Grpc.Product> UpdateProductAsync(ProductService.Grpc.UpdateProductRequest req, string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                return await client.UpdateProductAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented || ex.Status.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Products service not implemented on target: {Addr}", _grpcAddress);
                    return null;
                }
                throw;
            }
        }

        public async Task<ProductService.Grpc.DeleteProductResponse> DeleteProductAsync(string id, string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                var req = new ProductService.Grpc.DeleteProductRequest { Id = id };
                return await client.DeleteProductAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented || ex.Status.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Products service not implemented on target: {Addr}", _grpcAddress);
                    return new ProductService.Grpc.DeleteProductResponse();
                }
                throw;
            }
        }
    }
}