// <copyright file="GraphApiSubscriptionClient.Delete.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cps.Fct.Eas.Core;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Extensions;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// A client that can connect to the Graph API and manage change notification subscriptions.
/// </summary>
public partial class GraphApiSubscriptionClient :
    IMailFolderSubscriptionManager
{
    /// <inheritdoc/>
    public async Task DeleteMailboxSubscriptionsAsync(string mailboxId, CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);

        static bool IsMailboxMatch(string mailboxId, Subscription subscription)
        {
            Requires.NotNull(subscription);

            return subscription.Resource!.Contains(Uri.EscapeDataString(mailboxId), StringComparison.OrdinalIgnoreCase);
        }

        List<MailFolderSubscription> subscriptions = new();
        await this.FindSubscriptionsAsync(mailboxId, IsMailboxMatch, subscriptions, cancellationToken).ConfigureAwait(false);
        int deletedCount = 0;
        foreach (MailFolderSubscription subscription in subscriptions)
        {
            await this.DeleteSubscriptionAsync(subscription.SubscriptionId, cancellationToken).ConfigureAwait(false);
            deletedCount++;
        }

        if (deletedCount == 0)
        {
            this.logger.LogInformation("No subscriptions needed to be deleted for mailbox {MailboxId}. Monitoring of mailbox is disabled.", mailboxId);
        }
        else
        {
            this.logger.LogInformation(
                "Milestone: monitoring of mailbox {MailboxId} is now disabled after deleting {DeletedCount} subscription(s)",
                mailboxId,
                deletedCount);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(subscriptionId);

        var watch = Stopwatch.StartNew();

        Subscription subscription = await this.GetSubscriptionAsync(subscriptionId, cancellationToken).ConfigureAwait(false);

        var deleted = subscription.ToMailFolderSubscription();

        await this.DeleteGraphAPIChangeNotificationSubscriptionAsync(subscriptionId, cancellationToken).ConfigureAwait(false);

        LogDeletedEvent(watch, deleted, this.logger);
    }

    /// <summary>
    /// Deletes a change notification subscription remotely using the Graph API.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the change notification subscription to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    public virtual async Task DeleteGraphAPIChangeNotificationSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await this.GraphApiClient.GraphServiceClient.Subscriptions[subscriptionId].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Logs a subscription deleted event.
    /// </summary>
    /// <param name="watch">The stopwatch.</param>
    /// <param name="deleted">The subscription deleted.</param>
    /// <param name="logger">The logger.</param>
    private static void LogDeletedEvent(
        Stopwatch watch,
        MailFolderSubscription deleted,
        ILogger logger)
    {
        const string InfoMessage = @"Graph API subscription deleted at {EventTime}:
 - Id: {SubscriptionId}
 - Change type: {ChangeType}
 - URL: {NotificationUrl}
 - Lifecycle URL: {LifecycleNotificationUrl}
 - Resource: {Resource}
 - Expiration: {ExpirationTime}
 - Duration: {Duration}";
        logger.LogInformation(
            InfoMessage,
            TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
            deleted.SubscriptionId,
            deleted.ChangeType,
            deleted.NotificationUrl,
            deleted.LifecycleNotificationUrl,
            deleted.Resource,
            TimeUtility.GetUKLocalTime(deleted.ExpirationDateTime),
            watch.Elapsed);
    }

    private async Task DeleteSubscriptionAsync(
        MailFolderSubscription subscription,
        CancellationToken cancellationToken)
    {
        try
        {
            await this.DeleteSubscriptionAsync(subscription.SubscriptionId, cancellationToken).ConfigureAwait(false);
        }
        catch (ODataError exception)
        {
            if (exception.ResponseStatusCode != (int)System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }
    }

    private async Task DeleteMailboxSubscriptionsForOtherFoldersAsync(MailFolderResource folderResource, CancellationToken cancellationToken)
    {
        IList<MailFolderSubscription> unwanted = await this.FindMailboxSubscriptionsForOtherFoldersAsync(folderResource, cancellationToken).ConfigureAwait(false);
        if (unwanted.Any())
        {
            foreach (MailFolderSubscription subscription in unwanted)
            {
                await this.DeleteGraphAPIChangeNotificationSubscriptionAsync(subscription.SubscriptionId, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation(
                    "Milestone: deleted unwanted subscription {SubscriptionId} for mailbox {MailboxId}.\n - Resource: {Resource}",
                    subscription.SubscriptionId,
                    folderResource.MailboxId,
                    subscription.Resource);
            }
        }
    }
}
