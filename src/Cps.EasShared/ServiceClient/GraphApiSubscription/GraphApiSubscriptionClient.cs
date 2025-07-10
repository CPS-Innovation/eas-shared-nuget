// <copyright file="GraphApiSubscriptionClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription;

using Cps.Fct.Eas.Core.Diagnostics;
using Cps.EasShared.ServiceClient.GraphApi;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft.Extensions.Logging;

/// <summary>
/// A client that can connect to the Graph API and manage change notification subscriptions.
/// </summary>
public partial class GraphApiSubscriptionClient :
    IMailFolderSubscriptionManager
{
    private readonly ILogger<GraphApiSubscriptionClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiSubscriptionClient"/> class.
    /// </summary>
    /// <param name="graphApiClient">The Graph API client.</param>
    /// <param name="loggerFactory">
    /// The factory used to create instances of <see cref="ILogger"/> from the registered <see cref="ILoggerProvider"/>s.
    /// </param>
    public GraphApiSubscriptionClient(
        IGraphApiClient graphApiClient,
        ILoggerFactory loggerFactory)
    {
        Requires.NotNull(graphApiClient);
        Requires.NotNull(loggerFactory);

        this.logger = loggerFactory.CreateLogger<GraphApiSubscriptionClient>();
        this.GraphApiClient = graphApiClient;
    }

    /// <inheritdoc/>
    public IGraphApiClient GraphApiClient { get; }

    /// <summary>
    /// Gets the Graph API mail folder resource for the given mailbox and folder name.
    /// </summary>
    /// <param name="folderIdentifier">Used to identify the mailbox folder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the <see cref="MailFolderResource"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resource was not found.</exception>
    public virtual async Task<MailFolderResource> GetFolderResourceAsync(
        IMailFolderIdentifier folderIdentifier,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(folderIdentifier);

        string mailboxId = folderIdentifier.MailboxId;
        string folderName = folderIdentifier.FolderName;
        string filter = $"displayName eq '{folderName}'";
        MailFolderCollectionResponse? queryResults = null;
        try
        {
            queryResults = await this.GraphApiClient.GraphServiceClient
                .Users[mailboxId]
                .MailFolders
                .GetAsync(
                    (requestConfig) =>
                    {
                        requestConfig.QueryParameters.Select = new string[] { "id", "displayName" };
                        requestConfig.QueryParameters.Filter = filter;
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.GraphApiClient.LogError(ex);
            throw;
        }

        if (queryResults != null && queryResults.Value != null && queryResults.Value.Any())
        {
            return new MailFolderResource(MailboxId: folderIdentifier.MailboxId, FolderId: queryResults.Value.Single().Id!);
        }

        throw CreateFolderNotFoundException(folderIdentifier);
    }

    /// <summary>
    /// Logs a error.
    /// </summary>
    /// <param name="error">The error to log.</param>
    public void LogError(Exception error)
    {
        Requires.NotNull(error);

        this.logger.LogError(
            error,
            DiagnosticsUtility.Error + "Graph API subscription client raised an error.\n - Failure: {Reason}",
            error.ToAggregatedMessage());
    }

    /// <summary>
    /// Creates an exception to indicate that the folder was not found.
    /// </summary>
    /// <param name="folderIdentifier">Used to identify the mailbox folder.</param>
    /// <returns>An <see cref="InvalidOperationException"/>.</returns>
    protected static InvalidOperationException CreateFolderNotFoundException(IMailFolderIdentifier folderIdentifier)
    {
        Requires.NotNull(folderIdentifier);

        string mailboxId = folderIdentifier.MailboxId;
        string folderName = folderIdentifier.FolderName;
        return new InvalidOperationException($"The folder '{folderName}' was not found in the mailbox '{mailboxId}'.");
    }

    private static void ValidateSubscriptionOptions(SubscriptionOptions subscriptionOptions)
    {
        Requires.NotNull(subscriptionOptions);
        Requires.NotNullOrWhiteSpace(subscriptionOptions.ClientState!);
        Requires.Range(
            subscriptionOptions.ClientState!.Length > GraphApiIntegrationOptions.WebhookClientStateMinLength && subscriptionOptions.ClientState!.Length <= GraphApiIntegrationOptions.WebhookClientStateMaxLength,
            nameof(subscriptionOptions.ClientState),
            $"Expecting a string of between {GraphApiIntegrationOptions.WebhookClientStateMinLength} and {GraphApiIntegrationOptions.WebhookClientStateMaxLength} characters in length.");
    }
}
