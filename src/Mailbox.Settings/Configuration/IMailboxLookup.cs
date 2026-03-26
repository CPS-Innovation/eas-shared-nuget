// <copyright file="IMailboxLookup.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Provides general mailbox lookups used by microservices (e.g. CmsSyncService).
/// </summary>
public interface IMailboxLookup
{
    /// <summary>
    /// Gets the mailbox settings for the specified mailbox.
    /// </summary>
    /// <param name="id">The mailbox ID; either an email address or an AAD object GUID.</param>
    /// <returns>The mailbox settings for the specified mailbox.</returns>
    IMailboxSettings GetMailboxSettings(string id);

    /// <summary>
    /// Determines whether the specified mailbox is a supported shared mailbox.
    /// </summary>
    /// <param name="id">The mailbox ID; either an email address or an AAD object GUID.</param>
    /// <returns>A value indicating whether the specified mailbox is a supported shared mailbox.</returns>
    bool IsSharedMailbox(string id);

    /// <summary>
    /// Determines whether the specified mailbox requires Proceeds of Crime URN format support.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires Proceeds of Crime URN format support; otherwise, <c>false</c>.</returns>
    bool RequiresProceedsOfCrimeUrnSupport(string mailboxId);
}
