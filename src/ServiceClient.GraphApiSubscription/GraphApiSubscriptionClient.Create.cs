// <copyright file="GraphApiSubscriptionClient.Create.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription;

using System;
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
    public async Task<MailFolderSubscription> CreateMailFolderSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken = default)
    {
        ValidateSubscriptionOptions(subscriptionOptions);

        var watch = Stopwatch.StartNew();

        MailFolderResource folderResource = await this.GetFolderResourceAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);

        Subscription subscription = new()
        {
            ChangeType = subscriptionOptions.ChangeType,
            NotificationUrl = subscriptionOptions.NotificationUrl,
            Resource = folderResource.Resource,
            ExpirationDateTime = DateTimeOffset.UtcNow.Add(subscriptionOptions.ExpirationInterval),
            ClientState = subscriptionOptions.ClientState,
            LifecycleNotificationUrl = subscriptionOptions.LifecycleNotificationUrl,
        };

        Subscription newSubscription = await this.CreateSubscriptionAsync(subscription, cancellationToken).ConfigureAwait(false);

        var result = newSubscription.ToMailFolderSubscription();
        LogCreatedEvent(watch, result, folderResource.MailboxId, subscriptionOptions.FolderName, this.logger);
        return result;
    }

    /// <summary>
    /// Creates a change notification subscription using the Graph API.
    /// </summary>
    /// <param name="subscription">A Graph API subscription.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The newly created change notification subscription.</returns>
    public virtual async Task<Subscription> CreateSubscriptionAsync(
        Subscription subscription,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return (await this.GraphApiClient.GraphServiceClient.Subscriptions.PostAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }
    }

    private static void LogCreatedEvent(
        Stopwatch watch,
        MailFolderSubscription created,
        string mailbox,
        string folder,
        ILogger logger)
    {
        const string LoggerMessage = @"Milestone: Graph API subscription {SubscriptionId} created for mailbox {MailboxId} and folder {Folder}:
 - EventTime: {EventTime}
 - Change type: {ChangeType}
 - URL: {NotificationUrl}
 - Lifecycle URL: {LifecycleNotificationUrl}
 - Resource: {Resource}
 - Expiration: {ExpirationTime}
 - Duration: {Duration}";
        logger.LogInformation(
            LoggerMessage,
            created.SubscriptionId,
            mailbox,
            folder,
            TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
            created.ChangeType,
            created.NotificationUrl,
            created.LifecycleNotificationUrl,
            created.Resource,
            TimeUtility.GetUKLocalTime(created.ExpirationDateTime),
            watch.Elapsed);
    }
}
