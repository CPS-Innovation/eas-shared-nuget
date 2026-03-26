// <copyright file="IUrnSupportLookup.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Provides URN-related mailbox lookups used by Subscriber.Core (URN translators).
/// </summary>
public interface IUrnSupportLookup
{
    /// <summary>
    /// Determines whether the specified mailbox requires Proceeds of Crime URN format support.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires Proceeds of Crime URN format support; otherwise, <c>false</c>.</returns>
    bool RequiresProceedsOfCrimeUrnSupport(string mailboxId);

    /// <summary>
    /// Determines whether to accept other area URN(s). Only applicable to mailboxes that support Proceeds of Crime URN formats.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> to accept other area URN(s); otherwise <c>false</c>.</returns>
    bool AcceptNonProceedsOfCrimeUrn(string mailboxId);
}
