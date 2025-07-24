// <copyright file="FakeGraphApiEmailClient.Folders.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class FakeGraphApiEmailClient : GraphApiEmailClient
{
    /// <summary>
    /// Gets the folder cache.
    /// </summary>
    public static ConcurrentDictionary<string, MailFolder> FolderCache { get; } = new();

    /// <inheritdoc/>
    public override async Task<MailFolder> CreateFolderAsync(
        string mailboxId,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);
        Requires.NotNullOrWhiteSpace(folderName);

        try
        {
            MailFolder? result = await base.CreateFolderAsync(mailboxId, folderName, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        MailFolder folder = new()
        {
            DisplayName = folderName,
            Id = $"{mailboxId}.{folderName}",
        };

        FolderCache.TryAdd(folderName, folder);
        return folder;
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyDictionary<string, string>> GetMailboxFolderIdentitiesAsync(
        string mailboxId,
        IEnumerable<string> folderNames,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(mailboxId);
        Requires.NotNullOrEmpty(folderNames);

        try
        {
            IReadOnlyDictionary<string, string>? dictionary = await base.GetMailboxFolderIdentitiesAsync(mailboxId, folderNames, cancellationToken).ConfigureAwait(false);
            if (dictionary is not null && dictionary.Any())
            {
                return dictionary;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        Dictionary<string, string> result = new(StringComparer.InvariantCultureIgnoreCase);
        foreach (string folderName in folderNames)
        {
            if (FolderCache.TryGetValue(folderName, out MailFolder? folder))
            {
                result.Add(folderName, folder.Id!);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public override async Task<Message> MoveMessageToFolderAsync(MailMessageIdentifier identifier, string folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            Message? result = await base.MoveMessageToFolderAsync(identifier, folderId, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (MessageCache.TryGetValue(identifier.ToString(), out Message? message))
        {
            return message;
        }

        throw CreateNotFoundException();
    }
}
