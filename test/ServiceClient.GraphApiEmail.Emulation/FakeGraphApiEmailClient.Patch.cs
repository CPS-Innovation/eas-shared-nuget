// <copyright file="FakeGraphApiEmailClient.Patch.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;

using System.Threading;
using System.Threading.Tasks;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Microsoft.Graph.Models;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class FakeGraphApiEmailClient : GraphApiEmailClient
{
    /// <inheritdoc/>
    public override async Task<Message> SetMessageReadStatusAsync(
        MailMessageIdentifier identifier,
        bool isRead,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Message? result = await base.SetMessageReadStatusAsync(identifier, isRead, cancellationToken).ConfigureAwait(false);
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
            message.IsRead = isRead;
            return message;
        }

        throw CreateNotFoundException();
    }
}
