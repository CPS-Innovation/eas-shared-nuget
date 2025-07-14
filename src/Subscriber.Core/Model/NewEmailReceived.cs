// <copyright file="NewEmailReceived.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Model;

using System;
using System.Collections.Generic;

/// <summary>
/// A new email received message.
/// </summary>
/// <param name="CorrespondenceId">The correspondence ID.</param>
/// <param name="CorrespondenceStartTime">
/// The time that the process started when a new email notification was received.
/// Can be used to measure the total duration of the end to end process.
/// </param>
/// <param name="MailboxId">The mailbox globally unique identifier or user principal name.</param>
/// <param name="MessageId">The message identity.</param>
/// <param name="Attachments">The email message file attachments.</param>
/// <param name="EventTime">The time that the event occurred.</param>
/// <param name="From">The email address that this message is from.</param>
/// <param name="FromName">The name of the email address.</param>
/// <param name="MessageBodyDeleteUrl">The pre-signed DELETE url used to delete the body from storage.</param>
/// <param name="MessageBodyName">The name of the message body; for example, "email.body.msg".</param>
/// <param name="MessageBodyReadUrl">The pre-signed GET url used to read the body from storage.</param>
/// <param name="MessageReceivedTime">The time the email was received.</param>
/// <param name="MessageSentTime">The time that the sender sent the email.</param>
/// <param name="MessageSubject">The email message subject.</param>
/// <param name="Urn">The unique reference number of the case related to the email. </param>
/// <param name="MessageBodyContentType">The content type of the message body.</param>
public record NewEmailReceived(
    Guid CorrespondenceId,
    DateTimeOffset CorrespondenceStartTime,
    string MailboxId,
    string MessageId,
    IReadOnlyCollection<DocumentDetail> Attachments,
    DateTimeOffset EventTime,
    string From,
    string FromName,
    string MessageBodyDeleteUrl,
    string MessageBodyName,
    string MessageBodyReadUrl,
    DateTimeOffset MessageReceivedTime,
    DateTimeOffset MessageSentTime,
    string MessageSubject,
    string Urn,
    string MessageBodyContentType);
