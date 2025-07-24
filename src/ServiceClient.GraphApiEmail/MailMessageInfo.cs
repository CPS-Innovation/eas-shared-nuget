// <copyright file="MailMessageInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Basic information about an email message.
/// </summary>
/// <param name="Attachments">Information on any email attachment.</param>
/// <param name="ETag">The email messages ETag.</param>
/// <param name="FromEmailAddress">The from email address.</param>
/// <param name="IsRead">Indicates whether the email has been flagged as read.</param>
/// <param name="MailboxId">The identity of the mailbox that the folder belongs to. Can be either a user principal name or a user object ID.</param>
/// <param name="MessageId">The identity of the email message.</param>
/// <param name="Subject">The email subject.</param>
/// <param name="ToEmailAddresses">The to email addresses.</param>
public record MailMessageInfo(
    IReadOnlyCollection<MailMessageAttachment> Attachments,
    string ETag,
    string FromEmailAddress,
    bool IsRead,
    string MailboxId,
    string MessageId,
    string Subject,
    IReadOnlyCollection<string> ToEmailAddresses)
        : MailMessageIdentifier(MailboxId: MailboxId, MessageId: MessageId)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MailMessageInfo"/> class.
    /// </summary>
    /// <param name="message">The Graph API email message.</param>
    /// <param name="mailboxId">The identity of the mailbox that the folder belongs to. Can be either a user principal name or a user object ID.</param>
    /// <param name="messageId">The identity of the email message.</param>
    /// <param name="attachments">Information on any email attachment.</param>
    public MailMessageInfo(
        Message message,
        string mailboxId,
        string messageId,
        IReadOnlyCollection<MailMessageAttachment> attachments)
            : this(
                  Attachments: attachments,
                  ETag: message.AdditionalData["@odata.etag"].ToString()!,
                  FromEmailAddress: message.From!.EmailAddress!.Address!,
                  message.IsRead!.Value,
                  MailboxId: mailboxId,
                  MessageId: messageId,
                  Subject: message.Subject!,
                  ToEmailAddresses: DeriveToEmailAddresses(message))
    {
    }

    /// <summary>
    /// Gets a value indicating whether the email has attachments.
    /// </summary>
    public bool HasAttachments => this.Attachments.Any();

    /// <summary>
    /// Gets the to email addresses as a comma separated string.
    /// </summary>
    public string ToEmailAddressesText => string.Concat("'", string.Join(", ", this.ToEmailAddresses), "'");

    private static IReadOnlyCollection<string> DeriveToEmailAddresses(Message message)
    {
        Requires.NotNull(message);

        List<string>? toEmailAddresses = null;
        if (message.ToRecipients != null)
        {
            toEmailAddresses = new(message.ToRecipients.Select(r => r.EmailAddress!.Address!));
        }

        toEmailAddresses ??= new List<string>();
        return toEmailAddresses;
    }
}
