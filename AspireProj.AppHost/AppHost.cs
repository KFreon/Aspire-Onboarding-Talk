using AspireProj.AppHost;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using Shared;
#pragma warning disable ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

// Databases
var sql = builder.AddSqlServer("sql")
    .WithDbGate()  // Sql server management
    .WithDataVolume() // persisted volume across launches
    .WithLifetime(ContainerLifetime.Persistent); // Keeps container around instead of destroying on exit (performance)

var app1db = sql.AddDatabase("app1db");

var pg = builder.AddPostgres("pg").WithPgAdmin();  // admin management, nothing persisted
var app2db = pg.AddDatabase("app2db");

// External logging. Aspire dash has this built in, but sometimes I want Seq :)
var seq = builder.AddSeq();

// Some config that's shared between the apps
// This is a common scenario for auth or caching or connection strings
var sharedKey = builder.AddParameterFromConfiguration(AspireConstants.SharedConfig, AspireConstants.SharedConfigSomeKey, true);

var app1 = builder.AddProject<Projects.App1_Web>("app1")
    .WithReference(app1db) // Injects connection string for App1Db
    .WaitFor(app1db)
    .WithSeq() // Use seq
    .WithEnvironment(AspireConstants.SharedConfigSomeKey, sharedKey) // Inject parameter, bit clunky :(
    .WithExplicitStart() // Don't start on startup, so I can start what I want
    .WithDbUp<Projects.App1DbUp>() // Use this DBUp
    .WithSeed<Projects.App1Seed>() // Use this seed
    .WithExternalHttpEndpoints() // expose endpoints outside aspire

    // Add a command to run the dbup and seed apps from UI
    .WithCustomCommandToExecuteDifferentProjects(["app1dbup", "app1seed"], "DbUp and seed DB", new CommandOptions
    {
        IsHighlighted = true,
        IconName = "ArrowSyncCircle",
        IconVariant = IconVariant.Filled
    }, true);

var app2 = builder.AddProject<Projects.App2_Web>("app2")
    .WithReference(app2db)
    .WaitFor(app2db)
    .WithSeq()
    .WithEnvironment(AspireConstants.SharedConfigSomeKey, sharedKey)
    .WithExplicitStart()
    .WithDbUp<Projects.App2DbUp>()
    .WithSeed<Projects.App2Seed>()
    .WithExternalHttpEndpoints()
    .WithCustomCommandToExecuteDifferentProjects(["app2dbup", "app2seed"], "DbUp and seed DB", new CommandOptions
    {
        IsHighlighted = true,
        IconName = "ArrowSyncCircle",
        IconVariant = IconVariant.Filled
    }, true);

builder.AddJavaScriptApp("app1-frontend", "../App1/App1.Frontend", "watch")  // Run the watch command. I don't want the dev server, serve through aspnetcore.
    .WithNpm(true) // do install first
    .WithReference(app1)
    .WaitFor(app1)
    .WithParentRelationship(app1) // make child of the app itself in the UI. Just makes it tidier
    .WithHttpEndpoint(env: "PORT"); // Not really used here, but kept as this is how we run the dev server on the right port for aspire to be aware of.

builder.AddJavaScriptApp("app2-frontend", "../App2/App2.Frontend", "watch")
    .WithNpm(true)
    .WithReference(app2)
    .WaitFor(app2)
    .WithParentRelationship(app2)
    .WithHttpEndpoint(env: "PORT");

// Create function list so we can show them in the modal UI
List<InteractionInput> enabledFunctions = new();
var functionOptions = new string[] { MyFunctionNames.MyTimerFunction, MyFunctionNames.GetProductsSummary }
    .Select(x => new InteractionInput { InputType = InputType.Boolean, Name = x, Value = x })
    .ToArray();

builder.AddAzureFunctionsProject<Projects.SomeFunctions>("somefunctions") // good naming right :D
    .WithReference(app1)
    .WithReference(seq)
    .WithExplicitStart()
    .WithEnvironment(c =>
    {
        // Only enable the selected functions, disable the rest
        foreach (var func in enabledFunctions)
            c.EnvironmentVariables.Add($"AzureWebJobs.{func.Name}.Disabled", !bool.Parse(func.Value));
    })

    // Add a command to pop a modal that shows a list of functions
    // Then we can pick the functions we want to run
    .WithCommand("Pick funcs", "Pick funcs", async c =>
    {
        var interactionService = c.ServiceProvider.GetRequiredService<IInteractionService>();
        if (interactionService.IsAvailable)
        {
            var result = await interactionService.PromptInputsAsync("Choose functions to run", "Selected functions will be enabled for execution, the rest will be disabled.", functionOptions);

            if (result.Canceled) return CommandResults.Success();

            enabledFunctions = [.. result.Data.Select(x => x)];

            var commandService = c.ServiceProvider.GetRequiredService<ResourceCommandService>();
            return await commandService.ExecuteCommandAsync(c.ResourceName, KnownResourceCommands.StartCommand);
        }
        return CommandResults.Success();
    }, new CommandOptions
    {
        IsHighlighted = true,
        IconName = "PlaySettings",
        IconVariant = IconVariant.Regular
    });

// I like being able to have a "group" where I can launch several things at once
// Groups aren't a thing in Aspire though, so using parameters as placeholders
// The first bit is good to have when you have many real parameters and group commands to group the groups together :D
builder.AddParameter(AspireConstants.GroupedCommands, "parent of below group commands");
builder.AddGroupExecution([app1, app2], "chartMultiple20Regular", "Start both");

builder.Build().Run();