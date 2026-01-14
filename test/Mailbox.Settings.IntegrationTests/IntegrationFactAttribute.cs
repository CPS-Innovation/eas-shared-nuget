// <copyright file="IntegrationFactAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;

using System;
using Amazon.Runtime;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

/// <summary>
/// Custom fact attribute for integration tests that automatically skips when AWS credentials are not available.
/// </summary>
public class IntegrationFactAttribute : FactAttribute
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
                return "AWS credentials are not configured. Skipping integration test.";
            }

            return base.Skip;
        }
        set => base.Skip = value;
    }

    /// <summary>
    /// Checks if AWS credentials are available for testing.
    /// </summary>
    /// <returns><c>true</c> if credentials are available; otherwise, <c>false</c>.</returns>
    private static bool CheckCredentialsAvailable()
    {
        try
        {
            // Check environment variables first (for pipeline)
            string? accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ??
                               Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            string? secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ??
                               Environment.GetEnvironmentVariable("AWS_SECRET_KEY");

            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
            {
                return true;
            }

            // Check configuration (for local development with privatesettings.json)
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
                        .AddServiceOptions<AmazonS3ApiOptions>(AmazonS3ApiOptions.DefaultSectionName)
                        .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName)
                        .AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>();
                })
                .Build();

            using IServiceScope scope = host.Services.CreateScope();
            IOptions<AmazonS3ApiOptions> apiOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonS3ApiOptions>>();
            IOptions<AmazonS3ClientFactoryOptions> factoryOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonS3ClientFactoryOptions>>();

            string? configAccessKey = apiOptions.Value.AccessKey;
            string? configSecretKey = apiOptions.Value.SecretKey;
            string? mailboxConfigContainer = factoryOptions.Value.MailboxConfigurationContainerName;

            // Check if credentials are configured and not placeholders
            bool hasCredentials = !string.IsNullOrWhiteSpace(configAccessKey) &&
                                 !string.IsNullOrWhiteSpace(configSecretKey) &&
                                 !configAccessKey.Contains("Placeholder", StringComparison.OrdinalIgnoreCase) &&
                                 !configSecretKey.Contains("Placeholder", StringComparison.OrdinalIgnoreCase) &&
                                 !string.IsNullOrWhiteSpace(mailboxConfigContainer) &&
                                 !mailboxConfigContainer.Contains("Placeholder", StringComparison.OrdinalIgnoreCase);

            if (hasCredentials)
            {
                // Try to create a client to verify credentials work
                try
                {
                    IAmazonS3ClientFactory factory = scope.ServiceProvider.GetRequiredService<IAmazonS3ClientFactory>();
                    var client = factory.GetMailboxConfigurationClient();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // Try AWS SDK default credential chain (IAM roles, profiles, etc.)
            try
            {
                var credentials = FallbackCredentialsFactory.GetCredentials();
                return credentials != null;
            }
            catch
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}
