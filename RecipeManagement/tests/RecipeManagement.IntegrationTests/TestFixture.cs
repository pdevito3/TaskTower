namespace RecipeManagement.IntegrationTests;

using RecipeManagement.Extensions.Services;
using RecipeManagement.Databases;
using RecipeManagement.Resources;
using RecipeManagement.SharedTestHelpers.Utilities;
using Configurations;
using FluentAssertions;
using FluentAssertions.Extensions;
using Hangfire;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

[CollectionDefinition(nameof(TestFixture))]
public class TestFixtureCollection : ICollectionFixture<TestFixture> {}

public class TestFixture : IAsyncLifetime
{
    public static IServiceScopeFactory BaseScopeFactory;
    private PostgreSqlContainer _dbContainer;
    private RabbitMqContainer _rmqContainer;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName
        });

        _dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();
        builder.Configuration.GetSection(ConnectionStringOptions.SectionName)[ConnectionStringOptions.RecipeManagementKey] = _dbContainer.GetConnectionString();
        await RunMigration(_dbContainer.GetConnectionString());

        var freePort = DockerUtilities.GetFreePort();
        _rmqContainer = new RabbitMqBuilder()
            .WithPortBinding(freePort, 5672)
            .Build();
        await _rmqContainer.StartAsync();
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.HostKey] = "localhost";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.VirtualHostKey] = "/";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.UsernameKey] = "guest";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PasswordKey] = "guest";
        builder.Configuration.GetSection(RabbitMqOptions.SectionName)[RabbitMqOptions.PortKey] = _rmqContainer.GetConnectionString();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();
        services.ReplaceServiceWithSingletonMock<IBackgroundJobClient>();

        var provider = services.BuildServiceProvider();
        BaseScopeFactory = provider.GetService<IServiceScopeFactory>();
    }

    private static async Task RunMigration(string connectionString)
    {
        var options = new DbContextOptionsBuilder<RecipesDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        var context = new RecipesDbContext(options, null, null, null);
        await context?.Database?.MigrateAsync();
    }

    public async Task DisposeAsync()
    {        
        await _dbContainer.DisposeAsync();
        await _rmqContainer.DisposeAsync();
    }
}

public static class ServiceCollectionServiceExtensions
{
    public static IServiceCollection ReplaceServiceWithSingletonMock<TService>(this IServiceCollection services)
        where TService : class
    {
        services.RemoveAll(typeof(TService));
        services.AddSingleton(_ => Substitute.For<TService>());
        return services;
    }
}
