// <copyright file="FakeGraphApiEmailClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class FakeGraphApiEmailClient : GraphApiEmailClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeGraphApiEmailClient"/> class.
    /// </summary>
    /// <param name="graphApiClient">The Graph API client.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public FakeGraphApiEmailClient(
        IGraphApiClient graphApiClient,
        ILoggerFactory loggerFactory)
            : base(graphApiClient, loggerFactory)
    {
    }

    /// <summary>
    /// Gets the message cache.
    /// </summary>
    public static ConcurrentDictionary<string, Message> MessageCache { get; } = new();

    /// <summary>
    /// Gets the MIME content cache.
    /// </summary>
    public static ConcurrentDictionary<string, Stream> MimeContentCache { get; } = new();

    /// <inheritdoc/>
    public override async Task<Message> SetExtendedPropertyCorrespondenceIdAsync(
        MailMessageIdentifier identifier,
        CorrespondenceIdExtendedProperty correspondenceId,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        try
        {
            Message? result = await base.SetExtendedPropertyCorrespondenceIdAsync(identifier, correspondenceId, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (MessageCache.TryGetValue(identifier.ToString(), out Message? message))
        {
            return message;
        }

        throw CreateNotFoundException();
    }

    /// <inheritdoc/>
    public override async Task ReplyAsync(
        MailMessageIdentifier identifier,
        string messageText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await base.ReplyAsync(identifier, messageText, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }
    }

    /// <inheritdoc/>
    protected override async Task<Message> GetGraphApiMessageAsync(
        MailMessageIdentifier identifier,
        string? select = null,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        try
        {
            Message? result = await base.GetGraphApiMessageAsync(identifier, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (MessageCache.TryGetValue(identifier.ToString(), out Message? message))
        {
            return message;
        }

        throw CreateNotFoundException();
    }

    /// <inheritdoc/>
    protected override async Task<MailMessageInfo> GetGraphApiMailMessageInfoAsync(MailMessageIdentifier identifier, CancellationToken cancellationToken = default)
    {
        try
        {
            MailMessageInfo? result = await base.GetGraphApiMailMessageInfoAsync(identifier, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (MessageCache.TryGetValue(identifier.ToString(), out Message? message))
        {
            IReadOnlyCollection<Attachment> attachments = await this.GetEmailMessageAttachmentsAsync(message, cancellationToken).ConfigureAwait(false);
            return new MailMessageInfo(
                message: message,
                mailboxId: identifier.MailboxId,
                messageId: identifier.MessageId,
                attachments: attachments.Select(a => new MailMessageAttachment(a)).ToImmutableArray());
        }

        throw CreateNotFoundException();
    }

    /// <inheritdoc/>
    protected override async Task<Stream> GetGraphApiMimeContentAsync(MailMessageIdentifier identifier, CancellationToken cancellationToken = default)
    {
        Requires.NotNull(identifier);

        try
        {
            Stream? result = await base.GetGraphApiMimeContentAsync(identifier, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (MimeContentCache.TryGetValue(identifier.ToString(), out Stream? message))
        {
            message.Position = 0;
            return message;
        }

        throw CreateNotFoundException();
    }

    private static ODataError CreateNotFoundException()
    {
        return new ODataError()
        {
            ResponseStatusCode = (int)System.Net.HttpStatusCode.NotFound,
            Error = new()
            {
                Code = "NotFound",
                Message = "The specified subscription does not exist.",
            },
        };
    }
}
