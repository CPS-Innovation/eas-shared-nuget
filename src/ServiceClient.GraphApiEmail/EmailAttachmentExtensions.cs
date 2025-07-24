// <copyright file="EmailAttachmentExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Linq;
using Cps.Fct.EasShared.Core.Diagnostics;

/// <summary>
/// Email attachment extensions used for diagnostics.
/// </summary>
public static class EmailAttachmentExtensions
{
    /// <summary>
    /// Derives a string representation of a <see cref="MailMessageInfo"/> attachments for diagnostic purposes.
    /// </summary>
    /// <param name="message">The <see cref="MailMessageInfo"/>.</param>
    /// <returns>A string representation of a <see cref="MailMessageInfo"/> attachments for diagnostic purposes.</returns>
    public static string DeriveAttachmentsText(this MailMessageInfo message)
    {
        Requires.NotNull(message);

        if (message.Attachments.Any())
        {
            return $"#{message.Attachments.Count}: {string.Join(", ", message.Attachments.Select(a => DiagnosticsUtility.ToAttachmentText(a.Name, a.Size, a.ContentType)))}";
        }
        else
        {
            return "#0: no attachments";
        }
    }

    /// <summary>
    /// Derives a string representation of a <see cref="Message"/> attachments for diagnostic purposes.
    /// </summary>
    /// <param name="message">The <see cref="Message"/>.</param>
    /// <returns>A string representation of a <see cref="Message"/> attachments for diagnostic purposes.</returns>
    public static string DeriveAttachmentsText(this Message message)
    {
        Requires.NotNull(message);

        if (message.Attachments != null && message.Attachments.Any())
        {
            return $"#{message.Attachments.Count}: {string.Join(", ", message.Attachments.Select(a => DiagnosticsUtility.ToAttachmentText(a.Name!, (long)a.Size!, a.ContentType!, a.OdataType!)))}";
        }
        else
        {
            return "#0: no attachments";
        }
    }
}
