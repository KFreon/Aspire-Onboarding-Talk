using App1.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<App1DbContext>("app1db");
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.MapOpenApi();
app.MapDefaultEndpoints();

var api = app.MapGroup("/api/products");

api.MapGet("/", async (App1DbContext db) =>
{
    var products = await db.Products.AsNoTracking().ToListAsync();
    return products.Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CreatedAt));
});

api.MapGet("/{id:int}", async (int id, App1DbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    return p is null
        ? Results.NotFound()
        : Results.Ok(new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CreatedAt));
});

api.MapPost("/", async (CreateProductRequest request, App1DbContext db) =>
{
    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        Price = request.Price,
        StockQuantity = request.StockQuantity
    };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}",
        new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.CreatedAt));
});

api.MapPut("/{id:int}", async (int id, CreateProductRequest request, App1DbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = request.Name;
    product.Description = request.Description;
    product.Price = request.Price;
    product.StockQuantity = request.StockQuantity;
    await db.SaveChangesAsync();
    return Results.Ok(new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.CreatedAt));
});

api.MapDelete("/{id:int}", async (int id, App1DbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
