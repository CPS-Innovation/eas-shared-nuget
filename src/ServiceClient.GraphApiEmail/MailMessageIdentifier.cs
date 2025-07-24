// <copyright file="MailMessageIdentifier.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System;

/// <summary>
/// A mail message identifier.
/// </summary>
/// <param name="MailboxId">The mailbox globally unique identifier or user principal name.</param>
/// <param name="MessageId">The message identifier.</param>
public record MailMessageIdentifier(
    string MailboxId,
    string MessageId)
{
    /// <inheritdoc/>
    public override string ToString() => $"Users/{Uri.EscapeDataString(this.MailboxId)}/{this.ToMessageKey()}";

    /// <summary>
    /// Gets the message key used to identify the message in storage.
    /// </summary>
    /// <returns>The message key used to identify the message in storage.</returns>
    public string ToMessageKey() => $"Messages/{this.MessageId}";

    /// <summary>
    /// Parses a resource identifier.
    /// </summary>
    /// <param name="resource">The resource identifier.</param>
    /// <returns>The resource identifier as a <see cref="MailMessageIdentifier"/>.</returns>
    public static MailMessageIdentifier Parse(string resource)
    {
        string[] parts = resource.Split('/');
        return new MailMessageIdentifier(
            MailboxId: Uri.UnescapeDataString(parts[1]),
            MessageId: parts[3]);
    }
}
