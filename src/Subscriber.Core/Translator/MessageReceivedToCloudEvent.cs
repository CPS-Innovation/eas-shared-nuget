// <copyright file="MessageReceivedToCloudEvent.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using System;
using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Core.Interfaces;

/// <summary>
/// Translates a <see cref="MessageReceived"/> to a <see cref="CloudEvent"/>
/// with a payload that is a <see cref="NewEmailNotificationReceived"/>.
/// </summary>
public class MessageReceivedToCloudEvent :
    IEntityTranslator<MessageReceived, CloudEvent>
{
    /// <inheritdoc/>
    public CloudEvent Translate(MessageReceived source)
    {
        Requires.NotNull(source);

        string messageId = source.Id;
        string mailboxId = "Unknown";

        // source.Resource is a relative URL that looks like this: Users/{mailboxId}/Messages/{messageId}
        string[] partition = source.Resource.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (partition.Length > 1)
        {
            mailboxId = Uri.UnescapeDataString(partition[1]);
        }

        NewEmailNotificationReceived payload = new(
            CorrespondenceId: Guid.NewGuid(),
            ETag: source.ETag,
            EventTime: DateTimeOffset.UtcNow,
            MailboxId: mailboxId,
            MessageId: messageId);

        CloudEvent result = new()
        {
            Type = source.CloudEventType,
            Source = new Uri($"Users/{mailboxId}/Messages", UriKind.Relative),
            Subject = mailboxId,
            Id = $"{messageId} {payload.ETag}",
            Time = payload.EventTime,
            DataContentType = SubscriberConstants.NewEmailNotificationReceivedContentType,
            Data = payload,
        };

        result[SubscriberConstants.ChangeTypeAttributeName] = source.ChangeType.ToString()!;
        return result;
    }
}
