using App2.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<App2DbContext>("app2db");
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

var api = app.MapGroup("/api/orders");

api.MapGet("/", async (App2DbContext db) =>
{
    var orders = await db.Orders.AsNoTracking().ToListAsync();
    return orders.Select(o => new OrderResponse(o.Id, o.CustomerName, o.CustomerEmail, o.ProductId, o.Quantity, o.TotalPrice, o.OrderDate, o.Status));
});

api.MapGet("/{id:int}", async (int id, App2DbContext db) =>
{
    var o = await db.Orders.FindAsync(id);
    return o is null
        ? Results.NotFound()
        : Results.Ok(new OrderResponse(o.Id, o.CustomerName, o.CustomerEmail, o.ProductId, o.Quantity, o.TotalPrice, o.OrderDate, o.Status));
});

api.MapPost("/", async (CreateOrderRequest request, App2DbContext db) =>
{
    var order = new Order
    {
        CustomerName = request.CustomerName,
        CustomerEmail = request.CustomerEmail,
        ProductId = request.ProductId,
        Quantity = request.Quantity,
        TotalPrice = request.Quantity * request.UnitPrice
    };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}",
        new OrderResponse(order.Id, order.CustomerName, order.CustomerEmail, order.ProductId, order.Quantity, order.TotalPrice, order.OrderDate, order.Status));
});

api.MapPut("/{id:int}", async (int id, CreateOrderRequest request, App2DbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();

    order.CustomerName = request.CustomerName;
    order.CustomerEmail = request.CustomerEmail;
    order.ProductId = request.ProductId;
    order.Quantity = request.Quantity;
    order.TotalPrice = request.Quantity * request.UnitPrice;
    await db.SaveChangesAsync();
    return Results.Ok(new OrderResponse(order.Id, order.CustomerName, order.CustomerEmail, order.ProductId, order.Quantity, order.TotalPrice, order.OrderDate, order.Status));
});

api.MapDelete("/{id:int}", async (int id, App2DbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
