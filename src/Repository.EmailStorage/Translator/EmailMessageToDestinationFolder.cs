// <copyright file="EmailMessageToDestinationFolder.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Translator;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Interfaces;
using Cps.Fct.EasShared.Core.Text;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;


/// <summary>
/// Email message to mailbox destination folder translator.
/// </summary>
public class EmailMessageToDestinationFolder
    : IEntityTranslator<EmailMessageInfo, DestinationFolderResult>
{
    private static readonly List<string> PostmasterEmailAddresses = new()
    {
        "postmaster@cps.gov.uk",
        "MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk",
    };

    private readonly ITextMatcher textMatcher;
    private readonly EmailAddressExclusionOptions emailAddressExclusionOptions;
    private readonly EmailSubjectExclusionOptions emailSubjectExclusionOptions;
    private readonly EmailSubjectToUrnTranslator urnTranslator;
    private readonly MailboxOptions mailboxOptions;
    private readonly AttachmentOptions attachmentOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailMessageToDestinationFolder"/> class.
    /// </summary>
    /// <param name="textMatcher">The text matcher.</param>
    /// <param name="urnTranslator">The email subject to URN translator.</param>
    /// <param name="emailAddressExclusionOptions">The email address exclusion list.</param>
    /// <param name="emailSubjectExclusionOptions">The email subject exclusion list.</param>
    /// <param name="folderOptions">The mailbox folder options.</param>
    /// <param name="mailboxOptions">The mailbox options.</param>
    public EmailMessageToDestinationFolder(
        ITextMatcher textMatcher,
        EmailSubjectToUrnTranslator urnTranslator,
        IOptions<EmailAddressExclusionOptions> emailAddressExclusionOptions,
        IOptions<EmailSubjectExclusionOptions> emailSubjectExclusionOptions,
        IOptions<MailboxFolderOptions> folderOptions,
        MailboxOptions mailboxOptions,
        IOptions<AttachmentOptions> attachmentSize
        )
    {
        Requires.NotNull(textMatcher);
        Requires.NotNull(urnTranslator);
        Requires.NotNull(emailAddressExclusionOptions);
        Requires.NotNull(emailSubjectExclusionOptions);
        Requires.NotNull(folderOptions);
        Requires.NotNull(mailboxOptions);
        Requires.NotNull(attachmentSize);

        this.textMatcher = textMatcher;
        this.urnTranslator = urnTranslator;
        this.emailAddressExclusionOptions = emailAddressExclusionOptions.Value;
        this.emailSubjectExclusionOptions = emailSubjectExclusionOptions.Value;
        this.FolderOptions = folderOptions.Value;
        this.mailboxOptions = mailboxOptions;
        this.attachmentOptions = attachmentSize.Value;
    }

    /// <summary>
    /// Gets the mailbox folder options.
    /// </summary>
    public MailboxFolderOptions FolderOptions { get; }

    /// <inheritdoc/>
    public DestinationFolderResult Translate(EmailMessageInfo source)
    {
        Requires.NotNull(source);
        Requires.NotNull(source.Email);
        Requires.NotNull(source.MailboxId);

        DestinationFolderResult? destinationFolderResult = null;

        Message message = source.Email;
        EmailSubjectInfo subjectInfo = new(MailboxId: source.MailboxId, Subject: message.Subject!);
        string? urn = this.urnTranslator.Translate(subjectInfo);

        string? fromEmailAddress = message.From?.EmailAddress?.Address;

        if (!string.IsNullOrWhiteSpace(fromEmailAddress))
        {
            destinationFolderResult ??= this.IsFromOutlookPluginEmailAddress(DetermineUrn(urn), fromEmailAddress);

            destinationFolderResult ??= this.IsFromExcludedOrExceptionEmailAddress(DetermineUrn(urn), fromEmailAddress);
        }

        destinationFolderResult ??= this.HasAnExcludedSubjectPhrase(DetermineUrn(urn), fromEmailAddress, message.Subject);

        destinationFolderResult ??= this.HasZipAttachmentException(urn, source.MailboxId, message);

        if (this.mailboxOptions.RequiresEmailAsMsgFile(source.MailboxId))
        {
            destinationFolderResult ??= this.HasInsecureUrlInMessageBody(DetermineUrn(urn), message);
        }

        destinationFolderResult ??= this.HasZeroOrMultipleUrns(urn);

        destinationFolderResult ??= this.HasNoFromEmailAddress(DetermineUrn(urn), fromEmailAddress);

        destinationFolderResult ??= this.HasAttachmentsWithNoName(urn, message);

        destinationFolderResult ??= this.HasAttachmentsWithUnsupportedMimeType(urn, message);

        destinationFolderResult ??= this.HasAttachmentsWithLongFileExtension(urn, message);

        destinationFolderResult ??= this.HasAttachmentsWithSizeExceedingLimit(urn, message);

        if (destinationFolderResult is null)
        {
            string destinationFolder = this.FolderOptions.ProcessingFolderName;
            string reason = "further processing required";
            urn ??= "Unspecified or invalid";
            destinationFolderResult = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return destinationFolderResult;
    }

    private static string DetermineUrn(string? urn)
    {
        return urn ?? "Unspecified or invalid";
    }

    private DestinationFolderResult? HasAttachmentsWithNoName(string? urn, Message message)
    {
        DestinationFolderResult? result = null;
        if (urn is not null &&
            message?.Attachments != null &&
            message.Attachments.Any(a => string.IsNullOrEmpty(a.Name)))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "email has attachments with no name";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasAttachmentsWithUnsupportedMimeType(string? urn, Message message)
    {
        DestinationFolderResult? result = null;
        if (urn is not null &&
            message?.Attachments != null &&
            message.Attachments.Any(a => !string.IsNullOrEmpty(a.ContentType) && a.ContentType.Equals("text/rfc822-headers", StringComparison.OrdinalIgnoreCase)))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "email has attachments with unsupported content type";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasAttachmentsWithLongFileExtension(string? urn, Message message)
    {
        DestinationFolderResult? result = null;

        if (urn is not null &&
            message?.Attachments != null &&
            message.Attachments.Any(a =>
            {
                if (a.Name != null)
                {
                    // Extract the file extension
                    string? extension = Path.GetExtension(a.Name);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        // Remove the dot and check length
                        string extWithoutDot = extension.TrimStart('.');
                        return extWithoutDot.Length > 4;
                    }
                }

                return false;
            }))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "email has attachments with file extension longer than 4 characters";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasAttachmentsWithSizeExceedingLimit(string? urn, Message message)
    {
        DestinationFolderResult? result = null;
        int attachmentsSize = 0;
        if (urn is not null &&
            message?.Attachments != null &&
            message.Attachments.Any(a =>
            {
                if (a.Size != null)
                {
                    // Extract the file extension
                    attachmentsSize += (int)a.Size;
                    return attachmentsSize > attachmentOptions.AttachmentSize;
                }

                return false;
            }))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "email has attachments with size " + attachmentsSize + " exceeding allowed limit of " + attachmentOptions.AttachmentSize;
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? IsFromOutlookPluginEmailAddress(string urn, string fromEmailAddress)
    {
        // Error emails from the Outlook plugin should be treated as exceptions and not added to CMS.
        DestinationFolderResult? result = null;
        string pluginEmailAddress = "responses@cmscps.pnn.police.uk";
        if (pluginEmailAddress.Equals(fromEmailAddress, StringComparison.OrdinalIgnoreCase))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = $"email from plug-in '{pluginEmailAddress}'";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? IsFromExcludedOrExceptionEmailAddress(string urn, string fromEmailAddress)
    {
        // See https://cpsgovuk.atlassian.net/wiki/spaces/FCT2/pages/3847553053/Email+Exclusion+List#Email-Address-Exclusions
        DestinationFolderResult? result = null;
        if (this.emailAddressExclusionOptions.EmailAddressExclusionList.Any() &&
            this.textMatcher.IsMatch(this.emailAddressExclusionOptions.EmailAddressExclusionList, fromEmailAddress, out string? emailAddressOutcome))
        {
            string destinationFolder;
            string reason;
            if ("Exclude".Equals(emailAddressOutcome, StringComparison.OrdinalIgnoreCase))
            {
                destinationFolder = this.FolderOptions.ExcludedFolderName;
                reason = $"excluded email address '{fromEmailAddress}'";
            }
            else
            {
                destinationFolder = this.FolderOptions.ExceptionsFolderName;
                reason = $"exception email address '{fromEmailAddress}'";
            }

            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasAnExcludedSubjectPhrase(string urn, string? fromEmailAddress, string? subject)
    {
        DestinationFolderResult? result = null;

        /*
         * We will not exclude emails from the postmaster because they may be undeliverable email auto responses,
         * which London say they want to be added to CMS.
         */
        bool fromPostMaster = PostmasterEmailAddresses.Any(p => p.Equals(fromEmailAddress, StringComparison.OrdinalIgnoreCase));

        string destinationFolder;
        string reason;
        if (subject is not null &&
            !fromPostMaster &&
            this.emailSubjectExclusionOptions.EmailSubjectExclusionList.Any() &&
            this.textMatcher.IsMatch(this.emailSubjectExclusionOptions.EmailSubjectExclusionList, subject, out _))
        {
            destinationFolder = this.FolderOptions.ExcludedFolderName;
            reason = "excluded subject";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        if (result is null &&
            subject is not null &&
            !fromPostMaster &&
            this.emailSubjectExclusionOptions.EmailSubjectExclusionList.Any())
        {
            List<TextMatchInfo> hyphenList = this.CreateHyphenList();
            if (hyphenList.Any() && this.textMatcher.IsMatch(hyphenList, subject, out _))
            {
                destinationFolder = this.FolderOptions.ExcludedFolderName;
                reason = "excluded subject with hyphen exclusion";
                result = new DestinationFolderResult(destinationFolder, reason, urn);
            }
        }

        return result;
    }

    private DestinationFolderResult? HasZipAttachmentException(string? urn, string mailboxId, Message message)
    {
        DestinationFolderResult? result = null;
        if (urn is not null &&
            this.urnTranslator.MailboxOptions.GetMailboxSettings(mailboxId).TreatZipAttachmentEmailsAsExceptions &&
            message?.Attachments != null &&
            message.Attachments.Any(a => a.Name is not null && a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "email has ZIP attachment";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasInsecureUrlInMessageBody(string urn, Message message)
    {
        DestinationFolderResult? result = null;
        const string phrase = "http://www.cps.gov.uk";

        if (message.Body is not null &&
            message.Body.Content is not null &&
            message.Body.Content.Contains(phrase, StringComparison.OrdinalIgnoreCase))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = $"insecure URL detected in message body - '{phrase}'";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private DestinationFolderResult? HasZeroOrMultipleUrns(string? urn)
    {
        DestinationFolderResult? result = null;
        if (urn is null)
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "no URN or more than one found in subject";
            result = new DestinationFolderResult(destinationFolder, reason, "Unspecified or invalid");
        }

        return result;
    }

    private DestinationFolderResult? HasNoFromEmailAddress(string urn, string? fromEmailAddress)
    {
        DestinationFolderResult? result = null;
        if (string.IsNullOrWhiteSpace(fromEmailAddress))
        {
            string destinationFolder = this.FolderOptions.ExceptionsFolderName;
            string reason = "from email address unspecified";
            result = new DestinationFolderResult(destinationFolder, reason, urn);
        }

        return result;
    }

    private List<TextMatchInfo> CreateHyphenList()
    {
        List<TextMatchInfo> hyphenList = new();
        foreach (TextMatchInfo exclusion in this.emailSubjectExclusionOptions.EmailSubjectExclusionList)
        {
            string snippetToMatch = exclusion.SnippetToMatch;
            if (snippetToMatch.Contains(' '))
            {
                TextMatchInfo hyphenExclusion = new(
                    criteriaForMatching: exclusion.CriteriaForMatching,
                    snippetToMatch: snippetToMatch.Replace(' ', '-'),
                    outcome: exclusion.Outcome);
                hyphenList.Add(hyphenExclusion);
            }
        }

        return hyphenList;
    }
}
