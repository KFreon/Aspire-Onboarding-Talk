using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("app1db")
    ?? throw new InvalidOperationException("Connection string 'app1db' not found.");

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var products = new[]
{
    ("Wireless Keyboard", "Compact wireless keyboard with Bluetooth", 49.99m, 150),
    ("USB-C Hub", "7-in-1 USB-C hub with HDMI and ethernet", 29.99m, 200),
    ("Mechanical Keyboard", "Cherry MX Blue switches, RGB backlit", 89.99m, 75),
    ("27-inch Monitor", "4K IPS display with USB-C input", 349.99m, 30),
    ("Ergonomic Mouse", "Vertical ergonomic mouse with adjustable DPI", 39.99m, 120),
};

foreach (var (name, description, price, stock) in products)
{
    var sql = """
        IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = @Name)
        INSERT INTO Products (Name, Description, Price, StockQuantity)
        VALUES (@Name, @Description, @Price, @StockQuantity)
        """;

    using var cmd = new SqlCommand(sql, connection);
    cmd.Parameters.AddWithValue("@Name", name);
    cmd.Parameters.AddWithValue("@Description", description);
    cmd.Parameters.AddWithValue("@Price", price);
    cmd.Parameters.AddWithValue("@StockQuantity", stock);
    await cmd.ExecuteNonQueryAsync();
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("App1 seed data inserted successfully!");
Console.ResetColor();
