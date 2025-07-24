// <copyright file="GraphApiSubscriptionClientTest.Read.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests;

using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Configuration;
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
    /// Tests the finding of change notification subscriptions using resource only as search criteria.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    [InlineData(nameof(SharedMailBox2))]
    public async Task FindSubscriptionsForSharedMailBoxGivenResourceAsync(string configSectionName)
    {
        // Assemble
        await this.CreateSubscriptionForSharedMailBoxAsync(configSectionName).ConfigureAwait(true);
        Assumes.NotNull(this.SubscriptionId);
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName));
        MailFolderResource mailFolderResource = await this.Client.GetFolderResourceAsync(mailBoxOptions).ConfigureAwait(true);

        // Act
        IReadOnlyCollection<MailFolderSubscription> results = await this.Client.FindMailFolderSubscriptionsAsync(mailBoxOptions).ConfigureAwait(true);

        // Assert
        results.Should().NotBeNullOrEmpty();
        foreach (MailFolderSubscription item in results)
        {
            item.Resource.Should().Be(mailFolderResource.Resource);
        }
    }

    private async Task<MailFolderSubscription?> GetSubscriptionAsync()
    {
        Assumes.NotNull(this.SubscriptionId);
        return await this.Client.GetMailFolderSubscriptionAsync(this.SubscriptionId).ConfigureAwait(true);
    }
}
