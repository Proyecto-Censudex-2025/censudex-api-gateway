using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Core;
using OrdersService.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes; 

/// <summary>
/// Adaptador gRPC para el servicio de órdenes.
/// </summary>
namespace censudex_api.src.Services
{
    /// <summary>
    /// Adaptador gRPC para el servicio de órdenes.
    /// </summary>
    public class OrdersGrpcAdapter
    {
        /// <summary>
        /// Dirección del servicio gRPC de órdenes.
        /// </summary>
        private readonly string _grpcAddress;
        /// <summary>
        /// Logger para el adaptador gRPC de órdenes.
        /// </summary>
        private readonly ILogger<OrdersGrpcAdapter> _logger;
        /// <summary>
        /// Constructor del adaptador gRPC para órdenes.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="logger">Logger para el adaptador.</param>
        public OrdersGrpcAdapter(IConfiguration configuration, ILogger<OrdersGrpcAdapter> logger)
        {
            
            _grpcAddress = configuration["GrpcServices:OrdersService"] ?? "http://localhost:50052";
            _logger = logger;
        }
        /// <summary>
        /// Crea un cliente gRPC para el servicio de órdenes.
        /// </summary>
        /// <returns>Cliente gRPC para órdenes.</returns>
        private OrdersService.Grpc.OrdersService.OrdersServiceClient CreateClient()
        {
            var channel = GrpcChannel.ForAddress(_grpcAddress);
            return new OrdersService.Grpc.OrdersService.OrdersServiceClient(channel);
        }
        /// <summary>
        /// Construye la metadata de autenticación para las llamadas gRPC.
        /// </summary>
        /// <param name="authHeader">Encabezado de autorización.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="userRole">Rol del usuario.</param>
        /// <param name="userEmail">Correo electrónico del usuario.</param>
        /// <returns>Metadata con información de autenticación.</returns>
        private Metadata BuildAuthMetadata(string authHeader, string userId, string userRole, string userEmail)
        {
            var meta = new Metadata();
            
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                meta.Add("authorization", authHeader.ToString());
            }

            if (!string.IsNullOrWhiteSpace(userId)) meta.Add("x-user-id", userId);
            if (!string.IsNullOrWhiteSpace(userRole)) meta.Add("x-user-role", userRole);
            if (!string.IsNullOrWhiteSpace(userEmail)) meta.Add("x-user-email", userEmail);
            
            return meta;
        }
        
        /// <summary>
        /// Obtiene todas las órdenes según los parámetros de consulta.
        /// </summary>
        /// <param name="req">Parámetros para filtrar y paginar las órdenes.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con la lista de órdenes.</returns>
        public async Task<FindAllOrdersResponse> FindAllOrdersAsync(QueryOrderRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.FindAllOrdersAsync(req, meta);
        }

        /// <summary>
        /// Obtiene una orden por su ID.
        /// </summary>
        /// <param name="req">Solicitud con el ID de la orden.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con la orden solicitada.</returns>
        public async Task<OrderResponse> FindOneOrderAsync(FindOneOrderRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.FindOneOrderAsync(req, meta);
        }
        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="req">Datos para crear la orden.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con la orden creada.</returns>
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.CreateOrderAsync(req, meta);
        }
        
        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <param name="req">Datos para actualizar el estado de la orden.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con la orden actualizada.</returns>
        public async Task<OrderResponse> UpdateOrderStatusAsync(UpdateOrderStatusRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.UpdateOrderStatusAsync(req, meta);
        }

        /// <summary>
        /// Cancela una orden.
        /// </summary>
        /// <param name="req">Datos para cancelar la orden.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con la orden cancelada.</returns>
        public async Task<OrderResponse> CancelOrderAsync(CancelOrderRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.CancelOrderAsync(req, meta);
        }

        /// <summary>
        /// Obtiene el historial de un cliente.
        /// </summary>
        /// <param name="req">Solicitud con los parámetros para obtener el historial del cliente.</param>
        /// <param name="meta">Metadata con información de autenticación.</param>
        /// <returns>Respuesta con el historial del cliente.</returns>
        public async Task<FindAllOrdersResponse> GetClientHistoryAsync(GetClientHistoryRequest req, Metadata meta)
        {
            var client = CreateClient();
            return await client.GetClientHistoryAsync(req, meta);
        }

    }
}