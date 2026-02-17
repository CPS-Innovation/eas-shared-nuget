// <copyright file="GraphApiIntegrationFactAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

/// <summary>
/// Custom fact attribute for integration tests that require Graph API credentials.
/// Automatically skips when Graph API credentials are not available.
/// </summary>
public class GraphApiIntegrationFactAttribute : FactAttribute
{
    private static readonly Lazy<bool> CredentialsAvailable = new(CheckCredentialsAvailable);

    /// <summary>
    /// Gets the skip reason if credentials are not available.
    /// </summary>
    public override string? Skip
    {
        get
        {
            if (!CredentialsAvailable.Value)
            {
                return "Graph API credentials are not configured. Skipping Graph API integration test.";
            }

            return base.Skip;
        }
        set => base.Skip = value;
    }

    /// <summary>
    /// Checks if Graph API credentials are available for testing.
    /// </summary>
    /// <returns><c>true</c> if credentials are available; otherwise, <c>false</c>.</returns>
    private static bool CheckCredentialsAvailable()
    {
        try
        {
            string[] args = Array.Empty<string>();
            IHost host = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName);
                })
                .Build();

            using IServiceScope scope = host.Services.CreateScope();
            IOptions<GraphApiConnectionOptions> graphApiOptions = scope.ServiceProvider.GetRequiredService<IOptions<GraphApiConnectionOptions>>();

            string? clientId = graphApiOptions.Value.ClientId;
            string? clientSecret = graphApiOptions.Value.ClientSecret;
            string? tenantId = graphApiOptions.Value.TenantId;

            bool hasCredentials = ConfigurationHelper.IsValueConfigured(clientId) &&
                                 ConfigurationHelper.IsValueConfigured(clientSecret) &&
                                 ConfigurationHelper.IsValueConfigured(tenantId);

            return hasCredentials;
        }
        catch
        {
            return false;
        }
    }
}
