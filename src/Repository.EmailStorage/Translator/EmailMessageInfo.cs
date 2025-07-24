// <copyright file="EmailMessageInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Translator;

using Microsoft.Graph.Models;

/// <summary>
/// Information about an email message.
/// </summary>
/// <param name="Email">The email message.</param>
/// <param name="MailboxId">The mailbox ID; either a GUID or an email address.</param>
public record EmailMessageInfo(
    Message Email,
    string MailboxId)
{
}
