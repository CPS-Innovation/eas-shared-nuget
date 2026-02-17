// <copyright file="DevelopmentMailboxConfigurationIntegrationTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests.Development;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for mailbox configuration in the Development environment that connect to real S3.
/// </summary>
[Trait("Category", "IntegrationTest")]
[Trait("Status", "Ready")]
public class DevelopmentMailboxConfigurationIntegrationTests
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentMailboxConfigurationIntegrationTests"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public DevelopmentMailboxConfigurationIntegrationTests(ITestOutputHelper output)
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
    /// Tests that the Development environment has the expected number of mailboxes.
    /// </summary>
    [S3IntegrationFact]
    public void DevelopmentEnvironmentHasExpectedMailboxCount()
    {
        // Arrange
        const int expectedCount = 1;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var developmentMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        int actualCount = developmentMailboxes.Count;

        // Assert
        actualCount.Should().Be(
            expectedCount,
            $"Expected {expectedCount} mailbox(es) in Development environment, but found {actualCount}. " +
            $"Mailboxes found: {string.Join(", ", developmentMailboxes.Select(m => m.MailboxId))}");
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
    /// Tests that the Development region has the expected number of mailboxes.
    /// </summary>
    [S3IntegrationFact]
    public void DevelopmentRegionHasExpectedMailboxCount()
    {
        // Arrange
        const int expectedCount = 1;
        const string region = "Testing development";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var regionMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        int actualCount = regionMailboxes.Count;

        // Assert
        actualCount.Should().Be(
            expectedCount,
            $"Expected {expectedCount} mailbox(es) in '{region}' region, but found {actualCount}. " +
            $"Mailboxes found: {string.Join(", ", regionMailboxes.Select(m => m.MailboxId))}");
    }

    /// <summary>
    /// Tests that the Development mailbox exists and has the correct properties.
    /// </summary>
    [S3IntegrationFact]
    public void DevelopmentMailboxHasCorrectProperties()
    {
        // Arrange
        const string emailAddress = "Email.Automation1@cps.gov.uk";
        const string mailboxId = "4e1a071c-f10f-43a5-8989-b26ba346dc48";
        const string expectedName = "CPS - Email Automation 1";
        const string expectedEnvironment = "Development";
        const string expectedRegion = "Testing development";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var mailbox = mailboxOptions.MailboxList
            .FirstOrDefault(m => m.MailboxId.Equals(mailboxId, StringComparison.OrdinalIgnoreCase) ||
                                 m.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

        // Assert
        mailbox.Should().NotBeNull($"Mailbox with ID '{mailboxId}' or email '{emailAddress}' should exist in Development environment");
        mailbox!.Name.Should().Be(expectedName, $"Mailbox name should be '{expectedName}'");
        mailbox.Environment.Should().Be(expectedEnvironment, $"Mailbox environment should be '{expectedEnvironment}'");
        mailbox.Region.Should().Be(expectedRegion, $"Mailbox region should be '{expectedRegion}'");
        mailbox.FullyAutomated.Should().BeTrue("Development mailbox should be fully automated");
        mailbox.ProceedsOfCrimeUrnSupport.Should().NotBeNull("ProceedsOfCrimeUrnSupport should be configured");
        mailbox.ProceedsOfCrimeUrnSupport!.Supported.Should().BeFalse("Development mailbox should not support Proceeds of Crime URN");
        mailbox.TreatZipAttachmentEmailsAsExceptions.Should().BeFalse("Development mailbox should not treat ZIP attachments as exceptions");
    }

    /// <summary>
    /// Tests that the Development mailbox can be found by both email address and GUID.
    /// </summary>
    [S3IntegrationFact]
    public void DevelopmentMailboxIsRecognizedAsSharedMailbox()
    {
        // Arrange
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        const string emailAddress = "Email.Automation1@cps.gov.uk";
        const string mailboxId = "4e1a071c-f10f-43a5-8989-b26ba346dc48";

        // Act & Assert
        mailboxOptions.IsSharedMailbox(emailAddress).Should().BeTrue(
            $"Mailbox with email address '{emailAddress}' should be recognized as a shared mailbox");
        
        mailboxOptions.IsSharedMailbox(mailboxId).Should().BeTrue(
            $"Mailbox with ID '{mailboxId}' should be recognized as a shared mailbox");
    }

    /// <summary>
    /// Tests that all Development mailboxes can be accessed via Microsoft Graph API.
    /// This proves that the app registration has the necessary permissions to access each mailbox.
    /// </summary>
    [GraphApiIntegrationFact]
    public async Task DevelopmentMailboxesCanBeAccessedViaGraphApi()
    {
        // Arrange
        const string folderName = "Inbox";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        IGraphApiEmailClient emailClient = this.serviceProvider.GetRequiredService<IGraphApiEmailClient>();

        // Act & Assert - Test access for Development mailboxes only
        var developmentMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        developmentMailboxes.Should().NotBeEmpty("At least one Development mailbox should be configured");

        foreach (var mailbox in developmentMailboxes)
        {
            string mailboxId = mailbox.MailboxId;
            string emailAddress = mailbox.EmailAddress;

            // Try to get the Inbox folder ID and read mailbox rules
            // This proves access to the mailbox (similar to what the scheduler does)
            string? folderId = null;
            try
            {
                folderId = await emailClient.GetMailboxFolderIdentityAsync(
                    mailboxId,
                    folderName,
                    default)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to access mailbox '{emailAddress}' (ID: '{mailboxId}') via Graph API. " +
                    $"This may indicate missing permissions or incorrect configuration. Error: {ex.Message}",
                    ex);
            }

            // Assert - If we can get the folder ID, we have basic access
            folderId.Should().NotBeNull(
                $"Should be able to get folder ID for '{folderName}' in mailbox '{emailAddress}' (ID: '{mailboxId}'). " +
                "This proves the app registration has access to the mailbox.");

            // Try to read mailbox rules (this is what the scheduler does to prove access)
            try
            {
                MessageRuleCollectionResponse? rules = await emailClient.GraphApiClient.GraphServiceClient
                    .Users[mailboxId]
                    .MailFolders[folderId]
                    .MessageRules
                    .GetAsync(cancellationToken: default)
                    .ConfigureAwait(false);

                // If we can read rules (even if empty), we have proven access
                rules.Should().NotBeNull(
                    $"Should be able to read mailbox rules for mailbox '{emailAddress}' (ID: '{mailboxId}'). " +
                    "This proves the app registration has the necessary permissions.");
            }
            catch (ODataError ex)
            {
                throw new InvalidOperationException(
                    $"Failed to read mailbox rules for mailbox '{emailAddress}' (ID: '{mailboxId}'). " +
                    $"This may indicate missing permissions. Error: {ex.Error?.Code} - {ex.Error?.Message}",
                    ex);
            }
        }
    }
}
