using censudex_api.src.Services;
using InventoryService.Grpc;
using ProductService.Grpc;
using OrdersService.Grpc;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using ClientsService.Grcp;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ClientOrAbove", policy =>
        policy.RequireRole("Admin", "User"));
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthService:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddGrpcClient<ClientService.ClientServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:GrpcBalancer"]);
});

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ClientsGrpcAdapter>();
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


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<censudex_api.src.Middleware.TokenValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
