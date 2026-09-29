// <copyright file="FakeGraphApiSubscriptionClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Emulation;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Extensions;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// A fake version of the <see cref="GraphApiSubscriptionClient"/> that can be unit tested.
/// </summary>
public class FakeGraphApiSubscriptionClient : GraphApiSubscriptionClient
{
    private static readonly ConcurrentDictionary<string, MailFolderSubscription> SubscriptionEntities = new();
    private static readonly Dictionary<string, string> FolderCache = new()
    {
        { "Inbox", "Inbox" },
        { "CMS.Exceptions", "CMS.Exceptions" },
        { "CMS.Excluded", "CMS.Excluded" },
        { "CMS.Inbox", "CMS.Inbox" },
        { "CMS.Processed", "CMS.Processed" },
        { "CMS.Processing", "CMS.Processing" },
        { "CMS.Urgent", "CMS.Urgent" },
    };

    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeGraphApiSubscriptionClient"/> class.
    /// </summary>
    /// <param name="graphApiClient">The Graph API client.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public FakeGraphApiSubscriptionClient(
        IGraphApiClient graphApiClient,
        ILoggerFactory loggerFactory)
        : base(
            graphApiClient,
            loggerFactory)
    {
        Requires.NotNull(loggerFactory);

        this.logger = loggerFactory.CreateLogger<FakeGraphApiSubscriptionClient>();
    }

    /// <inheritdoc/>
    public override Task<Subscription> CreateSubscriptionAsync(
        Subscription subscription,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(subscription);

        this.RemoveExpiredSubscriptions();

        subscription.Id ??= Guid.NewGuid().ToString();

        var mailFolderSubscription = subscription.ToMailFolderSubscription();
        SubscriptionEntities.TryAdd(mailFolderSubscription.SubscriptionId, mailFolderSubscription);

        return Task.FromResult(subscription);
    }

    /// <inheritdoc/>
    public override Task DeleteGraphAPIChangeNotificationSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(subscriptionId);

        this.RemoveExpiredSubscriptions();

        if (!SubscriptionEntities.TryRemove(subscriptionId, out MailFolderSubscription? _))
        {
            throw CreateNotFoundException();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task<int> FindSubscriptionsAsync(
        string identifier,
        Func<string, Subscription, bool> isSubscriptionMatch,
        IList<MailFolderSubscription> results,
        CancellationToken cancellationToken)
    {
        Requires.NotNullOrWhiteSpace(identifier);

        try
        {
            await base.FindSubscriptionsAsync(identifier, isSubscriptionMatch, results, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }

        this.RemoveExpiredSubscriptions();

        IEnumerable<MailFolderSubscription> selected = SubscriptionEntities.Values
            .Where(s => isSubscriptionMatch(identifier, s.ToSubscription()));

        foreach (MailFolderSubscription result in selected)
        {
            results.Add(result);
        }

        return SubscriptionEntities.Count;
    }

    /// <inheritdoc/>
    public override async Task<MailFolderResource> GetFolderResourceAsync(
        IMailFolderIdentifier folderIdentifier,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.GetFolderResourceAsync(folderIdentifier, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }

        if (FolderCache.TryGetValue(folderIdentifier.FolderName, out string? folderId))
        {
            return new MailFolderResource(folderIdentifier.MailboxId, $"{folderIdentifier.MailboxId}-{folderId}");
        }

        throw CreateFolderNotFoundException(folderIdentifier);
    }

    /// <inheritdoc/>
    public override Task<Subscription> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(subscriptionId);

        this.RemoveExpiredSubscriptions();

        Subscription? result = null;
        if (SubscriptionEntities.TryGetValue(subscriptionId, out MailFolderSubscription? subscription))
        {
            result = subscription.ToSubscription();
        }

        if (result is null)
        {
            throw CreateNotFoundException();
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public override Task<Subscription> RenewSubscriptionAsync(
        string subscriptionId,
        Subscription subscription,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(subscriptionId);
        Requires.NotNull(subscription);

        this.RemoveExpiredSubscriptions();

        MailFolderSubscription? result = null;
        if (SubscriptionEntities.TryGetValue(subscriptionId, out MailFolderSubscription? entity))
        {
            result = new MailFolderSubscription(
                ChangeType: entity.ChangeType,
                ClientState: entity.ClientState,
                ExpirationDateTime: subscription.ExpirationDateTime!.Value,
                LifecycleNotificationUrl: entity.LifecycleNotificationUrl,
                NotificationUrl: entity.NotificationUrl,
                Resource: entity.Resource,
                SubscriptionId: subscriptionId);
            SubscriptionEntities.TryUpdate(subscriptionId, result, entity);
        }

        if (result is null)
        {
            throw CreateNotFoundException();
        }

        return Task.FromResult(result.ToSubscription());
    }

    /// <inheritdoc/>
    protected override async Task<IList<MailFolderSubscription>> FindMailboxSubscriptionsForOtherFoldersAsync(MailFolderResource mailFolderResource, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.FindMailboxSubscriptionsForOtherFoldersAsync(mailFolderResource, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }

        List<MailFolderSubscription> results = new();
        foreach (MailFolderSubscription subscription in SubscriptionEntities.Values)
        {
            IdentifySubscriptionsForOtherFolders(mailFolderResource, subscription.ToSubscription(), results);
        }

        return results;
    }

    private static ODataError CreateNotFoundException()
    {
        return new ODataError()
        {
            ResponseStatusCode = (int)System.Net.HttpStatusCode.NotFound,
            Error = new()
            {
                Code = "NotFound",
                Message = "The specified subscription does not exist.",
            },
        };
    }

    private void RemoveExpiredSubscriptions()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IEnumerable<MailFolderSubscription> expired = SubscriptionEntities.Values
            .Where(s => s.ExpirationDateTime < now);

        int count = expired.Count();
        foreach (MailFolderSubscription entity in expired)
        {
            SubscriptionEntities.TryRemove(entity.SubscriptionId, out MailFolderSubscription? _);
        }

        this.logger.LogInformation(
            "Number of subscriptions expired is {ExpiredCount}",
            count);
    }
}
