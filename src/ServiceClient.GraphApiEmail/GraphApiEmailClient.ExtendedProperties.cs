// <copyright file="GraphApiEmailClient.ExtendedProperties.cs" company="TheCrownProsecutionService">
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
    public string GetExtendedPropertyCorrespondenceIdExpandClause()
    {
        return $"singleValueExtendedProperties($filter=id eq '{CorrespondenceIdExtendedProperty.ExtendedPropertyId}')";
    }

    /// <inheritdoc/>
    public CorrespondenceIdExtendedProperty? GetExtendedPropertyCorrespondenceIdFromMessage(Message message)
    {
        Requires.NotNull(message);

        CorrespondenceIdExtendedProperty? result = null;
        SingleValueLegacyExtendedProperty? extendedProperty;
        extendedProperty = message.SingleValueExtendedProperties?.SingleOrDefault(x => x.Id == CorrespondenceIdExtendedProperty.ExtendedPropertyId);
        if (extendedProperty != null &&
            extendedProperty.Value != null &&
            CorrespondenceIdExtendedProperty.TryParse(extendedProperty.Value, out CorrespondenceIdExtendedProperty? parsedResult))
        {
            result = parsedResult;
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<Message> SetExtendedPropertyCorrespondenceIdAsync(
        MailMessageIdentifier identifier,
        CorrespondenceIdExtendedProperty correspondenceId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);
        Requires.NotNull(correspondenceId);

        Message patchedMessage = new()
        {
            SingleValueExtendedProperties = new(),
        };

        patchedMessage.SingleValueExtendedProperties.Add(new SingleValueLegacyExtendedProperty()
        {
            Id = CorrespondenceIdExtendedProperty.ExtendedPropertyId,
            Value = correspondenceId.ExtendedPropertyValue,
        });

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
