// <copyright file="FakeGraphApiEmailClient.Attachments.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

/// <summary>
/// The Graph API email client.
/// </summary>
public partial class FakeGraphApiEmailClient : GraphApiEmailClient
{
    private const string DefaultNamespace = "Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation";

    /// <summary>
    /// Gets the file attachment cache.
    /// </summary>
    public static ConcurrentDictionary<string, FileAttachment> FileAttachmentCache { get; } = new();

    /// <inheritdoc/>
    public override async Task<Stream> GetAttachmentMimeContentAsync(MailMessageIdentifier identifier, string attachmentId, CancellationToken cancellationToken = default)
    {
        Stream? result = null;
        try
        {
            result = await base.GetAttachmentMimeContentAsync(identifier, attachmentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        if (result is not null)
        {
            return result;
        }

        if (MimeContentCache.TryGetValue($"MimeAttachments\\{attachmentId}", out Stream? stream))
        {
            result = stream;
        }

        if (result is null)
        {
            throw CreateAttachmentTypeNotSupportedException();
        }

        return result;
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyCollection<Attachment>> GetEmailMessageAttachmentsAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await base.GetEmailMessageAttachmentsAsync(message, cancellationToken).ConfigureAwait(false);

        List<Attachment> results = new();
        if (message.HasAttachments.HasValue && message.HasAttachments.Value)
        {
            FileAttachment attachment = CreateAttachment1();
            results.Add(attachment);
            FileAttachmentCache.TryAdd(attachment.Id!, attachment);

            attachment = CreateAttachment2();
            results.Add(attachment);
            FileAttachmentCache.TryAdd(attachment.Id!, attachment);

            attachment = CreateAttachment3();
            results.Add(attachment);
            FileAttachmentCache.TryAdd(attachment.Id!, attachment);
        }

        return results.OrderBy(r => r.Size).ThenBy(r => r.Name).ToImmutableArray();
    }

    /// <inheritdoc/>
    public override async Task<byte[]> GetFileAttachmentContentAsync(
        MailMessageIdentifier identifier,
        string attachmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            byte[]? bytes = await base.GetFileAttachmentContentAsync(identifier, attachmentId, cancellationToken).ConfigureAwait(false);
            if (bytes is not null)
            {
                return bytes;
            }
        }
        catch (Exception exception)
        {
            this.LogError(exception);
        }

        byte[]? result = null;
        if (FileAttachmentCache.TryGetValue(attachmentId, out FileAttachment? attachment))
        {
            result = attachment.ContentBytes!;
        }

        if (result is null)
        {
            throw CreateAttachmentTypeNotSupportedException();
        }

        return result;
    }

    private static FileAttachment CreateAttachment1()
    {
        string fileName = "Text.attachment.txt";
        byte[] contentBytes = EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{DefaultNamespace}.Embedded.{fileName}", typeof(FakeGraphApiEmailClient));
        FileAttachment attachment = new()
        {
            ContentBytes = contentBytes,
            ContentType = "text/plain",
            IsInline = false,
            LastModifiedDateTime = DateTimeOffset.UtcNow,
            Name = fileName,
            Size = contentBytes.Length,
            Id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAPI1IlAAABEgAQAL_IEZTp2Y1OiqgwdwBF50k=",
        };
        return attachment;
    }

    private static FileAttachment CreateAttachment2()
    {
        string fileName = "Sample.email.body.html";
        byte[] contentBytes = EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{DefaultNamespace}.Embedded.{fileName}", typeof(FakeGraphApiEmailClient));
        FileAttachment attachment = new()
        {
            ContentBytes = contentBytes,
            ContentType = "text/html",
            IsInline = false,
            LastModifiedDateTime = DateTimeOffset.UtcNow,
            Name = fileName,
            Size = contentBytes.Length,
            Id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAPI1IlAAABEgAQAJOmGK2HypJIj0w9XTKwhEw=",
        };
        return attachment;
    }

    private static FileAttachment CreateAttachment3()
    {
        string fileName = "Pdf.attachment.pdf";
        byte[] contentBytes = EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{DefaultNamespace}.Embedded.{fileName}", typeof(FakeGraphApiEmailClient));
        FileAttachment attachment = new()
        {
            ContentBytes = contentBytes,
            ContentType = "application/pdf",
            IsInline = false,
            LastModifiedDateTime = DateTimeOffset.UtcNow,
            Name = fileName,
            Size = contentBytes.Length,
            Id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAPI1IlAAABEgAQAL16jUlLYU1DsfPir7nlxTo=",
        };
        return attachment;
    }
}
