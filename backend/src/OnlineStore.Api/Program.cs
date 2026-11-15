using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Application.Auth;
using OnlineStore.Application.Cart;
using OnlineStore.Application.Brands;
using OnlineStore.Application.Categories;
using OnlineStore.Application.Customers;
using OnlineStore.Application.Orders;
using OnlineStore.Application.Products;
using OnlineStore.Application.Shipping;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Auth;
using OnlineStore.Infrastructure.Carts;
using OnlineStore.Infrastructure.Brands;
using OnlineStore.Infrastructure.Categories;
using OnlineStore.Infrastructure.Customers;
using OnlineStore.Infrastructure.Products;
using OnlineStore.Infrastructure.Orders;
using OnlineStore.Infrastructure.Shipping;
using OnlineStore.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=.\\SQLEXPRESS;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IAdminBrandService, AdminBrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IAdminShippingService, AdminShippingService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IOrderService>(services => services.GetRequiredService<OrderService>());
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("Jwt:Secret is not configured. Set it via dotnet user-secrets.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "online-store-local";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "online-store-client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
});
builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<ProductCatalogSeeder>();
builder.Services.AddScoped<GovernorateSeeder>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // TODO comment this later when admin dashboard is ready
    await using var scope = app.Services.CreateAsyncScope();
    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await adminSeeder.SeedAsync();
    var productCatalogSeeder = scope.ServiceProvider.GetRequiredService<ProductCatalogSeeder>();
    await productCatalogSeeder.SeedAsync();
    var governorateSeeder = scope.ServiceProvider.GetRequiredService<GovernorateSeeder>();
    await governorateSeeder.SeedAsync();
}
app.UseCors("AngularApp");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
