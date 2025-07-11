// <copyright file="GraphApiEmailClient.Attachments.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class GraphApiEmailClient : IGraphApiEmailClient
{
    /// <inheritdoc/>
    public virtual async Task<Stream> GetAttachmentMimeContentAsync(
            MailMessageIdentifier identifier,
            string attachmentId,
            CancellationToken cancellationToken = default)
    {
        Stream? result = null;
        try
        {
            Attachment? attachment = await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .Attachments[attachmentId]
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (attachment is ItemAttachment itemAttachment)
            {
                NativeResponseHandler nativeResponseHandler = new();
                Microsoft.Graph.Users.Item.Messages.Item.Attachments.Item.AttachmentItemRequestBuilder reqBuilder = this.graphApiClient.GraphServiceClient
                    .Users[identifier.MailboxId]
                    .Messages[identifier.MessageId]
                    .Attachments[attachmentId];
                RequestInformation request = reqBuilder.ToGetRequestInformation(
                    requestConfiguration => requestConfiguration.Options.Add(new ResponseHandlerOption() { ResponseHandler = nativeResponseHandler }));
                request.URI = new Uri(request.URI.OriginalString + "/$value");
                if (request != null)
                {
                    await this.graphApiClient.GraphServiceClient.RequestAdapter.SendAsync(request, ItemAttachment.CreateFromDiscriminatorValue, cancellationToken: cancellationToken).ConfigureAwait(false);
                    using var responseMessage = nativeResponseHandler.Value as HttpResponseMessage;
                    if (responseMessage != null)
                    {
                        responseMessage.EnsureSuccessStatusCode();
                        byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                        result = new MemoryStream(bytes)
                        {
                            Position = 0,
                        };
                    }
                }
            }
            else if (attachment is FileAttachment fileAttachment)
            {
                result = new MemoryStream(fileAttachment.ContentBytes!)
                {
                    Position = 0,
                };
            }
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }

        if (result is null)
        {
            throw CreateAttachmentTypeNotSupportedException();
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual Task<IReadOnlyCollection<Attachment>> GetEmailMessageAttachmentsAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNull(message);

        List<Attachment> attachments = new();
        if (message.Attachments != null)
        {
            IEnumerable<Attachment> viableAttachments = message.Attachments.Where(a => a.IsInline.HasValue && !a.IsInline.Value);
            if (viableAttachments.Any())
            {
                attachments.AddRange(viableAttachments);
            }
        }

        IReadOnlyCollection<Attachment> result = attachments.OrderBy(r => r.Size).ThenBy(r => r.Name).ToImmutableArray();
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public virtual async Task<byte[]> GetFileAttachmentContentAsync(
        MailMessageIdentifier identifier,
        string attachmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Attachment? attachment = await this.graphApiClient.GraphServiceClient
                .Users[identifier.MailboxId]
                .Messages[identifier.MessageId]
                .Attachments[attachmentId]
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (attachment is FileAttachment fileAttachment)
            {
                return fileAttachment.ContentBytes!;
            }

            throw CreateAttachmentTypeNotSupportedException();
        }
        catch (ODataError ex)
        {
            this.graphApiClient.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Creates a <see cref="NotSupportedException"/> for when an attachment type is not supported.
    /// </summary>
    /// <returns>A <see cref="NotSupportedException"/> for when an attachment type is not supported.</returns>
    protected static NotSupportedException CreateAttachmentTypeNotSupportedException()
    {
        return new NotSupportedException("Only attachments that are file attachments or MIME attachments are supported.");
    }
}
