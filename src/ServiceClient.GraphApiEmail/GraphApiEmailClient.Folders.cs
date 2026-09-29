// <copyright file="GraphApiEmailClient.Folders.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class GraphApiEmailClient : IGraphApiEmailClient
{
    /// <inheritdoc/>
    public virtual async Task<MailFolder> CreateFolderAsync(string mailboxId, string folderName, CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);
        Requires.NotNullOrWhiteSpace(folderName);

        MailFolder folder = new()
        {
            DisplayName = folderName,
        };

        try
        {
            return (await this.graphApiClient.GraphServiceClient
                .Users[mailboxId]
                .MailFolders
                .PostAsync(folder, cancellationToken: cancellationToken)
                .ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyDictionary<string, string>> GetMailboxFolderIdentitiesAsync(
        string mailboxId,
        IEnumerable<string> folderNames,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);
        Requires.NotNullOrEmpty(folderNames);

        Dictionary<string, string> result = new(StringComparer.InvariantCultureIgnoreCase);
        SortedSet<string> folderNamesSet = new(folderNames, StringComparer.InvariantCultureIgnoreCase);
        string filter = string.Join(" or ", folderNamesSet.Select(f => $"displayName eq '{f}'"));

        MailFolderCollectionResponse? firstPage = null;
        try
        {
            firstPage = await this.graphApiClient.GraphServiceClient
               .Users[mailboxId]
               .MailFolders
               .GetAsync(
                   (requestConfiguration) =>
                   {
                       requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName" };
                       requestConfiguration.QueryParameters.Filter = filter;
                   },
                   cancellationToken: cancellationToken)
               .ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }

        if (firstPage != null)
        {
            var pageIterator = PageIterator<MailFolder, MailFolderCollectionResponse>.CreatePageIterator(
                client: this.graphApiClient.GraphServiceClient,
                page: firstPage,
                callback: mailFolder =>
                {
                    if (!result.ContainsKey(mailFolder.DisplayName!))
                    {
                        result.Add(mailFolder.DisplayName!, mailFolder.Id!);
                        folderNamesSet.Remove(mailFolder.DisplayName!);
                    }
                    else
                    {
                        throw new InvalidOperationException($"A duplicate mailbox folder with name '{mailFolder.DisplayName}' was found in mailbox '{mailFolder.Id}'.");
                    }

                    return true;
                });

            try
            {
                await pageIterator.IterateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ODataError ex)
            {
                this.graphApiClient.LogError(ex);
                throw;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string?> GetMailboxFolderIdentityAsync(
        string mailboxId,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);
        Requires.NotNullOrWhiteSpace(folderName);

        IReadOnlyDictionary<string, string> results = await this.GetMailboxFolderIdentitiesAsync(
            mailboxId,
            new[] { folderName },
            cancellationToken)
                .ConfigureAwait(false);
        results.TryGetValue(folderName, out string? result);
        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<Message> MoveMessageToFolderAsync(
        MailMessageIdentifier identifier,
        string folderId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);
        Requires.NotNullOrWhiteSpace(folderId);

        var request = new Microsoft.Graph.Users.Item.Messages.Item.Move.MovePostRequestBody
        {
            DestinationId = folderId,
        };

        try
        {
            return (await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .Move
                .PostAsync(body: request, cancellationToken: cancellationToken)
                .ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }
}
