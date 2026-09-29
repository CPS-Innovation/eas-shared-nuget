// <copyright file="GraphApiSubscriptionClientTest.Delete.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests;

using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft;

/// <summary>
/// Test connectivity with the Graph API and the management of notification change subscriptions.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiSubscriptionClientTest
{
    /// <summary>
    /// Tests the deletion of a single subscription.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    public async Task DeleteSubscriptionSucceedsAsync(string configSectionName)
    {
        // Assemble
        await this.CreateSubscriptionForSharedMailBoxAsync(configSectionName).ConfigureAwait(true);
        Assumes.NotNull(this.SubscriptionId);

        // Act
        await this.Client.DeleteSubscriptionAsync(this.SubscriptionId).ConfigureAwait(true);

        // Assert
        MailFolderSubscription? subscription = await this.GetSubscriptionAsync().ConfigureAwait(true);
        subscription.Should().BeNull();
    }

    /// <summary>
    /// Tests the deletion of all subscriptions for a mailbox.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox2))]
    public async Task DeleteSubscriptionsForMailboxSucceedsAsync(string configSectionName)
    {
        // Assemble
        await this.CreateSubscriptionForSharedMailBoxAsync(configSectionName).ConfigureAwait(true);
        Assumes.NotNull(this.SubscriptionId);
        await this.CreateSubscriptionForSharedMailBoxAsync(configSectionName).ConfigureAwait(true);
        Assumes.NotNull(this.SubscriptionId);
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName));

        // Act
        await this.Client.DeleteMailboxSubscriptionsAsync(mailBoxOptions.MailboxId).ConfigureAwait(true);

        // Assert
        IReadOnlyCollection<MailFolderSubscription> subscriptions = await this.Client.FindMailFolderSubscriptionsAsync(mailBoxOptions).ConfigureAwait(true);
        subscriptions.Should().BeEmpty();
    }

    private async Task DeleteAllExistingSubscriptions(IMailFolderIdentifier folderIdentifier)
    {
        await this.Client.DeleteMailboxSubscriptionsAsync(folderIdentifier.MailboxId).ConfigureAwait(true);
    }
}
