var builder = DistributedApplication.CreateBuilder(args);

// Databases
var sql = builder.AddSqlServer("sql");
var app1db = sql.AddDatabase("app1db");

var pg = builder.AddPostgres("pg");
var app2db = pg.AddDatabase("app2db");

// DbUp migrations (run after databases are ready)
var app1dbup = builder.AddProject<Projects.App1DbUp>("app1dbup")
    .WithReference(app1db)
    .WaitFor(app1db);

var app2dbup = builder.AddProject<Projects.App2DbUp>("app2dbup")
    .WithReference(app2db)
    .WaitFor(app2db);

// Seed data (run after migrations complete)
var app1seed = builder.AddProject<Projects.App1Seed>("app1seed")
    .WithReference(app1db)
    .WaitForCompletion(app1dbup);

var app2seed = builder.AddProject<Projects.App2Seed>("app2seed")
    .WithReference(app2db)
    .WaitForCompletion(app2dbup);

// APIs (wait for seed to complete)
var app1 = builder.AddProject<Projects.App1>("app1")
    .WithReference(app1db)
    .WaitForCompletion(app1seed)
    .WithExternalHttpEndpoints();

var app2 = builder.AddProject<Projects.App2>("app2")
    .WithReference(app2db)
    .WaitForCompletion(app2seed)
    .WithExternalHttpEndpoints();

// React Frontends
builder.AddNpmApp("app1-frontend", "../App1.Frontend", "dev")
    .WithReference(app1)
    .WaitFor(app1)
    .WithHttpEndpoint(env: "PORT");

builder.AddNpmApp("app2-frontend", "../App2.Frontend", "dev")
    .WithReference(app2)
    .WaitFor(app2)
    .WithHttpEndpoint(env: "PORT");

// Azure Functions
builder.AddAzureFunctionsProject<Projects.SomeFunctions>("somefunctions")
    .WithReference(app1)
    .WaitFor(app1);

builder.Build().Run();
