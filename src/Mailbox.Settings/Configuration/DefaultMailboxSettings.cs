// <copyright file="DefaultMailboxSettings.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Default mailbox settings. Update each class that uses these settings when adding new values.
/// </summary>
internal static class DefaultMailboxSettings
{
    /// <summary>
    /// A value indicating whether to create a communication task.
    /// </summary>
    public const bool CreateCommunicationTask = true;

    /// <summary>
    /// The default party.
    /// </summary>
    public const string DefaultParty = "Defence";

    /// <summary>
    /// The default party description.
    /// </summary>
    public const string DefaultPartyDescription = "DEF/CSL";

    /// <summary>
    /// A value indicating whether to disable adding emails to a finalised case.
    /// </summary>
    public const bool DisableAddToFinalisedCase = false;

    /// <summary>
    /// A value indicating whether to disable the auto reply for an invalid URN.
    /// </summary>
    public const bool DisableInvalidUrnAutoReply = false;

    /// <summary>
    /// The dummy URN prefix.
    /// </summary>
    public const string? DummyUrnPrefix = null;

    /// <summary>
    /// The default environment the mailbox is associated with.
    /// </summary>
    public const string Environment = "N/A";

    /// <summary>
    /// A value indicating whether the mailbox is fully automated.
    /// </summary>
    public const bool FullyAutomated = false;

    /// <summary>
    /// A value indicating whether to ignore attachments.
    /// </summary>
    public const bool IgnoreAttachments = false;

    /// <summary>
    /// A value indicating whether to mark the communication as read.
    /// </summary>
    public const bool MarkCommunicationAsRead = false;

    /// <summary>
    /// The mailbox name.
    /// </summary>
    public const string Name = "Crown Prosecution Service";

    /// <summary>
    /// The Welsh mailbox name.
    /// </summary>
    public const string? NameWelsh = null;

    /// <summary>
    /// The mailbox region. Used for categorisation.
    /// </summary>
    public const string Region = "Unknown";

    /// <summary>
    /// A value indicating whether to treat emails with ZIP attachment as exceptions.
    /// </summary>
    public const bool TreatZipAttachmentEmailsAsExceptions = false;

    /// <summary>
    /// A value indicating whether the secure queue should be used for processing emails.
    /// </summary>
    public const bool IsSecureMailbox = false;
}
