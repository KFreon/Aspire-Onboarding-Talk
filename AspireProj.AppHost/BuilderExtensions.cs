using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireProj.AppHost;

public static class AspireConstants
{
    public const string Seq = "seq";
    public const string AzureStorage = "azure-storage";
    public const string GroupedCommands = "groupexecutions";
    public const string SharedConfig = "somesharedconfig";
    public const string SharedConfigSomeKey = "SharedConfig__SomeKey";
}

public static class BuilderExtensions
{
    public static IResourceBuilder<IResourceWithConnectionString> AddSeq(this IDistributedApplicationBuilder builder)
    {
        IResourceBuilder<IResourceWithConnectionString> seq = default!;

        var seqConnectionString = builder.Configuration.GetConnectionString(AspireConstants.Seq);

        if (!string.IsNullOrEmpty(seqConnectionString))
            seq = builder.AddConnectionString(AspireConstants.Seq);
        else
            seq = builder.AddSeq(AspireConstants.Seq)
                         .ExcludeFromManifest()
                         .WithLifetime(ContainerLifetime.Persistent)
                         .WithEnvironment("ACCEPT_EULA", "Y");

        return seq;
    }

    public static IResourceBuilder<T> WithCustomCommandToExecuteDifferentProject<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> resource, string commandName, CommandOptions commandOptions = null) where T : IResource
    {
        return builder.WithCustomCommandToExecuteDifferentProjects([resource], commandName, commandOptions);
    }

    public static IResourceBuilder<T> WithCustomCommandToExecuteDifferentProject<T>(this IResourceBuilder<T> builder, string resourceName, string commandName, CommandOptions commandOptions = null) where T : IResource
    {
        var resource = builder.GetResouceBuilder(resourceName);
        return builder.WithCustomCommandToExecuteDifferentProjects([resource], commandName, commandOptions);
    }

    public static IResourceBuilder<T> WithCustomCommandToExecuteDifferentProjects<T>(this IResourceBuilder<T> builder, string[] resourceNames, string commandName, CommandOptions? commandOptions = null) where T : ProjectResource
    {
        var resources = resourceNames.Select(r => builder.GetResouceBuilderForProject(r));
        return builder.WithCustomCommandToExecuteDifferentProjects([.. resources], commandName, commandOptions);
    }

    public static IResourceBuilder<T> WithCustomCommandToExecuteDifferentProjects<T>(this IResourceBuilder<T> builder, IEnumerable<IResourceBuilder<IResource>> resources, string commandName, CommandOptions? commandOptions = null) where T : IResource
    {
        return builder.WithCommand(commandName, commandName, async execute =>
        {
            var commandService = execute.ServiceProvider.GetRequiredService<ResourceCommandService>();

            List<ExecuteCommandResult> results = [];
            foreach (var resource in resources)
            {
                var result = await commandService.ExecuteCommandAsync(resource.Resource, KnownResourceCommands.StartCommand);
                results.Add(result);
            }

            var errors = results.Where(x => !x.Success).ToArray();

            return errors.Any()
                ? new ExecuteCommandResult { Success = false, ErrorMessage = string.Join(", ", errors.Select(x => x.ErrorMessage)) }
                : new ExecuteCommandResult { Success = true };

        }, commandOptions ?? new CommandOptions
        {
            IsHighlighted = true,
            IconName = "Play",
            IconVariant = IconVariant.Regular
        });
    }

    public static IResourceBuilder<ProjectResource> WithDbUp<X>(this IResourceBuilder<ProjectResource> builder) where X : IProjectMetadata, new()
    {
        var name = builder.Resource.Name + "dbup";
        builder.ApplicationBuilder.AddProject<X>(name)
            .WithSeq()
            .WithParentRelationship(builder)
            .WithExplicitStart();

        return builder;
    }

    public static IResourceBuilder<ProjectResource> WithSeed<X>(this IResourceBuilder<ProjectResource> builder) where X : IProjectMetadata, new()
    {
        var name = builder.Resource.Name + "seed";
        builder.ApplicationBuilder.AddProject<X>(name)
            .WithSeq()
            .WithParentRelationship(builder)
            .WithExplicitStart();

        return builder;
    }

    public static IResourceBuilder<T> WithSeq<T>(this IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {
        var storageBuilder = builder.GetResouceBuilder(AspireConstants.Seq);
        return builder.WithReference(storageBuilder);
    }

    public static IResourceBuilder<IResourceWithConnectionString> GetResouceBuilder<T>(this IResourceBuilder<T> builder, string name) where T : IResource
    {
        return builder.ApplicationBuilder.GetResouceBuilder<IResourceWithConnectionString>(name);
    }

    public static IResourceBuilder<ProjectResource> GetResouceBuilderForProject<T>(this IResourceBuilder<T> builder, string name) where T : ProjectResource
    {
        return builder.ApplicationBuilder.GetResouceBuilder<ProjectResource>(name);
    }

    public static IResourceBuilder<X> GetResouceBuilder<X>(this IDistributedApplicationBuilder builder, string name) where X : IResource
    {
        var resource = (X)builder.Resources.First(x => x.Name == name);
        var resourceBuilder = builder.CreateResourceBuilder(resource);
        return resourceBuilder;
    }

    public static IResourceBuilder<ParameterResource> AddGroupExecution(this IDistributedApplicationBuilder builder, IResourceBuilder<IResource>[] resources, string iconName, string description)
    {
        var parent = builder.GetResouceBuilder<IResource>(AspireConstants.GroupedCommands);

        var paramName = string.Join("-and-", resources.Select(x => x.Resource.Name));
        return builder.AddParameter(paramName, "nothing")
            .WithExplicitStart()
            .WithIconName(iconName)
            .WithDescription(description)
            .WithCustomCommandToExecuteDifferentProjects(resources, "Start")
            .WithParentRelationship(parent);
    }
}
