// <copyright file="S3IntegrationFactAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

/// <summary>
/// Custom fact attribute for integration tests that require S3/AWS credentials.
/// Automatically skips when AWS credentials are not available.
/// </summary>
public class S3IntegrationFactAttribute : FactAttribute
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
                return "AWS credentials are not configured. Skipping S3 integration test.";
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
                        .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName);
                })
                .Build();

            using IServiceScope scope = host.Services.CreateScope();
            IOptions<AmazonS3ApiOptions> apiOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonS3ApiOptions>>();
            IOptions<AmazonS3ClientFactoryOptions> factoryOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonS3ClientFactoryOptions>>();

            string? accessKey = apiOptions.Value.AccessKey;
            string? secretKey = apiOptions.Value.SecretKey;
            string? mailboxConfigContainer = factoryOptions.Value.MailboxConfigurationContainerName;

            return ConfigurationHelper.IsValueConfigured(accessKey) &&
                   ConfigurationHelper.IsValueConfigured(secretKey) &&
                   ConfigurationHelper.IsValueConfigured(mailboxConfigContainer);
        }
        catch
        {
            return false;
        }
    }
}
