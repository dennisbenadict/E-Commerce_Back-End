using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure.Models;
using ProductService.Application.Services;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services DI
builder.Services.AddScoped<ProductService.Application.Services.ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();


// CORS (Recommended for Angular/React client later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");

app.MapControllers();

app.Run();

