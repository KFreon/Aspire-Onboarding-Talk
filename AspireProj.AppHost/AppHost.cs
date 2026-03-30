var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.App1>("app1");

builder.AddProject<Projects.App1DbUp>("app1dbup");

builder.AddProject<Projects.App1Seed>("app1seed");

builder.AddProject<Projects.App2>("app2");

builder.AddProject<Projects.App2DbUp>("app2dbup");

builder.AddProject<Projects.App2Seed>("app2seed");

builder.AddAzureFunctionsProject<Projects.SomeFunctions>("somefunctions");

builder.Build().Run();
