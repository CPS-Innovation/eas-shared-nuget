// <copyright file="MailFolderResource.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A Graph API mail folder resource that can be used to create change notification subscriptions.
/// </summary>
/// <param name="MailboxId">The identity of the mailbox that the folder belongs to. Can be either a user principal name or a user object ID.</param>
/// <param name="FolderId">The identity of the folder.</param>
public record MailFolderResource(
    string MailboxId,
    string FolderId)
{
    /// <summary>
    /// Gets the Graph API resource path that can be used to set up a subscription.
    /// </summary>
    public string Resource
    {
        get
        {
            return $"/users/{Uri.EscapeDataString(this.MailboxId)}/mailFolders/{Uri.EscapeDataString(this.FolderId)}/messages";
        }
    }

    /// <summary>
    /// Tries to parse a resource string into a <see cref="MailFolderResource"/>.
    /// </summary>
    /// <param name="resource">The resource string to parse.</param>
    /// <param name="mailFolderIdentifier">A <see cref="MailFolderResource"/> if parsed successfully; otherwise <c>null</c>.</param>
    /// <returns>Returns <c>true</c> if <paramref name="resource"/> could be parsed; otherwise returns <c>false</c>.</returns>
    public static bool TryParse(string resource, [NotNullWhen(true)] out MailFolderResource? mailFolderIdentifier)
    {
        mailFolderIdentifier = null;

        if (string.IsNullOrWhiteSpace(resource))
        {
            return false;
        }

        string[] parts = resource.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            return false;
        }

        if (!"users".Equals(parts[0], StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!"mailFolders".Equals(parts[2], StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!"messages".Equals(parts[4], StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        mailFolderIdentifier = new MailFolderResource(
            MailboxId: Uri.UnescapeDataString(parts[1]),
            FolderId: Uri.UnescapeDataString(parts[3]));
        return true;
    }
}
