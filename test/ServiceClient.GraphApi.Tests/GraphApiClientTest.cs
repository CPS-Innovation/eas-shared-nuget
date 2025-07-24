// <copyright file="GraphApiClientTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApi.Tests;

using System;
using Azure.Identity;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

/// <summary>
/// Test connectivity with the Graph API.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class GraphApiClientTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiClientTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public GraphApiClientTest(ITestOutputHelper output)
    {
        string[] args = Array.Empty<string>();

        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
            })
            .ConfigureLogging(builder =>
            {
                builder
                    .AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(output)
                    .AddSingleton<GraphApiClient>()
                    .AddSingleton<IGraphApiClient, FakeGraphApiClient>();

                services
                    .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName);
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Instantiates a <see cref="GraphApiClient"/> and validates that <c>GraphServiceClient</c> is not <c>null</c>.
    /// </summary>
    [Fact]
    public void GraphServiceClientIsNotNull()
    {
        // Assemble
        IGraphApiClient client = this.ServiceProvider.GetRequiredService<IGraphApiClient>();

        // Act
        GraphServiceClient serviceClient = client.GraphServiceClient;

        // Assert
        serviceClient.Should().NotBeNull();
        client.Configuration.Should().NotBeNull();
    }

    /// <summary>
    /// The service client throws an exception when unauthorised.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GraphServiceClientThrowsExceptionForAnUnauthorisedCall()
    {
        // Assemble
        GraphApiClient client = this.ServiceProvider.GetRequiredService<GraphApiClient>();
        GraphServiceClient serviceClient = client.GraphServiceClient;

        // Act
        AuthenticationFailedException exception = await Assert
            .ThrowsAsync<AuthenticationFailedException>(async () =>
            {
                await serviceClient.Subscriptions
                    .GetAsync()
                    .ConfigureAwait(true);
            })
            .ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
    }

    /// <summary>
    /// Resetting authorisation succeeds.
    /// </summary>
    [Fact]
    public void ResettingAuthorisationSucceeds()
    {
        // Assemble
        IGraphApiClient client = this.ServiceProvider.GetRequiredService<IGraphApiClient>();
        client.GraphServiceClient.Should().NotBeNull();

        // Act
        client.ResetAuthorisation();

        // Assert
        client.GraphServiceClient.Should().NotBeNull();
    }
}
