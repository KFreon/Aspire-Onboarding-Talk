using AspireProj.AppHost;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

// TODO
// db viewers?


// Databases
var sql = builder.AddSqlServer("sql");
var app1db = sql.AddDatabase("app1db");

var pg = builder.AddPostgres("pg");
var app2db = pg.AddDatabase("app2db");

var seq = builder.AddSeq();

var sharedKey = builder.AddParameterFromConfiguration(AspireConstants.SharedConfig, AspireConstants.SharedConfigSomeKey, true);

// APIs (wait for seed to complete)
var app1 = builder.AddProject<Projects.App1>("app1")
    .WithReference(app1db)
    .WithSeq()
    .WithEnvironment(AspireConstants.SharedConfigSomeKey, sharedKey)
    .WithExplicitStart()
    .WithDbUp<Projects.App1DbUp>()
    .WithSeed<Projects.App1Seed>()
    .WithExternalHttpEndpoints()
    .WithCustomCommandToExecuteDifferentProjects(["app1dbup", "app1seed"], "DbUp and seed DB", new CommandOptions
    {
        IsHighlighted = true,
        IconName = "ArrowSyncCircle",
        IconVariant = IconVariant.Filled
    });

var app2 = builder.AddProject<Projects.App2>("app2")
    .WithReference(app2db)
    .WithSeq()
    .WithEnvironment(AspireConstants.SharedConfigSomeKey, sharedKey)
    .WithExplicitStart()
    .WithDbUp<Projects.App1DbUp>()
    .WithSeed<Projects.App1Seed>()
    .WithExternalHttpEndpoints()
    .WithCustomCommandToExecuteDifferentProjects(["app2dbup", "app2seed"], "DbUp and seed DB", new CommandOptions
    {
        IsHighlighted = true,
        IconName = "ArrowSyncCircle",
        IconVariant = IconVariant.Filled
    }); ;

// React Frontends
builder.AddJavaScriptApp("app1-frontend", "../App1.Frontend", "watch")
    .WithNpm(true)
    .WithReference(app1)
    .WaitFor(app1)
    .WithParentRelationship(app1)
    .WithHttpEndpoint(env: "PORT");

builder.AddJavaScriptApp("app2-frontend", "../App2.Frontend", "watch")
    .WithNpm(true)
    .WithReference(app2)
    .WaitFor(app2)
    .WithParentRelationship(app2)
    .WithHttpEndpoint(env: "PORT");

List<InteractionInput> enabledFunctions = new();
var functionOptions = new string[] { "MyTimerFunction", "GetProductsSummary" }
    .Select(x => new InteractionInput { InputType = InputType.Boolean, Name = x, Value = x })
    .ToArray();

// Azure Functions
builder.AddAzureFunctionsProject<Projects.SomeFunctions>("somefunctions")
    .WithReference(app1)
    .WithReference(seq)
    .WithExplicitStart()
    .WithEnvironment(c =>
    {
        foreach (var func in enabledFunctions)
            c.EnvironmentVariables.Add($"AzureWebJobs.{func.Name}.Disabled", !bool.Parse(func.Value));
    })
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

builder.AddParameter(AspireConstants.GroupedCommands, "parent of below group commands");
builder.AddGroupExecution([app1, app2], "chartMultiple20Regular", "Start both");

builder.Build().Run();
