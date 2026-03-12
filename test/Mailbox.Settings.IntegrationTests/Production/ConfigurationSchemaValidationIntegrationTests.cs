// <copyright file="ConfigurationSchemaValidationIntegrationTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests.Production;

using System;
using System.Collections.Generic;
using System.Linq;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

/// <summary>
/// Integration tests that validate the structure and integrity of S3 configuration for Production.
/// Valid JSON, required fields (MailboxId, EmailAddress) on Production mailboxes, no duplicate MailboxId or EmailAddress,
/// clear failure messages. Other properties have defaults and are not validated.
/// </summary>
[Trait("Category", "IntegrationTest")]
[Trait("Status", "Ready")]
public class ConfigurationSchemaValidationIntegrationTests
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurationSchemaValidationIntegrationTests(ITestOutputHelper output)
    {
        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Production", false);
            })
            .ConfigureLogging(builder =>
            {
                builder.AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(output)
                    .AddServiceOptions<AmazonS3ApiOptions>(AmazonS3ApiOptions.DefaultSectionName)
                    .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName)
                    .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<MailboxFolderOptions>(MailboxFolderOptions.DefaultSectionName)
                    .AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>()
                    .AddSingleton<GraphApiClient>()
                    .AddSingleton<IGraphApiClient>(sp => sp.GetRequiredService<GraphApiClient>())
                    .AddSingleton<GraphApiEmailClient>()
                    .AddSingleton<IGraphApiEmailClient>(sp => sp.GetRequiredService<GraphApiEmailClient>())
                    .AddSingleton<MailboxOptions>();
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.serviceProvider = serviceScope.ServiceProvider;
    }

    /// <summary>All configuration files parse without errors. Duplicates cause constructor to throw with clear message.</summary>
    [S3IntegrationFact]
    public void ConfigurationFilesParseWithoutErrors()
    {
        var act = () => this.serviceProvider.GetRequiredService<MailboxOptions>();

        act.Should().NotThrow(
            "All S3 configuration files must be valid JSON. Duplicate MailboxId or EmailAddress are reported in the exception message.");
    }

    /// <summary>Required fields (MailboxId, EmailAddress) are present on every Production mailbox. Failure message lists which fields are missing.</summary>
    [S3IntegrationFact]
    public void ProductionMailboxesHaveRequiredFields()
    {
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        var productionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var errors = new List<string>();
        for (int i = 0; i < productionMailboxes.Count; i++)
        {
            var m = productionMailboxes[i];
            string prefix = $"Production mailbox index {i}";
            if (string.IsNullOrWhiteSpace(m.MailboxId))
                errors.Add($"{prefix}: missing or empty MailboxId.");
            if (string.IsNullOrWhiteSpace(m.EmailAddress))
                errors.Add($"{prefix}: missing or empty EmailAddress.");
        }

        errors.Should().BeEmpty(
            "Required fields (MailboxId, EmailAddress) must be present. " + string.Join(" ", errors));
    }

    /// <summary>No duplicate MailboxId across all environments. Failure message identifies which values are duplicated.</summary>
    [S3IntegrationFact]
    public void NoDuplicateMailboxIds()
    {
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        var duplicateIds = mailboxOptions.MailboxList
            .GroupBy(m => m.MailboxId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicateIds.Should().BeEmpty(
            "Duplicate MailboxId values are not allowed. Duplicated: " + string.Join(", ", duplicateIds));
    }

    /// <summary>No duplicate EmailAddress across all environments. Failure message identifies which values are duplicated.</summary>
    [S3IntegrationFact]
    public void NoDuplicateEmailAddresses()
    {
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        var duplicateEmails = mailboxOptions.MailboxList
            .GroupBy(m => m.EmailAddress, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicateEmails.Should().BeEmpty(
            "Duplicate EmailAddress values are not allowed. Duplicated: " + string.Join(", ", duplicateEmails));
    }
}
