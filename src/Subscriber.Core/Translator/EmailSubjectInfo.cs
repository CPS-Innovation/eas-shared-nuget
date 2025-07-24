// <copyright file="EmailSubjectInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

/// <summary>
/// Information about an email subject.
/// </summary>
/// <param name="MailboxId">The mailbox ID; either a GUID or an email address.</param>
/// <param name="Subject">The email subject.</param>
public record EmailSubjectInfo(
    string MailboxId,
    string? Subject)
{
}
