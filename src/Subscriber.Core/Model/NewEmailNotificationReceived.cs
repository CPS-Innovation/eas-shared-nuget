// <copyright file="NewEmailNotificationReceived.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Model;

using System;

/// <summary>
/// A new email notification received message.
/// </summary>
/// <param name="CorrespondenceId">The correspondence ID.</param>
/// <param name="ETag">
/// The ETag for the email received in the mailbox. This value can be used to determine if
/// the mail has been modified when it is eventually read.
/// </param>
/// <param name="EventTime">The time that the notification was received.</param>
/// <param name="MailboxId">The mailbox globally unique identifier or user principal name.</param>
/// <param name="MessageId">The identity of the email message.</param>
public record NewEmailNotificationReceived(
    Guid CorrespondenceId,
    string ETag,
    DateTimeOffset EventTime,
    string MailboxId,
    string MessageId);
