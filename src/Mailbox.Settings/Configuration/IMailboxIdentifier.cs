// <copyright file="IMailboxIdentifier.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// An interface for identifying a mailbox.
/// </summary>
public interface IMailboxIdentifier
{
    /// <summary>
    /// Gets the email address of the mailbox.
    /// </summary>
    string EmailAddress { get; }

    /// <summary>
    /// Gets the ID of the mailbox. This is typically an Entra object GUID.
    /// </summary>
    string MailboxId { get; }
}
