// <copyright file="IEmailStorageMailboxLookup.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Provides email-storage-related mailbox lookups used by Repository.EmailStorage.
/// </summary>
public interface IEmailStorageMailboxLookup
{
    /// <summary>
    /// Determines whether the mailbox should use the secure queue for processing emails.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox is secure; otherwise, <c>false</c>.</returns>
    bool RequiresSecureMailbox(string mailboxId);

    /// <summary>
    /// Determines whether the mailbox requires emails to be in MSG file format.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires emails to be in MSG file format; otherwise, <c>false</c>.</returns>
    bool RequiresEmailAsMsgFile(string mailboxId);

    /// <summary>
    /// Determines whether the mailbox requires emails to be in MSG file format with attachments.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires emails to be in MSG file format with attachments; otherwise, <c>false</c>.</returns>
    bool RequiresEmailAsMsgFileIncludeAttachments(string mailboxId);

    /// <summary>
    /// Determines whether the mailbox requires attachments to be ignored.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires attachments to be ignored; otherwise, <c>false</c>.</returns>
    bool RequiresIgnoreAttachments(string mailboxId);

    /// <summary>
    /// Gets the mailbox settings for the specified mailbox.
    /// </summary>
    /// <param name="id">The mailbox ID; either an email address or an AAD object GUID.</param>
    /// <returns>The mailbox settings for the specified mailbox.</returns>
    IMailboxSettings GetMailboxSettings(string id);
}
