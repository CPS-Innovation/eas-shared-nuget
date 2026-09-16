// <copyright file="IMailboxSettings.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// An interface for mailbox settings.
/// </summary>
public interface IMailboxSettings
{
    /// <summary>
    /// Gets a value indicating whether to create a communication task.
    /// </summary>
    bool CreateCommunicationTask { get; }

    /// <summary>
    /// Gets the default party.
    /// </summary>
    string DefaultParty { get; }

    /// <summary>
    /// Gets the default party description.
    /// </summary>
    string DefaultPartyDescription { get; }

    /// <summary>
    /// Gets a value indicating whether to disable adding emails to a finalised case.
    /// </summary>
    bool DisableAddToFinalisedCase { get; }

    /// <summary>
    /// Gets a value indicating whether to disable the auto reply for an invalid URN.
    /// </summary>
    bool DisableInvalidUrnAutoReply { get; }

    /// <summary>
    /// Gets the dummy URN prefix. If a mailbox supports dummy URN(s), and a URN begins with this prefix and does not exist in CMS,
    /// it will be moved to CMS.Exceptions rather than CMS.InvalidURN folder.
    /// </summary>
    string? DummyUrnPrefix { get; }

    /// <summary>
    /// Gets the settings for email as MSG file support.
    /// </summary>
    EmailAsMsgFileSupport? EmailAsMsgFileSupport { get; }

    /// <summary>
    /// Gets the environment the mailbox is associated with.
    /// </summary>
    string Environment { get; }

    /// <summary>
    /// Gets a value indicating whether the mailbox is fully automated.
    /// </summary>
    bool FullyAutomated { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore attachments.
    /// </summary>
    bool IgnoreAttachments { get; }

    /// <summary>
    /// Gets a value indicating whether to mark the communication as read.
    /// </summary>
    bool MarkCommunicationAsRead { get; }

    /// <summary>
    /// Gets the mailbox name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the Welsh mailbox name. If this value is not <c>null</c>. Then the mailbox is considered to be a Welsh mailbox.
    /// </summary>
    string? NameWelsh { get; }

    /// <summary>
    /// Gets the settings for Proceeds of Crime URN support.
    /// </summary>
    ProceedsOfCrimeUrnSupport? ProceedsOfCrimeUrnSupport { get; }

    /// <summary>
    /// Gets the mailbox region. Used for categorisation.
    /// </summary>
    string Region { get; }

    /// <summary>
    /// Gets a value indicating whether to treat emails with ZIP attachment as exceptions.
    /// </summary>
    bool TreatZipAttachmentEmailsAsExceptions { get; }

    /// <summary>
    /// Gets a value indicating whether the secure queue should be used for processing emails.
    /// </summary>
    bool IsSecureMailbox { get; }
}
