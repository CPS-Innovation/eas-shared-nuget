// <copyright file="IMailboxCatalog.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

using System.Collections.Generic;

/// <summary>
/// Provides access to the full mailbox catalog, used by SchedulerService for iteration and reporting.
/// </summary>
public interface IMailboxCatalog
{
    /// <summary>
    /// Gets the list of mailboxes.
    /// </summary>
    IList<IdentifiableMailboxSettings> MailboxList { get; }
}
