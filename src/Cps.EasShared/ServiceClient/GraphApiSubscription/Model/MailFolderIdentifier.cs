// <copyright file="MailFolderIdentifier.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription.Model;

using Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;

/// <summary>
/// Identifies a mail folder.
/// </summary>
/// <param name="MailboxId">The identity of the mailbox that the folder belongs to. Can be either a user principal name or a user object ID.</param>
/// <param name="FolderName">The name of the folder.</param>
public record MailFolderIdentifier(
    string MailboxId,
    string FolderName) : IMailFolderIdentifier
{
}
