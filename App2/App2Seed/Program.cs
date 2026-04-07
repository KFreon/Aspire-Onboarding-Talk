using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("app2db")
    ?? throw new InvalidOperationException("Connection string 'app2db' not found.");

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

var orders = new[]
{
    ("Alice Johnson", "alice@example.com", 1, 2, 99.98m, "Completed"),
    ("Bob Smith", "bob@example.com", 3, 1, 89.99m, "Pending"),
    ("Carol Davis", "carol@example.com", 5, 3, 119.97m, "Shipped"),
};

foreach (var (name, email, productId, qty, total, status) in orders)
{
    var sql = """
        INSERT INTO "Orders" ("CustomerName", "CustomerEmail", "ProductId", "Quantity", "TotalPrice", "Status")
        SELECT @name, @email, @productId, @qty, @total, @status
        WHERE NOT EXISTS (SELECT 1 FROM "Orders" WHERE "CustomerEmail" = @email AND "ProductId" = @productId)
        """;

    using var cmd = new NpgsqlCommand(sql, connection);
    cmd.Parameters.AddWithValue("@name", name);
    cmd.Parameters.AddWithValue("@email", email);
    cmd.Parameters.AddWithValue("@productId", productId);
    cmd.Parameters.AddWithValue("@qty", qty);
    cmd.Parameters.AddWithValue("@total", total);
    cmd.Parameters.AddWithValue("@status", status);
    await cmd.ExecuteNonQueryAsync();
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("App2 seed data inserted successfully!");
Console.ResetColor();
