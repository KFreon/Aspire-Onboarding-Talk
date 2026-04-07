using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var dropDb = args.Length > 0 && args[0] == "--dropdb";
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("app2db")
    ?? throw new InvalidOperationException("Connection string 'app2db' not found.");

if (dropDb) DropDatabase.For.PostgresqlDatabase(connectionString);

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("App2 database migration completed successfully!");
Console.ResetColor();
return 0;
