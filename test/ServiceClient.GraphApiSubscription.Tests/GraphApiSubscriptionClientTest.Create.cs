// <copyright file="GraphApiSubscriptionClientTest.Create.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests;

using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;

/// <summary>
/// Test connectivity with the Graph API and the management of notification change subscriptions.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiSubscriptionClientTest
{
    /// <summary>
    /// Tests the creation of a change notification subscription to a shared mailbox.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    [InlineData(nameof(SharedMailBox2))]
    public async Task CreateSubscriptionForSharedMailBoxAsync(string configSectionName)
    {
        await this.InternalCreateSubscriptionForSharedMailBoxAsync(configSectionName, "Inbox").ConfigureAwait(true);
    }

    private async Task InternalCreateSubscriptionForSharedMailBoxAsync(string configSectionName, string folderName)
    {
        // Assemble
        GraphApiIntegrationOptions integrationOptions = this.IntegrationOptions.Value;
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName))
        {
            ClientState = integrationOptions.WebhookClientState,
            FolderName = folderName,
        };

        mailBoxOptions.Should().NotBeNull();
        this.SubscriptionId = null;

        // Act
        MailFolderSubscription subscription = await this.Client.CreateMailFolderSubscriptionAsync(mailBoxOptions).ConfigureAwait(true);

        // Assert
        this.SubscriptionId = subscription.SubscriptionId;
        this.SubscriptionId.Should().NotBeNullOrWhiteSpace();
    }
}
