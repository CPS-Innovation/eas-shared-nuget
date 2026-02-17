// <copyright file="StagingMailboxConfigurationIntegrationTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests.Staging;

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
/// Integration tests for mailbox configuration in the Staging environment that connect to real S3.
/// </summary>
[Trait("Category", "IntegrationTest")]
[Trait("Status", "Ready")]
public class StagingMailboxConfigurationIntegrationTests
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="StagingMailboxConfigurationIntegrationTests"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public StagingMailboxConfigurationIntegrationTests(ITestOutputHelper output)
    {
        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Staging", false);
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
    /// Tests that the Staging environment has the expected number of mailboxes.
    /// </summary>
    [S3IntegrationFact]
    public void StagingEnvironmentHasExpectedMailboxCount()
    {
        // Arrange
        const int expectedCount = 2;
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var stagingMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Staging", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        int actualCount = stagingMailboxes.Count;

        // Assert
        actualCount.Should().Be(
            expectedCount,
            $"Expected {expectedCount} mailbox(es) in Staging environment, but found {actualCount}. " +
            $"Mailboxes found: {string.Join(", ", stagingMailboxes.Select(m => m.MailboxId))}");
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
    /// Tests that the Staging region has the expected number of mailboxes.
    /// </summary>
    [S3IntegrationFact]
    public void StagingRegionHasExpectedMailboxCount()
    {
        // Arrange
        const int expectedCount = 2;
        const string region = "Testing staging";
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
    /// Tests that the first Staging mailbox (Email.Automation2) exists and has the correct properties.
    /// </summary>
    [S3IntegrationFact]
    public void StagingMailbox2HasCorrectProperties()
    {
        // Arrange
        const string emailAddress = "Email.Automation2@cps.gov.uk";
        const string mailboxId = "ef0ef3d7-9373-4f61-91a9-37fe5982abef";
        const string expectedName = "CPS - Email Automation 2";
        const string expectedEnvironment = "Staging";
        const string expectedRegion = "Testing staging";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var mailbox = mailboxOptions.MailboxList
            .FirstOrDefault(m => m.MailboxId.Equals(mailboxId, StringComparison.OrdinalIgnoreCase) ||
                                 m.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

        // Assert
        mailbox.Should().NotBeNull($"Mailbox with ID '{mailboxId}' or email '{emailAddress}' should exist in Staging environment");
        mailbox!.Name.Should().Be(expectedName, $"Mailbox name should be '{expectedName}'");
        mailbox.Environment.Should().Be(expectedEnvironment, $"Mailbox environment should be '{expectedEnvironment}'");
        mailbox.Region.Should().Be(expectedRegion, $"Mailbox region should be '{expectedRegion}'");
        mailbox.FullyAutomated.Should().BeTrue("Staging mailbox should be fully automated");
        mailbox.DefaultParty.Should().Be("Defence", "Mailbox default party should be 'Defence'");
        mailbox.DefaultPartyDescription.Should().Be("DEF/CSL", "Mailbox default party description should be 'DEF/CSL'");

        // POC - Automation2 does not have Proceeds of Crime URN support
        mailboxOptions.RequiresProceedsOfCrimeUrnSupport(mailboxId).Should().BeFalse(
            "Email.Automation2 should not have Proceeds of Crime URN support enabled");

        // MSG format - Automation2 does not require emails in MSG file format
        mailboxOptions.RequiresEmailAsMsgFile(mailboxId).Should().BeFalse(
            "Email.Automation2 should not require emails in MSG file format");

        // Secure queue - Automation2 does not route to the secure queue
        mailboxOptions.RequiresSecureMailbox(mailboxId).Should().BeFalse(
            "Email.Automation2 should not route to the secure mailbox queue");

        // Auto-reply and other settings
        mailbox.DisableInvalidUrnAutoReply.Should().BeFalse("Email.Automation2 should not disable invalid URN auto-reply");
    }

    /// <summary>
    /// Tests that the second Staging mailbox (Email.Automation3) exists and has the correct properties.
    /// </summary>
    [S3IntegrationFact]
    public void StagingMailbox3HasCorrectProperties()
    {
        // Arrange
        const string emailAddress = "Email.Automation3@cps.gov.uk";
        const string mailboxId = "2e256642-b947-4da3-a987-0233f165eff4";
        const string expectedName = "CPS - Email Automation 3 POC test mailbox";
        const string expectedEnvironment = "Staging";
        const string expectedRegion = "Testing staging";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        var mailbox = mailboxOptions.MailboxList
            .FirstOrDefault(m => m.MailboxId.Equals(mailboxId, StringComparison.OrdinalIgnoreCase) ||
                                 m.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

        // Assert
        mailbox.Should().NotBeNull($"Mailbox with ID '{mailboxId}' or email '{emailAddress}' should exist in Staging environment");
        mailbox!.Name.Should().Be(expectedName, $"Mailbox name should be '{expectedName}'");
        mailbox.Environment.Should().Be(expectedEnvironment, $"Mailbox environment should be '{expectedEnvironment}'");
        mailbox.Region.Should().Be(expectedRegion, $"Mailbox region should be '{expectedRegion}'");
        mailbox.FullyAutomated.Should().BeTrue("Staging mailbox should be fully automated");
        mailbox.CreateCommunicationTask.Should().BeFalse("Mailbox should not create communication task");
        mailbox.MarkCommunicationAsRead.Should().BeTrue("Mailbox should mark communication as read");
        mailbox.DisableAddToFinalisedCase.Should().BeTrue("Mailbox should disable add to finalised case");
        mailbox.DummyUrnPrefix.Should().Be("87", "Mailbox dummy URN prefix should be '87'");
        mailbox.ProceedsOfCrimeUrnSupport.Should().NotBeNull("ProceedsOfCrimeUrnSupport should be configured");
        mailbox.ProceedsOfCrimeUrnSupport!.Supported.Should().BeTrue("Mailbox should support Proceeds of Crime URN");
        mailbox.ProceedsOfCrimeUrnSupport.AcceptOtherAreaUrns.Should().BeTrue("Mailbox should accept other area URNs");
        mailbox.EmailAsMsgFileSupport.Should().NotBeNull("EmailAsMsgFileSupport should be configured");
        mailbox.EmailAsMsgFileSupport!.Supported.Should().BeTrue("Mailbox should support email as MSG file");
        mailbox.EmailAsMsgFileSupport.IncludeAttachments.Should().BeTrue("Mailbox should include attachments in MSG file");
        mailbox.TreatZipAttachmentEmailsAsExceptions.Should().BeTrue("Mailbox should treat ZIP attachments as exceptions");
        mailbox.IgnoreAttachments.Should().BeTrue("Mailbox should ignore attachments");
        mailbox.IsSecureMailbox.Should().BeTrue("Mailbox should be a secure mailbox");

        // Auto-reply - Automation3 does not disable invalid URN auto-reply
        mailbox.DisableInvalidUrnAutoReply.Should().BeFalse("Email.Automation3 should not disable invalid URN auto-reply");
    }

    /// <summary>
    /// Tests that Staging mailboxes can be found by both email address and GUID.
    /// </summary>
    [S3IntegrationFact]
    public void StagingMailboxesAreRecognizedAsSharedMailboxes()
    {
        // Arrange
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        const string emailAddress2 = "Email.Automation2@cps.gov.uk";
        const string mailboxId2 = "ef0ef3d7-9373-4f61-91a9-37fe5982abef";
        const string emailAddress3 = "Email.Automation3@cps.gov.uk";
        const string mailboxId3 = "2e256642-b947-4da3-a987-0233f165eff4";

        // Act & Assert
        mailboxOptions.IsSharedMailbox(emailAddress2).Should().BeTrue(
            $"Mailbox with email address '{emailAddress2}' should be recognized as a shared mailbox");
        
        mailboxOptions.IsSharedMailbox(mailboxId2).Should().BeTrue(
            $"Mailbox with ID '{mailboxId2}' should be recognized as a shared mailbox");

        mailboxOptions.IsSharedMailbox(emailAddress3).Should().BeTrue(
            $"Mailbox with email address '{emailAddress3}' should be recognized as a shared mailbox");
        
        mailboxOptions.IsSharedMailbox(mailboxId3).Should().BeTrue(
            $"Mailbox with ID '{mailboxId3}' should be recognized as a shared mailbox");
    }

    /// <summary>
    /// Tests that all Staging mailboxes can be accessed via Microsoft Graph API.
    /// This proves that the app registration has the necessary permissions to access each mailbox.
    /// </summary>
    [GraphApiIntegrationFact]
    public async Task StagingMailboxesCanBeAccessedViaGraphApi()
    {
        // Arrange
        const string folderName = "Inbox";
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();
        IGraphApiEmailClient emailClient = this.serviceProvider.GetRequiredService<IGraphApiEmailClient>();

        // Act & Assert - Test access for Staging mailboxes only
        var stagingMailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals("Staging", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        stagingMailboxes.Should().NotBeEmpty("At least one Staging mailbox should be configured");

        foreach (var mailbox in stagingMailboxes)
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
