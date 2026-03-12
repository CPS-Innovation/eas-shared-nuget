// <copyright file="ProductionMailboxConfigurationIntegrationTests.cs" company="TheCrownProsecutionService">
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
/// Integration tests for mailbox configuration in the Production environment that connect to real S3.
/// </summary>
[Trait("Category", "IntegrationTest")]
[Trait("Status", "Ready")]
public class ProductionMailboxConfigurationIntegrationTests
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionMailboxConfigurationIntegrationTests"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public ProductionMailboxConfigurationIntegrationTests(ITestOutputHelper output)
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

    /// <summary>
    /// Tests that the Production environment has the expected number of mailboxes.
    /// Expected count is based on current S3 configuration; verify against S3 at implementation time if the count changes.
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ProductionEnvironmentHasExpectedMailboxCount()
    {
        const int expectedCount = 189;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var productionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            .ToList();

        int actualCount = productionMailboxes.Count;

        actualCount.Should().Be(
            expectedCount,
            $"Expected {expectedCount} mailbox(es) in Production environment, but found {actualCount}. " +
            $"Mailboxes found: {string.Join(", ", productionMailboxes.Select(m => m.MailboxId))}");
    }

    /// <summary>
    /// Structural consistency: every mailbox in this bucket has valid Environment and non-empty Region.
    /// </summary>
    [S3IntegrationFact]
    public void EveryMailboxHasValidEnvironmentAndRegion()
    {
        var validEnvironments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Development",
            "Staging",
            "Production",
        };

        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        var errors = new List<string>();
        foreach (var m in mailboxOptions.MailboxList)
        {
            if (string.IsNullOrWhiteSpace(m.Environment))
                errors.Add($"MailboxId='{m.MailboxId}': Environment is missing or empty.");
            else if (!validEnvironments.Contains(m.Environment))
                errors.Add($"MailboxId='{m.MailboxId}': Environment '{m.Environment}' is not one of Development, Staging, Production.");
            if (string.IsNullOrWhiteSpace(m.Region))
                errors.Add($"MailboxId='{m.MailboxId}': Region is missing or empty.");
        }

        errors.Should().BeEmpty(
            "Every mailbox must have valid Environment and non-empty Region. " + string.Join(" ", errors));
    }

    /// <summary>
    /// Tests that each Production region has the expected number of mailboxes.
    /// Expected counts are based on current S3 configuration; verify against S3 at implementation time if counts change.
    /// </summary>
    [S3IntegrationTheory(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    [InlineData("Cymru-Wales", 6)]
    [InlineData("East Midlands", 6)]
    [InlineData("North West", 24)]
    [InlineData("West Midlands", 14)]
    [InlineData("Wessex", 12)]
    [InlineData("Yorkshire and Humberside", 12)]
    [InlineData("Mersey Cheshire", 21)]
    [InlineData("South West", 12)]
    [InlineData("South East", 10)]
    [InlineData("East of England", 9)]
    [InlineData("North East", 13)]
    [InlineData("Thames and Chiltern", 20)]
    [InlineData("London North", 2)]
    [InlineData("London South", 9)]
    [InlineData("Proceeds of Crime", 7)]
    [InlineData("Extradition", 4)]
    [InlineData("CPS Direct", 3)]
    [InlineData("ARU", 3)]
    [InlineData("VTP", 1)]
    [InlineData("London", 1)]
    public void ProductionRegionHasExpectedMailboxCount(string region, int expectedCount)
    {
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        int actualCount = regionMailboxes.Count;

        actualCount.Should().Be(
            expectedCount,
            $"Expected {expectedCount} mailbox(es) in Production region '{region}', but found {actualCount}. " +
            $"Mailboxes found: {string.Join(", ", regionMailboxes.Select(m => m.MailboxId))}");
    }

    /// <summary>
    /// Proceeds of Crime region: 7 mailboxes, all with POC URN support (from appsettings.Production.Proceeds of Crime.json).
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ProceedsOfCrimeRegion_Has7MailboxesAllWithPocUrnSupport()
    {
        const string region = "Proceeds of Crime";
        const int expectedCount = 7;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        regionMailboxes.Should().HaveCount(expectedCount,
            $"Region '{region}' must have {expectedCount} mailbox(es). Actual: {regionMailboxes.Count}.");

        var withoutPoc = regionMailboxes
            .Where(m => !mailboxOptions.RequiresProceedsOfCrimeUrnSupport(m.MailboxId))
            .ToList();

        withoutPoc.Should().BeEmpty(
            $"All {expectedCount} mailboxes in region '{region}' must have POC URN support. " +
            $"Missing: {string.Join(", ", withoutPoc.Select(m => m.MailboxId))}.");
    }

    /// <summary>
    /// Proceeds of Crime region: 7 mailboxes, all require email as MSG file (from appsettings.Production.Proceeds of Crime.json).
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ProceedsOfCrimeRegion_Has7MailboxesAllRequireEmailAsMsgFile()
    {
        const string region = "Proceeds of Crime";
        const int expectedCount = 7;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        regionMailboxes.Should().HaveCount(expectedCount,
            $"Region '{region}' must have {expectedCount} mailbox(es). Actual: {regionMailboxes.Count}.");

        var withoutMsg = regionMailboxes
            .Where(m => !mailboxOptions.RequiresEmailAsMsgFile(m.MailboxId))
            .ToList();

        withoutMsg.Should().BeEmpty(
            $"All {expectedCount} mailboxes in region '{region}' must require email as MSG file. " +
            $"Missing: {string.Join(", ", withoutMsg.Select(m => m.MailboxId))}.");
    }

    /// <summary>
    /// Proceeds of Crime region: 7 mailboxes, all secure (from appsettings.Production.Proceeds of Crime.json).
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ProceedsOfCrimeRegion_Has7MailboxesAllSecure()
    {
        const string region = "Proceeds of Crime";
        const int expectedCount = 7;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        regionMailboxes.Should().HaveCount(expectedCount,
            $"Region '{region}' must have {expectedCount} mailbox(es). Actual: {regionMailboxes.Count}.");

        var notSecure = regionMailboxes
            .Where(m => !mailboxOptions.RequiresSecureMailbox(m.MailboxId))
            .ToList();

        notSecure.Should().BeEmpty(
            $"All {expectedCount} mailboxes in region '{region}' must be secure. " +
            $"Not secure: {string.Join(", ", notSecure.Select(m => m.MailboxId))}.");
    }

    /// <summary>
    /// Extradition region: 4 mailboxes, 3 require email as MSG file (from appsettings.Production.Extradition.json; Extradition.Fees does not).
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ExtraditionRegion_Has4Mailboxes3RequireEmailAsMsgFile()
    {
        const string region = "Extradition";
        const int expectedRegionCount = 4;
        const int expectedWithMsg = 3;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        regionMailboxes.Should().HaveCount(expectedRegionCount,
            $"Region '{region}' must have {expectedRegionCount} mailbox(es). Actual: {regionMailboxes.Count}.");

        var withMsg = regionMailboxes
            .Where(m => mailboxOptions.RequiresEmailAsMsgFile(m.MailboxId))
            .ToList();

        withMsg.Should().HaveCount(expectedWithMsg,
            $"Region '{region}' must have exactly {expectedWithMsg} mailboxes that require email as MSG file. Actual: {withMsg.Count}. " +
            $"MailboxIds with MSG: {string.Join(", ", withMsg.Select(m => m.MailboxId))}.");
    }

    /// <summary>
    /// Extradition region: 4 mailboxes, all secure (from appsettings.Production.Extradition.json).
    /// </summary>
    [S3IntegrationFact(SkipUntilCanAccessBucketLocally = true)]
    [Trait("ReconsiderBrittleness", "true")]
    public void ExtraditionRegion_Has4MailboxesAllSecure()
    {
        const string region = "Extradition";
        const int expectedCount = 4;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                        m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();

        regionMailboxes.Should().HaveCount(expectedCount,
            $"Region '{region}' must have {expectedCount} mailbox(es). Actual: {regionMailboxes.Count}.");

        var notSecure = regionMailboxes
            .Where(m => !mailboxOptions.RequiresSecureMailbox(m.MailboxId))
            .ToList();

        notSecure.Should().BeEmpty(
            $"All {expectedCount} mailboxes in region '{region}' must be secure. " +
            $"Not secure: {string.Join(", ", notSecure.Select(m => m.MailboxId))}.");
    }
}
