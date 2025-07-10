// <copyright file="IMailFolderSubscriptionManager.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cps.EasShared.ServiceClient.GraphApi;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Model;

/// <summary>
/// An interface for a change notification subscription manager to monitor email folders in a shared mailbox.
/// </summary>
public interface IMailFolderSubscriptionManager
{
    /// <summary>
    /// Gets the Graph API client associated with this subscription manager.
    /// </summary>
    IGraphApiClient GraphApiClient { get; }

    /// <summary>
    /// Creates a change notification subscription.
    /// </summary>
    /// <param name="subscriptionOptions">The subscription options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The newly created change notification subscription.</returns>
    Task<MailFolderSubscription> CreateMailFolderSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes change notification subscriptions for a mailbox.
    /// </summary>
    /// <param name="mailboxId">The user principal name for the mailbox.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task DeleteMailboxSubscriptionsAsync(
        string mailboxId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a change notification subscription.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the change notification subscription to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task DeleteSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that a single subscription is active per mailbox.
    /// </summary>
    /// <param name="subscriptionOptions">The subscription options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current change notification subscription.</returns>
    Task<MailFolderSubscription> EnsureSingleMailboxSubscriptionAsync(
        SubscriptionOptions subscriptionOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for subscriptions using the mail folder identifier.
    /// </summary>
    /// <param name="folderIdentifier">The mail folder identifier used to identify the subscription.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of change notification subscriptions or an empty list if none exist.</returns>
    /// <remarks>
    /// It is assumed that there will only ever be one subscription per mailbox folder.
    /// This method will help identify any additional subscriptions that may need to be deleted.
    /// </remarks>
    Task<IReadOnlyCollection<MailFolderSubscription>> FindMailFolderSubscriptionsAsync(
        IMailFolderIdentifier folderIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription by subscription ID.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the subscription to get.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The subscription if found; otherwise returns <c>null</c>.</returns>
    Task<MailFolderSubscription?> GetMailFolderSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews an existing change notification subscription.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the change notification subscription to renew.</param>
    /// <param name="expirationInterval">The expiration interval.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The renewed change notification subscription.</returns>
    Task<MailFolderSubscription> RenewMailFolderSubscriptionAsync(
        string subscriptionId,
        TimeSpan expirationInterval,
        CancellationToken cancellationToken = default);
}
