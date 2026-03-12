// <copyright file="S3IntegrationAttributeBase.cs" company="TheCrownProsecutionService">
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
/// Abstract base for S3 integration test attributes. Holds the credential-check and skip logic
/// for use by both fact and theory attributes.
/// </summary>
public abstract class S3IntegrationAttributeBase : FactAttribute
{
    private static readonly Lazy<bool> CredentialsAvailable = new(CheckCredentialsAvailable);

    /// <summary>
    /// Gets the skip reason if credentials are not available; otherwise <c>null</c>.
    /// Used by both <see cref="S3IntegrationFactAttribute"/> and <see cref="S3IntegrationTheoryAttribute"/>.
    /// </summary>
    public static string? GetSkipReasonIfNotConfigured() =>
        CredentialsAvailable.Value ? null : "AWS credentials are not configured. Skipping S3 integration test.";

    /// <inheritdoc />
    public override string? Skip
    {
        get => GetSkipReasonIfNotConfigured() ?? base.Skip;
        set => base.Skip = value;
    }

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
