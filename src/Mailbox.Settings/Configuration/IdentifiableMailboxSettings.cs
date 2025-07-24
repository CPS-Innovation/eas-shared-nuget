// <copyright file="IdentifiableMailboxSettings.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Information for a mailbox, including settings and unique identifiers.
/// </summary>
/// <param name="EmailAddress">The email address of the mailbox.</param>
/// <param name="MailboxId">The ID of the mailbox. This is typically an Entra object GUID.</param>
/// <param name="CreateCommunicationTask">A value indicating whether to create a communication task.</param>
/// <param name="DefaultParty">The default party.</param>
/// <param name="DefaultPartyDescription">The default party description.</param>
/// <param name="DisableAddToFinalisedCase">A value indicating whether to disable adding emails to a finalised case.</param>
/// <param name="DisableInvalidUrnAutoReply">A value indicating whether to disable the auto reply for an invalid URN.</param>
/// <param name="DummyUrnPrefix">
/// The dummy URN prefix. If a mailbox supports dummy URN(s), and a URN begins with this prefix and does not exist in CMS,
/// it will be moved to CMS.Exceptions rather than CMS.InvalidURN folder.
/// </param>
/// <param name="EmailAsMsgFileSupport">The settings for email as MSG file support.</param>
/// <param name="Environment">The environment the mailbox is associated with.</param>
/// <param name="FullyAutomated">A value indicating whether the mailbox is fully automated.</param>
/// <param name="IgnoreAttachments">A value indicating whether to ignore attachments.</param>
/// <param name="MarkCommunicationAsRead">A value indicating whether to mark the communication as read.</param>
/// <param name="Name">The mailbox name.</param>
/// <param name="NameWelsh">The Welsh mailbox name. If this value is not <c>null</c>. Then the mailbox is considered to be a Welsh mailbox.</param>
/// <param name="ProceedsOfCrimeUrnSupport">The settings for Proceeds of Crime URN support.</param>
/// <param name="Region">The mailbox region. Used for categorisation.</param>
/// <param name="TreatZipAttachmentEmailsAsExceptions">Indicates whether to treat emails with ZIP attachment as exceptions.</param>
/// <param name="IsSecureMailbox">Indicates whether the secure queue should be used for processing emails.</param>
public record IdentifiableMailboxSettings(
    string EmailAddress,
    string MailboxId,
    bool CreateCommunicationTask = DefaultMailboxSettings.CreateCommunicationTask,
    string DefaultParty = DefaultMailboxSettings.DefaultParty,
    string DefaultPartyDescription = DefaultMailboxSettings.DefaultPartyDescription,
    bool DisableInvalidUrnAutoReply = DefaultMailboxSettings.DisableInvalidUrnAutoReply,
    bool DisableAddToFinalisedCase = DefaultMailboxSettings.DisableAddToFinalisedCase,
    string? DummyUrnPrefix = DefaultMailboxSettings.DummyUrnPrefix,
    EmailAsMsgFileSupport? EmailAsMsgFileSupport = null,
    string Environment = DefaultMailboxSettings.Environment,
    bool FullyAutomated = DefaultMailboxSettings.FullyAutomated,
    bool IgnoreAttachments = DefaultMailboxSettings.IgnoreAttachments,
    bool MarkCommunicationAsRead = DefaultMailboxSettings.MarkCommunicationAsRead,
    string Name = DefaultMailboxSettings.Name,
    string? NameWelsh = DefaultMailboxSettings.NameWelsh,
    ProceedsOfCrimeUrnSupport? ProceedsOfCrimeUrnSupport = null,
    string Region = DefaultMailboxSettings.Region,
    bool TreatZipAttachmentEmailsAsExceptions = DefaultMailboxSettings.TreatZipAttachmentEmailsAsExceptions,
    bool IsSecureMailbox = DefaultMailboxSettings.IsSecureMailbox)
        : IMailboxIdentifier, IMailboxSettings
{
}
