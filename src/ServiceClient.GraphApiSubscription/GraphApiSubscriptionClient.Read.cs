// <copyright file="GraphApiSubscriptionClient.Read.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Extensions;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// A client that can connect to the Graph API and manage change notification subscriptions.
/// </summary>
public partial class GraphApiSubscriptionClient :
    IMailFolderSubscriptionManager
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<MailFolderSubscription>> FindMailFolderSubscriptionsAsync(
        IMailFolderIdentifier folderIdentifier,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(folderIdentifier);

        MailFolderResource folderResource = await this.GetFolderResourceAsync(folderIdentifier, cancellationToken).ConfigureAwait(false);
        return await this.FindMailFolderSubscriptionsAsync(folderResource, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches for subscriptions using the mail folder Graph API resource.
    /// </summary>
    /// <param name="mailFolderResource">The mail folder resource.</param>
    /// <param name="results">The results found that matched.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total number of subscriptions and not the number of subscriptions found. This is useful for diagnostic tracing.</returns>
    public async Task<int> FindSubscriptionsAsync(
        MailFolderResource mailFolderResource,
        IList<MailFolderSubscription> results,
        CancellationToken cancellationToken = default)
    {
        return await this.FindSubscriptionsAsync(mailFolderResource.Resource, IsMailFolderSubscription, results, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches for subscriptions using a subscription matching function.
    /// </summary>
    /// <param name="identifier">An identifier used for matching the subscription.</param>
    /// <param name="isSubscriptionMatch">The subscription matching function.</param>
    /// <param name="results">The results found that matched.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The total number of subscriptions and not the number of subscriptions found.
    /// This is useful for diagnostic tracing.
    /// </returns>
    public virtual async Task<int> FindSubscriptionsAsync(
        string identifier,
        Func<string, Subscription, bool> isSubscriptionMatch,
        IList<MailFolderSubscription> results,
        CancellationToken cancellationToken)
    {
        int count = 0;
        SubscriptionCollectionResponse? firstPage = null;
        try
        {
            firstPage = await this.GraphApiClient.GraphServiceClient
                .Subscriptions
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }

        if (firstPage is not null)
        {
            var pageIterator = PageIterator<Subscription, SubscriptionCollectionResponse>.CreatePageIterator(
                    client: this.GraphApiClient.GraphServiceClient,
                    page: firstPage,
                    callback: subscription =>
                    {
                        count++;
                        if (isSubscriptionMatch(identifier, subscription))
                        {
                            results.Add(subscription.ToMailFolderSubscription());
                        }

                        return true;
                    });

            try
            {
                await pageIterator.IterateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ODataError ex)
            {
                this.GraphApiClient.LogError(ex);
                throw;
            }
        }

        return count;
    }

    /// <inheritdoc/>
    public async Task<MailFolderSubscription?> GetMailFolderSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        MailFolderSubscription? result = null;
        try
        {
            Subscription subscription = await this.GetSubscriptionAsync(subscriptionId, cancellationToken).ConfigureAwait(false);
            result = subscription.ToMailFolderSubscription();
        }
        catch (ODataError exception)
        {
            if (exception.ResponseStatusCode != (int)System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a subscription by subscription ID remotely using the Graph API.
    /// </summary>
    /// <param name="subscriptionId">The globally unique identifier for the subscription to get.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The subscription if found; otherwise throws a not found exception.</returns>
    public virtual async Task<Subscription> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return (await this.GraphApiClient.GraphServiceClient.Subscriptions[subscriptionId].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Determines if the <paramref name="subscription"/> is a match for <paramref name="mailFolderResource"/>.
    /// </summary>
    /// <param name="mailFolderResource">The mail folder resource.</param>
    /// <param name="subscription">The subscription to evaluate.</param>
    /// <returns>Returns <c>true</c> if matched; otherwise returns <c>false</c>.</returns>
    protected static bool IsMailFolderSubscription(
        string mailFolderResource,
        Subscription subscription)
    {
        bool found = subscription.Resource!.Equals(mailFolderResource, StringComparison.OrdinalIgnoreCase);
        return found;
    }

    /// <summary>
    /// Identifies subscriptions for any other folders being monitored for the same mailbox.
    /// </summary>
    /// <param name="mailFolderResource">The mail folder resource that we want to monitor.</param>
    /// <param name="subscription">The subscription to evaluate.</param>
    /// <param name="unwantedSubscriptions">The unwanted subscription list.</param>
    /// <returns>Always returns <c>true</c>.</returns>
    /// <remarks>If we switch monitoring from say 'CMS.Inbox' to 'Inbox', this will help identify the subscription on 'CMS.Inbox' so that it can be deleted.</remarks>
    protected static bool IdentifySubscriptionsForOtherFolders(
        MailFolderResource mailFolderResource,
        Subscription subscription,
        IList<MailFolderSubscription> unwantedSubscriptions)
    {
        if (MailFolderResource.TryParse(subscription.Resource!, out MailFolderResource? subscriptionResource))
        {
            if (subscriptionResource.MailboxId.Equals(mailFolderResource.MailboxId, StringComparison.OrdinalIgnoreCase) &&
                !subscriptionResource.FolderId.Equals(mailFolderResource.FolderId, StringComparison.OrdinalIgnoreCase))
            {
                unwantedSubscriptions.Add(subscription.ToMailFolderSubscription());
            }
        }

        return true;
    }

    /// <summary>
    /// Finds subscriptions to other folders that do not match the specified <paramref name="mailFolderResource"/>.
    /// </summary>
    /// <param name="mailFolderResource">The mail folder resource we want to monitor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The subscriptions to other folders that we want to delete.</returns>
    protected virtual async Task<IList<MailFolderSubscription>> FindMailboxSubscriptionsForOtherFoldersAsync(
        MailFolderResource mailFolderResource,
        CancellationToken cancellationToken = default)
    {
        List<MailFolderSubscription> results = new();
        SubscriptionCollectionResponse? firstPage = null;
        try
        {
            firstPage = await this.GraphApiClient.GraphServiceClient
                .Subscriptions
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }

        if (firstPage is not null)
        {
            var pageIterator = PageIterator<Subscription, SubscriptionCollectionResponse>.CreatePageIterator(
                client: this.GraphApiClient.GraphServiceClient,
                page: firstPage,
                callback: subscription =>
                {
                    return IdentifySubscriptionsForOtherFolders(mailFolderResource, subscription, results);
                });

            try
            {
                await pageIterator.IterateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ODataError ex)
            {
                this.GraphApiClient.LogError(ex);
                throw;
            }
        }

        return results;
    }

    /// <summary>
    /// Logs a find subscriptions event.
    /// </summary>
    /// <param name="watch">The stopwatch.</param>
    /// <param name="allFolderSubscriptionsCount">The total number of subscriptions for all mail folders.</param>
    /// <param name="results">The subscriptions found.</param>
    /// <param name="logger">The logger.</param>
    private static void LogFindSubscriptionsEvent(
        Stopwatch watch,
        int allFolderSubscriptionsCount,
        IReadOnlyCollection<MailFolderSubscription> results,
        ILogger logger)
    {
        logger.LogInformation(
            "Graph API found {FoundCount} subscription(s) for the folder and {SubscriptionCount} subscription(s) for all folders at {EventTime}\n - Duration: {Duration}",
            results.Count,
            allFolderSubscriptionsCount,
            TimeUtility.GetUKLocalTime(DateTimeOffset.UtcNow),
            watch.Elapsed);
    }

    private async Task<IReadOnlyCollection<MailFolderSubscription>> FindMailFolderSubscriptionsAsync(
        MailFolderResource folderResource,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(folderResource);

        var watch = Stopwatch.StartNew();
        List<MailFolderSubscription> results = new();

        // Returns the total number of active subscriptions for all folders.
        int totalSubscriptions = await this.FindSubscriptionsAsync(folderResource, results, cancellationToken).ConfigureAwait(false);

        IOrderedEnumerable<MailFolderSubscription> sorted = results.OrderByDescending(s => s.ExpirationDateTime);

        LogFindSubscriptionsEvent(watch, totalSubscriptions, results, this.logger);
        return sorted.ToImmutableArray();
    }
}
