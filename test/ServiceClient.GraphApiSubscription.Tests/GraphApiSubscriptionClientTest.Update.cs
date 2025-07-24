// <copyright file="GraphApiSubscriptionClientTest.Update.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests;

using System;
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
    /// Test that one subscription is ensured after deleting all existing subscriptions.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    public async Task EnsureSingleMailboxSubscriptionSucceedsGivenNoPreExistingSubscriptions(string configSectionName)
    {
        // Assemble
        GraphApiIntegrationOptions integrationOptions = this.IntegrationOptions.Value;
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName))
        {
            ClientState = integrationOptions.WebhookClientState,
        };
        await this.DeleteAllExistingSubscriptions(mailBoxOptions).ConfigureAwait(true);

        // Act
        await this.Client.EnsureSingleMailboxSubscriptionAsync(mailBoxOptions).ConfigureAwait(true);

        // Assert
        IReadOnlyCollection<MailFolderSubscription> results = await this.Client.FindMailFolderSubscriptionsAsync(mailBoxOptions).ConfigureAwait(true);
        results.Should().HaveCount(1);
    }

    /// <summary>
    /// Test that after ensuring a subscription that the subscription is renewed.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    public async Task EnsureSingleMailboxSubscriptionSucceedsRenewingSubscription(string configSectionName)
    {
        // Assemble
        GraphApiIntegrationOptions integrationOptions = this.IntegrationOptions.Value;
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName))
        {
            ClientState = integrationOptions.WebhookClientState,
            ChangeType = "created,updated,deleted",
        };
        await this.DeleteAllExistingSubscriptions(mailBoxOptions).ConfigureAwait(true);

        // Act
        MailFolderSubscription sub1 = await this.Client.EnsureSingleMailboxSubscriptionAsync(mailBoxOptions).ConfigureAwait(true);
        double milliSeconds = 100.0 + (mailBoxOptions.ExpirationInterval.TotalMilliseconds / 3.0);
        await Task.Delay(TimeSpan.FromMilliseconds(milliSeconds)).ConfigureAwait(true);
        mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName))
        {
            ClientState = integrationOptions.WebhookClientState + "additional-text",
            ChangeType = "created",
        };
        MailFolderSubscription sub2 = await this.Client.EnsureSingleMailboxSubscriptionAsync(mailBoxOptions).ConfigureAwait(true);

        // Assert
        IReadOnlyCollection<MailFolderSubscription> results = await this.Client.FindMailFolderSubscriptionsAsync(mailBoxOptions).ConfigureAwait(true);
        results.Should().HaveCount(1);
        sub2.ExpirationDateTime.Should().BeAfter(sub1.ExpirationDateTime);
        sub2.ChangeType.Should().Be(mailBoxOptions.ChangeType);
        sub2.ClientState.Should().Be(mailBoxOptions.ClientState);
    }

    /// <summary>
    /// Test that one subscription is ensured after creating multiple subscriptions and calling <c>EnsureSingleSubscriptionAsync</c> multiple times concurrently.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1))]
    public async Task EnsureSingleMailboxSubscriptionSucceedsGivenMultipleSubscriptionsCreatedAsync(string configSectionName)
    {
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        // Assemble
        const int numSubscriptions = 8;
        GraphApiIntegrationOptions integrationOptions = this.IntegrationOptions.Value;
        SubscriptionOptions mailBoxOptions = this.SubscriptionOptions.Get(configSectionName);
        mailBoxOptions.ExpirationInterval = TimeSpan.FromSeconds(40);
        mailBoxOptions = new(mailBoxOptions)
        {
            ClientState = integrationOptions.WebhookClientState,
        };
        Parallel.For(0, numSubscriptions, i =>
        {
            string folderName = "Inbox";
            if (i % 2 == 0)
            {
                folderName = "CMS.Inbox";
            }

            this.InternalCreateSubscriptionForSharedMailBoxAsync(configSectionName, folderName).Wait();
        });

        // Act
        Parallel.For(0, numSubscriptions, i =>
        {
            this.Client.EnsureSingleMailboxSubscriptionAsync(mailBoxOptions).Wait();
        });

        // Assert
        IReadOnlyCollection<MailFolderSubscription> results = await this.Client.FindMailFolderSubscriptionsAsync(mailBoxOptions).ConfigureAwait(true);
        results.Should().HaveCount(1);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
    }

    /// <summary>
    /// Tests for invalid <c>ClientState</c>.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <param name="clientState">The client state.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox1), "1")]
    [InlineData(nameof(SharedMailBox1), "12345678901")]
    [InlineData(nameof(SharedMailBox1), "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task EnsureSingleMailboxSubscriptionFailsForInvalidClientState(string configSectionName, string clientState)
    {
        // Assemble
        SubscriptionOptions mailBoxOptions = new(this.SubscriptionOptions.Get(configSectionName))
        {
            ClientState = clientState,
        };

        // Act
        ArgumentOutOfRangeException exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await this.Client.EnsureSingleMailboxSubscriptionAsync(mailBoxOptions).ConfigureAwait(true);
        })
            .ConfigureAwait(true);

        // Assert
        exception.Message.Should().Be("Expecting a string of between 12 and 128 characters in length. (Parameter 'ClientState')");
    }

    /// <summary>
    /// Tests the renewal of a change notification subscription.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section for the shared mailbox.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData(nameof(SharedMailBox2))]
    public async Task RenewSubscriptionForSharedMailBoxAsync(string configSectionName)
    {
        // Assemble
        await this.CreateSubscriptionForSharedMailBoxAsync(configSectionName).ConfigureAwait(true);
        Assumes.NotNull(this.SubscriptionId);
        MailFolderSubscription? subscriptionCreated = await this.GetSubscriptionAsync().ConfigureAwait(true);
        subscriptionCreated.Should().NotBeNull();
        Assumes.NotNull(subscriptionCreated);

        // Act
        await this.Client.RenewMailFolderSubscriptionAsync(this.SubscriptionId, TimeSpan.FromSeconds(10)).ConfigureAwait(true);

        // Assert
        MailFolderSubscription? subscriptionRenewed = await this.GetSubscriptionAsync().ConfigureAwait(true);
        subscriptionRenewed.Should().NotBeNull();
        Assumes.NotNull(subscriptionRenewed);
        subscriptionRenewed.ExpirationDateTime.Should().BeAfter(subscriptionCreated.ExpirationDateTime);
    }
}
