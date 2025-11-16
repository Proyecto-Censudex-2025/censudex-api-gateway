namespace censudex_api.src.Dto
{
    // DTO para el item del pedido en el body HTTP
    public class CreateOrderItemHttpDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    // DTO para la orden completa en el body HTTP
    public class CreateOrderHttpDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public List<CreateOrderItemHttpDto> Items { get; set; } = new List<CreateOrderItemHttpDto>();
    }
}