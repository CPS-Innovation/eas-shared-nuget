// <copyright file="IMailFolderIdentifier.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;

/// <summary>
/// Interface for identifying a mail folder.
/// </summary>
public interface IMailFolderIdentifier
{
    /// <summary>
    /// Gets the name of the folder.
    /// </summary>
    string FolderName { get; }

    /// <summary>
    /// Gets the identity of the mailbox that the folder belongs to. Can be either a user principal name or a user object ID.
    /// </summary>
    string MailboxId { get; }
}
