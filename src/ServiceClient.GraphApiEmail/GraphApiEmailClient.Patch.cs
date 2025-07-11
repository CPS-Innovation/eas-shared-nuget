// <copyright file="GraphApiEmailClient.Patch.cs" company="TheCrownProsecutionService">
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
    public virtual async Task<Message> SetMessageReadStatusAsync(
        MailMessageIdentifier identifier,
        bool isRead,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        Message patchedMessage = new()
        {
            IsRead = isRead,
        };

        try
        {
            return (await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .PatchAsync(patchedMessage, cancellationToken: cancellationToken)
                .ConfigureAwait(false))!;
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }
}
