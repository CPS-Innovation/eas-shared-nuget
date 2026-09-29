// <copyright file="EmailAttachmentExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core;

using System.Linq;
using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Core.Diagnostics;

/// <summary>
/// Email attachment extensions used for diagnostics.
/// </summary>
public static class EmailAttachmentExtensions
{
    /// <summary>
    /// Derives a string representation of a <see cref="NewEmailReceived"/> attachments for diagnostic purposes.
    /// </summary>
    /// <param name="message">The <see cref="NewEmailReceived"/>.</param>
    /// <returns>A string representation of a <see cref="NewEmailReceived"/> attachments for diagnostic purposes.</returns>
    public static string DeriveAttachmentsText(this NewEmailReceived message)
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
}
