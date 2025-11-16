using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientsService.Grcp;
using Grpc.Core;
using Grpc.Net.Client;

namespace censudex_api.src.Services
{
    public class ClientsGrpcAdapter
    {
        private readonly string _grpcAddress;
        private readonly ILogger<ClientsGrpcAdapter> _logger;

        public ClientsGrpcAdapter(IConfiguration configuration, ILogger<ClientsGrpcAdapter> logger)
        {
            _grpcAddress = configuration["GrpcServices:ClientsService"] ?? "http://localhost:5253";
            _logger = logger;
        }

        private ClientService.ClientServiceClient CreateClient()
        {
            var channel = GrpcChannel.ForAddress(_grpcAddress);
            return new ClientService.ClientServiceClient(channel);
        }

        private Metadata BuildAuthMetadata(string authHeader)
        {
            var meta = new Metadata();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                meta.Add("authorization", authHeader);
            }
            return meta;
        }

        public async Task<GetAllClientsResponse> GetAllClientsAsync(string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                var req = new GetAllClientsRequest();
                return await client.GetAllClientsAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new GetAllClientsResponse();
                }
                throw;
            }
        }

        public async Task<GetClientResponse> GetClientAsync(string id)
        {
            try
            {
                var client = CreateClient();
                var req = new GetClientRequest { Id = id };
                return await client.GetClientAsync(req);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new GetClientResponse { Found = false };
                }
                throw;
            }
        }

        public async Task<GetClientsFilteredResponse> GetClientsFilteredAsync(
            GetClientsFilteredRequest req, 
            string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                return await client.GetClientsFilteredAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new GetClientsFilteredResponse();
                }
                throw;
            }
        }

        public async Task<EnableDisableClientResponse> EnableDisableClientAsync(
            string id, 
            string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                var req = new EnableDisableClientRequest { Id = id };
                return await client.EnableDisableClientAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new EnableDisableClientResponse { Success = false, Message = "Service unavailable" };
                }
                throw;
            }
        }

        public async Task<UpdateClientResponse> UpdateClientAsync(
            UpdateClientRequest req, 
            string authHeader)
        {
            try
            {
                var client = CreateClient();
                var meta = BuildAuthMetadata(authHeader);
                return await client.UpdateClientAsync(req, meta);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new UpdateClientResponse { Success = false, Message = "Service unavailable" };
                }
                throw;
            }
        }

        public async Task<RegisterClientResponse> RegisterClientAsync(RegisterClientRequest req)
        {
            try
            {
                var client = CreateClient();
                return await client.RegisterClientAsync(req);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unimplemented)
                {
                    _logger.LogWarning("Clients service not implemented on target: {Addr}", _grpcAddress);
                    return new RegisterClientResponse { Success = false, Message = "Service unavailable" };
                }
                throw;
            }
        }
    }
}