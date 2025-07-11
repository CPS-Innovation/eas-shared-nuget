// <copyright file="GraphApiSubscriptionClient.Update.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Extensions;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft.Extensions.Logging;

/// <summary>
/// A client that can connect to the Graph API and manage change notification subscriptions.
/// </summary>
public partial class GraphApiSubscriptionClient :
    IMailFolderSubscriptionManager
{
    /// <inheritdoc/>
    public async Task<MailFolderSubscription> EnsureSingleMailboxSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken = default)
    {
        ValidateSubscriptionOptions(subscriptionOptions);
        MailFolderResource folderResource = await this.GetFolderResourceAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);
        await this.DeleteMailboxSubscriptionsForOtherFoldersAsync(folderResource, cancellationToken).ConfigureAwait(false);

        return await this.EnsureSingleMailFolderSubscriptionAsync(subscriptionOptions, folderResource, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<MailFolderSubscription> RenewMailFolderSubscriptionAsync(
        string subscriptionId,
        TimeSpan expirationInterval,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(subscriptionId);
        Requires.NotDefault(expirationInterval);

        var watch = Stopwatch.StartNew();

        Subscription subscription = new()
        {
            ExpirationDateTime = DateTimeOffset.UtcNow + expirationInterval,
        };

        subscription = await this.RenewSubscriptionAsync(subscriptionId, subscription, cancellationToken).ConfigureAwait(false);

        var updated = subscription.ToMailFolderSubscription();

        LogRenewalEvent(watch, updated, this.logger);

        return updated;
    }

    /// <summary>
    /// Renews a subscription.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the change notification subscription to renew.</param>
    /// <param name="subscription">The subscription with updated properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated subscription.</returns>
    public virtual async Task<Subscription> RenewSubscriptionAsync(
        string subscriptionId,
        Subscription subscription,
        CancellationToken cancellationToken)
    {
        try
        {
            return (await this.GraphApiClient.GraphServiceClient
                .Subscriptions[subscriptionId]
                .PatchAsync(subscription, cancellationToken: cancellationToken)
                .ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }
    }

    private static void LogRenewalEvent(
        Stopwatch watch,
        MailFolderSubscription updated,
        ILogger logger)
    {
        string? mailboxId = null;
        if (MailFolderResource.TryParse(updated.Resource, out MailFolderResource? mailFolderResource))
        {
            mailboxId = mailFolderResource.MailboxId;
        }

        mailboxId ??= "Unknown";

        const string InfoMessage = @"Milestone: Graph API subscription {SubscriptionId} for mailbox {MailboxId} renewed at {EventTime}:
 - Change type: {ChangeType}
 - URL: {NotificationUrl}
 - Lifecycle URL: {LifecycleNotificationUrl}
 - Resource: {Resource}
 - Expiration: {ExpirationTime}
 - Duration: {Duration}";

        logger.LogInformation(
           InfoMessage,
           updated.SubscriptionId,
           mailboxId,
           TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
           updated.ChangeType,
           updated.NotificationUrl,
           updated.LifecycleNotificationUrl,
           updated.Resource,
           TimeUtility.GetUKLocalTime(updated.ExpirationDateTime),
           watch.Elapsed);
    }

    private async Task<MailFolderSubscription> UpdateSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        MailFolderSubscription result,
        CancellationToken cancellationToken)
    {
        TimeSpan? expirationInterval = null;

        var threshold = TimeSpan.FromMilliseconds(2 * subscriptionOptions.ExpirationInterval.TotalMilliseconds / 3);
        TimeSpan timeLeft = result.ExpirationDateTime - DateTimeOffset.UtcNow;
        if (threshold > timeLeft)
        {
            // Renewed when more than a 1/3 of subscriptionOptions.ExpirationInterval has elapsed
            expirationInterval = subscriptionOptions.ExpirationInterval;
        }

        bool requiresNewSubscription = !result.ChangeType.Equals(subscriptionOptions.ChangeType, StringComparison.OrdinalIgnoreCase);
        if (!requiresNewSubscription)
        {
            requiresNewSubscription = result.ClientState is not null && !result.ClientState.Equals(subscriptionOptions.ClientState, StringComparison.Ordinal);
        }

        if (!requiresNewSubscription)
        {
            requiresNewSubscription = result.NotificationUrl != subscriptionOptions.NotificationUrl;
        }

        if (!requiresNewSubscription)
        {
            requiresNewSubscription = result.LifecycleNotificationUrl != subscriptionOptions.LifecycleNotificationUrl;
        }

        if (requiresNewSubscription)
        {
            await this.DeleteGraphAPIChangeNotificationSubscriptionAsync(result.SubscriptionId, cancellationToken).ConfigureAwait(false);
            MailFolderSubscription newSubscription = await this.CreateMailFolderSubscriptionAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);
            result = newSubscription;
        }
        else if (expirationInterval.HasValue)
        {
            result = await this.RenewMailFolderSubscriptionAsync(result.SubscriptionId, expirationInterval.Value, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private void LogEnsureSingleSubscriptionConclusion(Stopwatch watch, int count, MailFolderSubscription result, SubscriptionOptions subscriptionOptions)
    {
        const string InfoMessage = @"Graph API subscription {SubscriptionId} for mailbox {Mailbox} and folder {Folder} ensured:
 - EventTime: {EventTime}
 - Change type: {ChangeType}
 - URL: {NotificationUrl}
 - Lifecycle URL: {LifecycleNotificationUrl}
 - Resource: {Resource}
 - Expiration: {Expiration}
 - Duration: {Duration}";

        if (count > 1)
        {
            /*
             *  We shouldn't get more than one created, but it can happen when multiple
             *  schedulers create one at the same time.
             */
            const string WarningMessage = DiagnosticsUtility.Warning + InfoMessage + @"
 - Found more than one: {Count}";
            this.logger.LogWarning(
                WarningMessage,
                result.SubscriptionId,
                subscriptionOptions.MailboxId,
                subscriptionOptions.FolderName,
                TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
                result.ChangeType,
                result.NotificationUrl,
                result.LifecycleNotificationUrl,
                result.Resource,
                TimeUtility.GetUKLocalTime(result.ExpirationDateTime),
                watch.Elapsed,
                count);
        }
        else
        {
            this.logger.LogInformation(
                InfoMessage,
                result.SubscriptionId,
                subscriptionOptions.MailboxId,
                subscriptionOptions.FolderName,
                TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
                result.ChangeType,
                result.NotificationUrl,
                result.LifecycleNotificationUrl,
                result.Resource,
                TimeUtility.GetUKLocalTime(result.ExpirationDateTime),
                watch.Elapsed);
        }
    }

    private async Task<MailFolderSubscription> EnsureSingleMailFolderSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        MailFolderResource mailFolderResource,
        CancellationToken cancellationToken = default)
    {
        ValidateSubscriptionOptions(subscriptionOptions);

        var watch = Stopwatch.StartNew();

        IReadOnlyCollection<MailFolderSubscription> subscriptions = await this.FindMailFolderSubscriptionsAsync(
            mailFolderResource,
            cancellationToken)
                .ConfigureAwait(false);

        int count = 0;
        MailFolderSubscription? result = null;
        if (subscriptions.Any())
        {
            foreach (MailFolderSubscription subscription in subscriptions)
            {
                if (count == 0)
                {
                    result = subscription;
                }
                else
                {
                    await this.DeleteSubscriptionAsync(subscription, cancellationToken).ConfigureAwait(false);
                }

                count++;
            }
        }

        if (result is null)
        {
            result = await this.CreateMailFolderSubscriptionAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            result = await this.UpdateSubscriptionAsync(subscriptionOptions, result, cancellationToken).ConfigureAwait(false);
        }

        this.LogEnsureSingleSubscriptionConclusion(watch, count, result, subscriptionOptions);

        return result;
    }
}
