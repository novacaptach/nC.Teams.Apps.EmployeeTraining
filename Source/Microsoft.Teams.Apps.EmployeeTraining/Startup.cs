// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining;

using System;
using System.Threading.Tasks;
using global::Azure.Identity;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.Teams.Apps.EmployeeTraining.Authentication;
using Microsoft.Teams.Apps.EmployeeTraining.Bot;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// The Startup class is responsible for configuring the DI container and acts as the composition root.
/// </summary>
public sealed class Startup
{
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    /// <param name="configuration">The environment provided configuration.</param>
    /// <param name="environment">The environment details</param>
    public Startup(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(paramName: nameof(configuration));
        this.CurrentEnvironment = environment;
        this.GetKeyVaultByManagedServiceIdentity().Wait();
        this.ValidateConfigurationSettings();
    }

    private IWebHostEnvironment CurrentEnvironment { get; }

    /// <summary>
    /// Configure the composition root for the application.
    /// </summary>
    /// <param name="services">The stub composition root.</param>
    /// <remarks>
    /// For more information see: https://go.microsoft.com/fwlink/?LinkID=398940.
    /// </remarks>
#pragma warning disable CA1506 // Composition root expected to have coupling with many components.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.RegisterConfidentialCredentialProvider(configuration: this.configuration);
        services.RegisterCredentialProviders(configuration: this.configuration);
        services.RegisterConfigurationSettings(configuration: this.configuration);
        services.RegisterRepositories();
        services.AddControllers();
        services.RegisterHelpers();
        services.AddSearchService(configuration: this.configuration);
        services.AddSingleton<TelemetryClient>();

        services
            .AddApplicationInsightsTelemetry(
                instrumentationKey: this.configuration.GetValue<string>(key: "ApplicationInsights:InstrumentationKey"));

        // In production, the React files will be served from this directory.
        services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

        services.RegisterBotStates(configuration: this.configuration);

        IdentityModelEventSource.ShowPII = false;
        services.RegisterAuthenticationServices(configuration: this.configuration);
        services.AddSingleton<IChannelProvider, SimpleChannelProvider>();
        services.AddSingleton<IMemoryCache, MemoryCache>();

        // Create the Bot Framework Adapter with error handling enabled.
        services.AddSingleton<IBotFrameworkHttpAdapter, EmployeeTrainingAdapterWithErrorHandler>();

        services.AddTransient<IBot, EmployeeTrainingActivityHandler>();

        // Create the Activity middle-ware that will be added to the middle-ware pipeline in the AdapterWithErrorHandler.
        services.AddSingleton<EmployeeTrainingActivityMiddleware>();
        services.AddTransient(serviceProvider =>
            (BotFrameworkAdapter)serviceProvider.GetRequiredService<IBotFrameworkHttpAdapter>());

        // Add i18n.
        services.RegisterLocalizationSettings(configuration: this.configuration);
        services.AddSearchService(configuration: this.configuration);
    }
#pragma warning restore CA1506

    /// <summary>
    /// Configure the application request pipeline.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">Hosting Environment.</param>
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseSpaStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpointRouteBuilder => endpointRouteBuilder.MapControllers());

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (env.EnvironmentName.ToUpperInvariant() == "DEVELOPMENT")
            {
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
        });
    }

    /// <summary>
    /// Validate whether the configuration settings are missing or not.
    /// </summary>
    private void ValidateConfigurationSettings()
    {
        var azureSettings = new AzureSettings();
        this.configuration.Bind(key: "AzureAd", instance: azureSettings);
        azureSettings.ClientId = this.configuration.GetValue<string>(key: "MicrosoftAppId");

        if (string.IsNullOrWhiteSpace(value: azureSettings.ClientId))
        {
            throw new ApplicationException(message: "AzureAD ClientId is missing in the configuration file.");
        }

        if (string.IsNullOrWhiteSpace(value: azureSettings.TenantId))
        {
            throw new ApplicationException(message: "AzureAD TenantId is missing in the configuration file.");
        }

        if (string.IsNullOrWhiteSpace(value: azureSettings.ApplicationIdURI))
        {
            throw new ApplicationException(message: "AzureAD ApplicationIdURI is missing in the configuration file.");
        }

        if (string.IsNullOrWhiteSpace(value: azureSettings.ValidIssuers))
        {
            throw new ApplicationException(message: "AzureAD ValidIssuers is missing in the configuration file.");
        }

        if (string.IsNullOrWhiteSpace(value: this.configuration.GetValue<string>(key: "App:ManifestId")))
        {
            throw new ApplicationException(message: "Manifest Id is missing in the configuration file.");
        }

        if ((this.configuration.GetValue<int?>(key: "App:CacheDurationInMinutes") == null) ||
            (this.configuration.GetValue<int>(key: "App:CacheDurationInMinutes") < 1))
        {
            throw new ApplicationException(message: "Invalid cache duration value in the configuration file.");
        }

        if ((this.configuration.GetValue<int?>(key: "App:EventsPageSize") == null) ||
            (this.configuration.GetValue<int>(key: "App:EventsPageSize") < 30))
        {
            throw new ApplicationException(
                message: "Invalid events page size value in the configuration file. The minimum value must be 30.");
        }
    }

    /// <summary>
    /// Get KeyVault secrets and app settings values
    /// </summary>
    private async Task GetKeyVaultByManagedServiceIdentity()
    {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        if (this.CurrentEnvironment.IsDevelopment())
        {
            await azureServiceTokenProvider.GetAccessTokenAsync(resource: "https://vault.azure.net")
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        var client = new SecretClient(vaultUri: new Uri(uriString: this.configuration[key: "KeyVault:BaseURL"]), credential: new DefaultAzureCredential());

        this.configuration[key: "Storage:ConnectionString"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "StorageConnection").ConfigureAwait(continueOnCapturedContext: false);
        this.configuration[key: "MicrosoftAppId"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "MicrosoftAppId").ConfigureAwait(continueOnCapturedContext: false);
        this.configuration[key: "MicrosoftAppPassword"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "MicrosoftAppPassword").ConfigureAwait(continueOnCapturedContext: false);
        this.configuration[key: "SearchService:SearchServiceName"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "SearchServiceName").ConfigureAwait(continueOnCapturedContext: false);
        this.configuration[key: "SearchService:SearchServiceAdminApiKey"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "SearchServiceAdminApiKey").ConfigureAwait(continueOnCapturedContext: false);
        this.configuration[key: "SearchService:SearchServiceQueryApiKey"] =
            await this.GetValueFromKeyVault(client: client, keyVaultString: "SearchServiceQueryApiKey").ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<string> GetValueFromKeyVault(
        SecretClient client,
        string keyVaultString)
    {
        var secret = await client
            .GetSecretAsync(name: this.configuration[key: $"KeyVaultStrings:{keyVaultString}"])
            .ConfigureAwait(continueOnCapturedContext: false);
        return secret.Value.Value;
    }
}