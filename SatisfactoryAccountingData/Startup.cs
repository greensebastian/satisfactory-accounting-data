using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatisfactoryAccountingData.Configuration;
using SatisfactoryAccountingData.Services;
using System;
using System.Configuration;

[assembly: FunctionsStartup(typeof(SatisfactoryAccountingData.Startup))]

namespace SatisfactoryAccountingData
{
    public class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(Configuration.GetConfigurationSection<StorageConfiguration>());
            builder.Services.AddSingleton(Configuration.GetConfigurationSection<AuthConfiguration>());

            // Register the CosmosClient as a Singleton
            builder.Services.AddSingleton(CosmosClientInitializationFactory);

            builder.Services.AddSingleton<AuthorizationService>();
            builder.Services.AddSingleton<CosmosSatisfactoryClient>();
        }

        private static CosmosClient CosmosClientInitializationFactory(IServiceProvider serviceProvider)
        {
            var storageConfig = serviceProvider.GetRequiredService<StorageConfiguration>();

            var configurationBuilder = new CosmosClientBuilder(storageConfig.EndPointUrl, storageConfig.AuthorizationKey);
            return configurationBuilder
                .WithSerializerOptions(new CosmosSerializationOptions
                {
                    Indented = true,
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                })
                .Build();
        }
    }

    public static class ConfigurationExtensions
    {
        public static T GetConfigurationSection<T>(this IConfiguration configuration) where T : class, IAppConfigurationSection, new()
        {
            var config = new T();
            configuration.GetSection(config.SectionName).Bind(config);
            foreach (var property in config.GetType().GetProperties())
            {
                var value = property.GetValue(config);
                if ((value?.Equals(default) ?? false) || string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    throw new ConfigurationErrorsException(
                        $"Missing required configuration property {property.Name} in section {config.SectionName}");
                }
            }
            return config;
        }
    }
}