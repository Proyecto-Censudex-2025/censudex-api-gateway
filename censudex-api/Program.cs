using censudex_api.src.Services;
using InventoryService.Grpc;
using ProductService.Grpc;
using OrdersService.Grpc;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

builder.Services.AddGrpcClient<Inventory.InventoryClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:GrpcBalancer"]);
});


builder.Services.AddGrpcClient<ProductsService.ProductsServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:GrpcBalancer"]);
});

builder.Services.AddGrpcClient<OrdersService.Grpc.OrdersService.OrdersServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:GrpcBalancer"]);
});


builder.Services.AddScoped<InventoryGrpcAdapter>();
builder.Services.AddScoped<ProductsGrpcAdapter>();
builder.Services.AddScoped<OrdersGrpcAdapter>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
