// <copyright file="EmailStorageRepository.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Translator;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.Storage.Core;
using Cps.Fct.EasShared.Subscriber.Core;
using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using RabbitMQ.Client;

/// <summary>
/// The email storage repository.
/// </summary>
public class EmailStorageRepository
    : IEmailStorageRepository
{
    private static readonly ContentTypeTranslator ContentTypeTranslator = new();
    private static readonly EmailMessageBodyTranslator EmailMessageBodyTranslator = new();
    private readonly ILogger<EmailStorageRepository> logger;
    private readonly IGraphApiEmailClient graphApiEmailClient;
    private readonly IStorageClient emailStorageClient;
    private readonly IRabbitMQPublisher eventPublisher;
    private readonly EmailSubjectToUrnTranslator urnTranslator;
    private readonly EmailReceivedQueueOptions queueOptions;
    private readonly EmailReceivedProceedsOfCrimeQueueOptions pocQueueOptions;
    private readonly MailboxOptions mailboxOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailStorageRepository"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="graphApiEmailClient">The Graph API email client.</param>
    /// <param name="clientFactory">The AWS S3 client factory.</param>
    /// <param name="eventPublisher">The new email received event publisher.</param>
    /// <param name="urnTranslator">The email subject to URN translator.</param>
    /// <param name="applicationOptions">The application options.</param>
    /// <param name="queueOptions">The email received queue options.</param>
    /// <param name="pocQueueOptions">The email received for Proceeds of Crime queue options.</param>
    /// <param name="mailboxOptions">The mailbox options.</param>
    public EmailStorageRepository(
        ILoggerFactory loggerFactory,
        IGraphApiEmailClient graphApiEmailClient,
        IAmazonS3ClientFactory clientFactory,
        IRabbitMQPublisher eventPublisher,
        EmailSubjectToUrnTranslator urnTranslator,
        IOptions<ApplicationOptions> applicationOptions,
        IOptions<EmailReceivedQueueOptions> queueOptions,
        IOptions<EmailReceivedProceedsOfCrimeQueueOptions> pocQueueOptions,
        MailboxOptions mailboxOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(graphApiEmailClient);
        Requires.NotNull(clientFactory);
        Requires.NotNull(eventPublisher);
        Requires.NotNull(urnTranslator);
        Requires.NotNull(applicationOptions);
        Requires.NotNull(applicationOptions.Value);
        Requires.NotNull(queueOptions);
        Requires.NotNull(queueOptions.Value);
        Requires.NotNullOrWhiteSpace(queueOptions.Value.QueueName);
        Requires.NotNull(pocQueueOptions);
        Requires.NotNull(pocQueueOptions.Value);
        Requires.NotNullOrWhiteSpace(pocQueueOptions.Value.QueueName);
        Requires.NotNull(mailboxOptions);

        /*
         * Fixes issue:
         * System.NotSupportedException: No data is available for encoding 1252. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.
         * See: https://stackoverflow.com/questions/50858209/system-notsupportedexception-no-data-is-available-for-encoding-1252
         */
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        this.logger = loggerFactory.CreateLogger<EmailStorageRepository>();
        this.ApplicationOptions = applicationOptions.Value;
        this.queueOptions = queueOptions.Value;
        this.emailStorageClient = clientFactory.GetEmailStorageClient();
        this.graphApiEmailClient = graphApiEmailClient;
        this.eventPublisher = eventPublisher;
        this.urnTranslator = urnTranslator;
        this.pocQueueOptions = pocQueueOptions.Value;
        this.mailboxOptions = mailboxOptions;
    }

    /// <summary>
    /// Gets the application options.
    /// </summary>
    protected ApplicationOptions ApplicationOptions { get; }

    /// <inheritdoc/>
    public async Task PersistEmailMessageAsync(NewEmailNotificationReceived notificationReceived)
    {
        Requires.NotNull(notificationReceived);
        var watch = Stopwatch.StartNew();
        MailMessageIdentifier identifier = new(MailboxId: notificationReceived.MailboxId.ToString(), MessageId: notificationReceived.MessageId);

        Message? message = await this.graphApiEmailClient.GetEmailMessageAsync(identifier).ConfigureAwait(false);

        if (message is not null)
        {
            // URN cannot be null as it has been validated previously, otherwise we would not get to this stage.
            EmailSubjectInfo subjectInfo = new(MailboxId: identifier.MailboxId, Subject: message.Subject);
            string urn = this.urnTranslator.Translate(subjectInfo)!;
            StorageInfo emailBodyInfo = await this.SaveEmailBodyToStorage(
                notificationReceived: notificationReceived,
                urn: urn)
                    .ConfigureAwait(false);

            IReadOnlyCollection<DocumentDetail> attachments = await this.SaveMessageAttachmentsAsync(notificationReceived, message, urn).ConfigureAwait(false);
            string queueName = this.queueOptions.QueueName;

            if (this.mailboxOptions.RequiresSecureMailbox(identifier.MailboxId))
            {
                queueName = this.pocQueueOptions.QueueName;
            }

            CloudEvent emailReceivedEvent = this.CreateNewEmailReceivedEvent(notificationReceived, urn, notificationReceived.MailboxId, message, emailBodyInfo, attachments);
            using IModel channel = this.eventPublisher.PublishEvent(queueName, cloudEvent: emailReceivedEvent);
            this.LogMilestoneCompletedEvent((NewEmailReceived)emailReceivedEvent.Data!, queueName, watch);
        }
        else
        {
            throw new InvalidOperationException($"Unable to retrieve email message with identifier {identifier}.");
        }
    }

    private static string DetermineAttachmentContentType(Attachment attachment)
    {
        string? contentType = attachment.ContentType;

        /*
         * For some reason, the Graph API returns the content type of the message as "message/rfc822" or a blank content type for
         * emails attached to emails. When the content type is blank, we need to check the attachment OData type.
         */
        if (string.IsNullOrWhiteSpace(attachment.ContentType) && "#microsoft.graph.itemAttachment".Equals(attachment.OdataType, StringComparison.OrdinalIgnoreCase))
        {
            contentType = SubscriberConstants.MimeMessageContentType;
        }

        return ContentTypeTranslator.Translate(new Translator.ContentTypeInfo(Filename: attachment.Name!, ContentType: contentType));
    }

    private static string DetermineAttachmentName(Dictionary<string, int> attachmentCounts, Attachment attachment)
    {
        string name = attachment.Name!;
        if (attachmentCounts.TryGetValue(name, out int count))
        {
            count++;
            attachmentCounts[name] = count;
        }
        else
        {
            count = 1;
            attachmentCounts.Add(name, 1);
        }

        if (count > 1)
        {
            // Deal with attachments that have the same name by appending a number to the name.
            string[] parts = name.Split('.');
            if (parts.Length > 1)
            {
                name = string.Join(".", parts[0..^1]) + $" ({count})." + parts[^1];
            }
            else
            {
                name = $"{name} ({count})";
            }
        }

        return name;
    }

    private async Task<StorageInfo> SaveEmailBodyToStorage(
        NewEmailNotificationReceived notificationReceived,
        string urn)
    {
        var watch = Stopwatch.StartNew();
        MailMessageIdentifier identifier = new(MailboxId: notificationReceived.MailboxId.ToString(), MessageId: notificationReceived.MessageId);
        string key = "Unknown";
        string outcome = "Failed";
        string? milestone = null;
        try
        {
            int size = -1;
            bool requiresMsgFormat = this.mailboxOptions.RequiresEmailAsMsgFileIncludeAttachments(identifier.MailboxId) ||
                this.mailboxOptions.RequiresEmailAsMsgFile(identifier.MailboxId);
            outcome = $"Failed - getting email contents, requires MSG format: {requiresMsgFormat}";
            EmailBody emailBody = await this.GetEmailBody(identifier).ConfigureAwait(false);

            string contentType = emailBody.ContentType;
            string emailBodyName = emailBody.Filename;
            key = emailBody.StorageKey;
            bool exists = emailBody.ExistsInStorage;

            if (!exists && emailBody.Stream is null)
            {
                throw new InvalidOperationException($"Unable to retrieve email body for URN: {urn} and Correspondence ID: {notificationReceived.CorrespondenceId}.");
            }

            if (!exists && emailBody.Stream is not null)
            {
                outcome = "Failed - saving contents to storage";
                using Stream stream = emailBody.Stream;
                await this.emailStorageClient.PutStreamAsync(key, stream).ConfigureAwait(false);
                size = (int)stream.Length;
            }

            StorageInfo result = new(
                ContentType: contentType,
                DeleteUrl: this.emailStorageClient.GetDeleteUrl(key, TimeSpan.FromDays(5)),
                GetUrl: this.emailStorageClient.GetAccessUrl(key, TimeSpan.FromDays(5)),
                Name: emailBodyName,
                Size: size);
            if (exists)
            {
                outcome = "Succeeded - already exists";
                milestone = $"email body '{emailBodyName}' already saved to storage, requires MSG format: {requiresMsgFormat}";
            }
            else
            {
                outcome = $"Succeeded - saving {size:N0} bytes";
                milestone = $"email body '{emailBodyName}' saved to storage {size:N0} bytes as content type {contentType}, requires MSG format: {requiresMsgFormat}";
            }

            return result;
        }
        finally
        {
            if (milestone is null)
            {
                const string LogMessage = @"Saving email body to storage for URN {URN} and correspondence ID {CorrespondenceId} - Outcome: {Outcome} - Mailbox ID: {MailboxId}
 - Storage Key: {StorageKey}
 - Duration: {Duration}";
                this.logger.LogInformation(
                    LogMessage,
                    urn,
                    notificationReceived.CorrespondenceId,
                    outcome,
                    notificationReceived.MailboxId,
                    key,
                    watch.Elapsed);
            }
            else
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                const string LogMessage = @"Saved email body to storage at {EventTime}:
 - Storage Key: {StorageKey}
 - Duration: {Duration}
 - Accumulated time: {AccumulatedTime}, Milestone: {Milestone}, for mailbox {MailboxId}, URN: {Urn}, Correspondence ID: {CorrespondenceId}";
                this.logger.LogInformation(
                    LogMessage,
                    TimeUtility.GetUKLocalTime(now),
                    key,
                    watch.Elapsed,
                    now - notificationReceived.EventTime,
                    milestone,
                    notificationReceived.MailboxId,
                    urn,
                    notificationReceived.CorrespondenceId);
            }
        }
    }

    private async Task<EmailBody> GetEmailBody(MailMessageIdentifier identifier)
    {
        bool requiresAttachments = this.mailboxOptions.RequiresEmailAsMsgFileIncludeAttachments(identifier.MailboxId);
        bool requiresMsgFormat = requiresAttachments || this.mailboxOptions.RequiresEmailAsMsgFile(identifier.MailboxId);
        Stream? stream = null;
        string contentType = SubscriberConstants.EmailBodyContentType;
        string emailBodyName = SubscriberConstants.EmailBodyHtmlFilename;
        string key = $"{identifier.ToMessageKey()}/{emailBodyName}";
        bool exists = await this.emailStorageClient.ObjectExistsAsync(key).ConfigureAwait(false);

        if (requiresMsgFormat && !exists)
        {
            contentType = SubscriberConstants.OutlookMessageContentType;
            emailBodyName = SubscriberConstants.EmailBodyOutlookFilename;
            key = $"{identifier.ToMessageKey()}/{emailBodyName}";
            exists = await this.emailStorageClient.ObjectExistsAsync(key).ConfigureAwait(false);
        }

        if (requiresMsgFormat && !exists)
        {
            try
            {
                Stream? mimeStream = await this.graphApiEmailClient.GetEmailMimeContentAsync(identifier).ConfigureAwait(false);
                stream = this.graphApiEmailClient.ConvertMimeToOutlookMsg(mimeStream!, removeAttachments: !requiresAttachments);
            }
            catch
            {
                // If we cannot convert the MIME content to an Outlook message, we will fall back to the HTML message body.
            }
        }

        if (!exists && stream is null)
        {
            contentType = SubscriberConstants.EmailBodyContentType;
            emailBodyName = SubscriberConstants.EmailBodyHtmlFilename;
            key = $"{identifier.ToMessageKey()}/{emailBodyName}";
            Message? message = await this.graphApiEmailClient.GetEmailMessageAsync(identifier).ConfigureAwait(false);
            if (message is not null && message.Body?.Content is not null)
            {
                byte[] content = Encoding.UTF8.GetBytes(EmailMessageBodyTranslator.Translate(message));
                stream = new MemoryStream(content);
            }
        }

        return new EmailBody(
            ContentType: contentType,
            ExistsInStorage: exists,
            Filename: emailBodyName,
            StorageKey: key,
            Stream: stream);
    }

    private async Task<StorageInfo> SaveAttachmentToStorage(
        NewEmailNotificationReceived notificationReceived,
        Attachment attachment,
        string urn,
        Dictionary<string, int> attachmentCounts)
    {
        var watch = Stopwatch.StartNew();
        MailMessageIdentifier identifier = new(MailboxId: notificationReceived.MailboxId.ToString(), MessageId: notificationReceived.MessageId);
        string name = DetermineAttachmentName(attachmentCounts, attachment);
        string key = $"{identifier.ToMessageKey()}/Attachments/{attachment.Id}/{name}";
        string outcome = "Failed";
        string? milestone = null;
        try
        {
            outcome = "Failed - checking object existence in storage";
            int size = attachment.Size ?? 0;
            string contentType = DetermineAttachmentContentType(attachment);
            bool exists = await this.emailStorageClient.ObjectExistsAsync(key).ConfigureAwait(false);
            if (!exists)
            {
                outcome = $"Failed - reading attachment contents of type '{attachment.ContentType}' and derived content type '{contentType}'";
                Stream stream;
                if (contentType.Equals(SubscriberConstants.MimeMessageContentType, StringComparison.OrdinalIgnoreCase))
                {
                    using Stream mimeContent = await this.graphApiEmailClient.GetAttachmentMimeContentAsync(identifier, attachment.Id!).ConfigureAwait(false);
                    stream = this.graphApiEmailClient.ConvertMimeToOutlookMsg(mimeContent);
                    size = (int)stream.Length;
                    contentType = SubscriberConstants.OutlookMessageContentType;
                }
                else
                {
                    byte[] content = await this.graphApiEmailClient.GetFileAttachmentContentAsync(identifier, attachment.Id!).ConfigureAwait(false);
                    size = content.Length;
                    stream = new MemoryStream(content);
                }

                outcome = $"Failed - saving {contentType} attachment contents to storage";
                try
                {
                    await this.emailStorageClient.PutStreamAsync(key, stream).ConfigureAwait(false);
                }
                finally
                {
                    stream.Dispose();
                }
            }

            StorageInfo result = new(
                ContentType: contentType,
                DeleteUrl: this.emailStorageClient.GetDeleteUrl(key, TimeSpan.FromDays(5)),
                GetUrl: this.emailStorageClient.GetAccessUrl(key, TimeSpan.FromDays(5)),
                Name: name,
                Size: size);
            if (exists)
            {
                outcome = "Succeeded - already exists";
                milestone = $"attachment '{result.Name}' already saved to storage";
            }
            else
            {
                outcome = $"Succeeded - saving {result.Size:N0} bytes";
                milestone = $"attachment '{result.Name}' of type {result.ContentType} saved to storage with size {result.Size:N0} bytes";
            }

            return result;
        }
        finally
        {
            if (milestone is null)
            {
                const string LogMessage = @"Saving attachment {Attachment} to storage for URN {URN} and correspondence ID {CorrespondenceId} - Outcome: {Outcome} - Mailbox ID: {MailboxId}
 - Storage Key: {StorageKey}
 - Duration: {Duration}";
                this.logger.LogInformation(
                    LogMessage,
                    name,
                    urn,
                    notificationReceived.CorrespondenceId,
                    outcome,
                    notificationReceived.MailboxId,
                    key,
                    watch.Elapsed);
            }
            else
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                const string LogMessage = @"Saved attachment {Attachment} to storage at {EventTime}:
 - Storage Key: {StorageKey}
 - Duration: {Duration}
 - Accumulated time: {AccumulatedTime}, Milestone: {Milestone}, for mailbox {MailboxId}, URN: {Urn}, Correspondence ID: {CorrespondenceId}";
                this.logger.LogInformation(
                    LogMessage,
                    name,
                    TimeUtility.GetUKLocalTime(now),
                    key,
                    watch.Elapsed,
                    now - notificationReceived.EventTime,
                    milestone,
                    notificationReceived.MailboxId,
                    urn,
                    notificationReceived.CorrespondenceId);
            }
        }
    }

    private async Task<IReadOnlyCollection<DocumentDetail>> SaveMessageAttachmentsAsync(
        NewEmailNotificationReceived notificationReceived,
        Message message,
        string urn)
    {
        IMailboxSettings settings = this.mailboxOptions.GetMailboxSettings(notificationReceived.MailboxId);

        if (settings.IgnoreAttachments)
        {
            return Array.Empty<DocumentDetail>();
        }

        IReadOnlyCollection<Attachment> attachments = await this.graphApiEmailClient.GetEmailMessageAttachmentsAsync(message).ConfigureAwait(false);
        List<DocumentDetail> fileAttachments = new();
        Dictionary<string, int> attachmentCounts = new(StringComparer.OrdinalIgnoreCase);
        foreach (Attachment attachment in attachments)
        {
            if (attachment.IsInline == false && attachment.Size.HasValue)
            {
                StorageInfo info = await this.SaveAttachmentToStorage(notificationReceived, attachment, urn, attachmentCounts).ConfigureAwait(false);
                DocumentDetail detail = new(
                    ContentType: info.ContentType,
                    Name: info.Name,
                    Size: info.Size,
                    ReadUrl: info.GetUrl,
                    DeleteUrl: info.DeleteUrl);
                fileAttachments.Add(detail);
            }
        }

        return fileAttachments;
    }

    private CloudEvent CreateNewEmailReceivedEvent(
        NewEmailNotificationReceived notificationReceived,
        string urn,
        string mailboxId,
        Message message,
        StorageInfo emailBodyInfo,
        IReadOnlyCollection<DocumentDetail> attachments)
    {
        DateTimeOffset eventTime = DateTimeOffset.UtcNow;
        NewEmailReceived payload = new(
            CorrespondenceId: notificationReceived.CorrespondenceId,
            CorrespondenceStartTime: notificationReceived.EventTime,
            MailboxId: mailboxId,
            MessageId: message.Id!,
            Attachments: attachments,
            EventTime: eventTime,
            From: message.From!.EmailAddress!.Address!,
            FromName: message.From!.EmailAddress!.Name!,
            MessageBodyContentType: emailBodyInfo.ContentType,
            MessageBodyDeleteUrl: emailBodyInfo.DeleteUrl,
            MessageBodyName: emailBodyInfo.Name,
            MessageBodyReadUrl: emailBodyInfo.GetUrl,
            MessageReceivedTime: message.ReceivedDateTime!.Value,
            MessageSentTime: message.SentDateTime!.Value,
            MessageSubject: message.Subject!,
            Urn: urn);

        CloudEvent result = new()
        {
            Type = $"#Cps.Fct.EasShared.{nameof(NewEmailReceived)}",
            Source = new Uri($"#{this.ApplicationOptions.Name}", UriKind.Relative),
            Subject = mailboxId.ToString(),
            Id = message.Id,
            Time = payload.EventTime,
            DataContentType = $"application/vnd.Cps.Fct.EasShared.{nameof(NewEmailReceived)}\u002Bjson",
            Data = payload,
        };

        return result;
    }

    private void LogMilestoneCompletedEvent(NewEmailReceived eventPublished, string queueName, Stopwatch watch)
    {
        const string LogMessage = @"Published a new email received event to queue {Queue} at {EventTime}:
 - Message ID: {MessageId}
 - From: {From}
 - Subject: {Subject}
 - Attachments: {Attachments}
 - Duration: {Duration}
 - Accumulated time: {AccumulatedTime}, Milestone: new email received event published, for mailbox {MailboxId}, URN: {Urn}, Correspondence ID: {CorrespondenceId}";

        this.logger.LogInformation(
            LogMessage,
            queueName,
            TimeUtility.GetUKLocalTime(eventPublished.EventTime),
            eventPublished.MessageId,
            eventPublished.From,
            eventPublished.MessageSubject,
            eventPublished.DeriveAttachmentsText(),
            watch.Elapsed,
            DateTimeOffset.UtcNow - eventPublished.CorrespondenceStartTime,
            eventPublished.MailboxId,
            eventPublished.Urn,
            eventPublished.CorrespondenceId);
    }

    private sealed record EmailBody(
        string ContentType,
        bool ExistsInStorage,
        string Filename,
        string StorageKey,
        Stream? Stream);
}
