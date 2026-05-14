// <copyright file="GraphApiEmailClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cps.Fct.EasShared.Core.Diagnostics;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;
using MsgReader.Outlook;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class GraphApiEmailClient : IGraphApiEmailClient
{
    private readonly IGraphApiClient graphApiClient;
    private readonly ILogger<GraphApiEmailClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiEmailClient"/> class.
    /// </summary>
    /// <param name="graphApiClient">The Graph API client.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public GraphApiEmailClient(
        IGraphApiClient graphApiClient,
        ILoggerFactory loggerFactory)
    {
        Requires.NotNull(graphApiClient);
        Requires.NotNull(loggerFactory);

        this.graphApiClient = graphApiClient;
        this.logger = loggerFactory.CreateLogger<GraphApiEmailClient>();
    }

    /// <inheritdoc/>
    public IGraphApiClient GraphApiClient => this.graphApiClient;

    /// <inheritdoc/>
    public Stream ConvertMimeToOutlookMsg(Stream mimeStream, bool removeAttachments = false)
    {
        Requires.NotNull(mimeStream);

        using MemoryStream msgStream = new();
        MsgKit.Converter.ConvertEmlToMsg(mimeStream, msgStream);
        msgStream.Position = 0;

        // leaveStreamOpen: true so MsgReader 6 / OpenMcdf 3 doesn't close msgStream when 'msg' is disposed.
        using Storage.Message msg = new(msgStream, FileAccess.ReadWrite, leaveStreamOpen: true);
        if (removeAttachments && msg.Attachments != null && msg.Attachments.Any())
        {
            Storage.Attachment? attachment = GetFirstAttachment(msg);
            while (attachment is not null)
            {
                msg.DeleteAttachment(attachment);
                attachment = GetFirstAttachment(msg);
            }
        }

        // MsgReader 6 / OpenMcdf 3: msg.Save(stream) takes ownership of the stream and
        // disposes it when the underlying RootStorage is disposed. Save into a temp
        // buffer and copy the bytes out before disposal so the caller gets a clean,
        // fully-owned stream that can be read after this method returns.
        using MemoryStream tempStream = new();
        msg.Save(tempStream);
        byte[] bytes = tempStream.ToArray();

        return new MemoryStream(bytes) { Position = 0 };
    }

    /// <inheritdoc/>
    public async Task<Message?> GetEmailMessageAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default)
    {
        Message? result = null;
        try
        {
            result = await this.GetGraphApiMessageAsync(identifier, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (ODataError exception)
        {
            if (exception.ResponseStatusCode != (int)System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<MailMessageInfo?> GetEmailMessageInfoAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default)
    {
        MailMessageInfo? result = null;
        try
        {
            result = await this.GetGraphApiMailMessageInfoAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (ODataError exception)
        {
            if (exception.ResponseStatusCode != (int)System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Stream?> GetEmailMimeContentAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default)
    {
        Stream? result = null;
        try
        {
            result = await this.GetGraphApiMimeContentAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (ODataError exception)
        {
            if (exception.ResponseStatusCode != (int)System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual async Task ReplyAsync(
        MailMessageIdentifier identifier,
        string messageText,
        CancellationToken cancellationToken = default)
    {
        Microsoft.Graph.Users.Item.Messages.Item.Reply.ReplyPostRequestBody request = new()
        {
            Comment = messageText,
        };

        try
        {
            await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .Reply
                .PostAsync(request, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Logs a error.
    /// </summary>
    /// <param name="error">The error to log.</param>
    protected void LogError(Exception error)
    {
        Requires.NotNull(error);

        this.logger.LogError(
            error,
            DiagnosticsUtility.Error + "Graph API email client raised an error\n - Failure: {Reason}",
            error.ToAggregatedMessage());
    }

    /// <summary>
    /// Gets an email message using the Graph API.
    /// </summary>
    /// <param name="identifier">The message identifier.</param>
    /// <param name="select">Optional: OData select fields. All fields are selected if this value is <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The email message.</returns>
    protected virtual async Task<Message> GetGraphApiMessageAsync(
        MailMessageIdentifier identifier,
        string? select = null,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        const string Expand = "attachments($select=contentType,isInline,lastModifiedDateTime,name,size,id)";
        try
        {
            Message? message = await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .GetAsync(
                    (requestConfiguration) =>
                    {
                        requestConfiguration.QueryParameters.Expand = new string[] { Expand };
                        if (!string.IsNullOrWhiteSpace(select))
                        {
                            requestConfiguration.QueryParameters.Select = select.Split(',');
                        }
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return message!;
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets email message information using the Graph API.
    /// </summary>
    /// <param name="identifier">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The email message information.</returns>
    protected virtual async Task<MailMessageInfo> GetGraphApiMailMessageInfoAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        const string Select = "from,isRead,subject,toRecipients";
        Message message = await this.GetGraphApiMessageAsync(identifier: identifier, select: Select, cancellationToken: cancellationToken).ConfigureAwait(false);
        IReadOnlyCollection<Attachment> attachments = await this.GetEmailMessageAttachmentsAsync(message, cancellationToken).ConfigureAwait(false);
        return new MailMessageInfo(
            message: message,
            mailboxId: identifier.MailboxId,
            messageId: identifier.MessageId,
            attachments: attachments.Select(a => new MailMessageAttachment(a)).ToImmutableList<MailMessageAttachment>());
    }

    /// <summary>
    /// Gets the MIME content of a message using the Graph API.
    /// </summary>
    /// <param name="identifier">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream that can be used to access the content.</returns>
    protected virtual async Task<Stream> GetGraphApiMimeContentAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Stream? result = await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .Content
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result!;
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }

    private static Storage.Attachment? GetFirstAttachment(Storage.Message msg)
    {
        return msg.Attachments.OfType<Storage.Attachment>().Where(AttachmentCanBeDeleted).FirstOrDefault();
    }

    private static bool AttachmentCanBeDeleted(Storage.Attachment a)
    {
        return !a.IsInline && !a.Hidden;
    }
}
