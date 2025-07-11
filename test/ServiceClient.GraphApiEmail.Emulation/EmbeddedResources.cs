// <copyright file="EmbeddedResources.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;

using System;
using System.Text;
using Cps.Fct.EasShared.Core;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

/// <summary>
/// Loads embedded resources.
/// </summary>
public class EmbeddedResources
{
    private const string ResourceNamespace = "Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation";

    private static readonly JsonParseNodeFactory ParseNodeFactory = new();

    private static readonly Lazy<string> EmailTemplateJsonLazy = new(
        () => EmbeddedResourceUtility.LoadEmbeddedResource($"{ResourceNamespace}.Embedded.Email.template.json", typeof(EmbeddedResources)));

    private static readonly Lazy<string> PdfAttachmentPdfLazy = new(
            () => Convert.ToBase64String(
                EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{ResourceNamespace}.Embedded.Pdf.attachment.pdf", typeof(EmbeddedResources))));

    /// <summary>
    /// Gets the 'Email.template.json' embedded resource.
    /// </summary>
    public static string EmailTemplateJson => EmailTemplateJsonLazy.Value;

    /// <summary>
    /// Gets the 'Pdf.attachment.pdf' embedded resource as a base64 encoded string.
    /// </summary>
    public static string PdfAttachmentBase64 => PdfAttachmentPdfLazy.Value;

    /// <summary>
    /// Gets the 'Sample.MIME.content.eml' embedded resource as a stream.
    /// </summary>
    public static Stream MimeEmailContent => new MemoryStream(EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{ResourceNamespace}.Embedded.Sample.MIME.content.eml", typeof(EmbeddedResources)));

    /// <summary>
    /// Gets the 'Sample.MIME.attachment.content.eml' embedded resource as a stream.
    /// </summary>
    public static Stream MimeAttachmentContent => new MemoryStream(EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary($"{ResourceNamespace}.Embedded.Sample.MIME.attachment.content.eml", typeof(EmbeddedResources)));

    /// <summary>
    /// Creates a <see cref="Message"/> from <c>EmailTemplateJson</c>.
    /// </summary>
    /// <param name="id">The ID of the message.</param>
    /// <param name="mailboxId">The ID of the mailbox.</param>
    /// <param name="eTag">The message's ETag.</param>
    /// <param name="hasAttachments">If <c>true</c> the message has attachments; otherwise <c>false</c>.</param>
    /// <param name="senderEmailAddress">The from email address.</param>
    /// <param name="senderName">The from name.</param>
    /// <param name="subject">The email subject. Defaults to "test email".</param>
    /// <returns>The <see cref="Message"/>.</returns>
    public static Message CreateMessageFromEmailTemplateJson(
        string id,
        string mailboxId,
        string eTag,
        bool hasAttachments = false,
        string senderEmailAddress = "AdeleV@zpkw1.onmicrosoft.com",
        string senderName = "Adele Vance",
        string subject = "Test email")
    {
        string token;
        token = "$(Id)";
        string json = EmailTemplateJson.Replace(token, id, StringComparison.OrdinalIgnoreCase);

        token = "$(IdUrlEncoded)";
        json = json.Replace(token, System.Net.WebUtility.UrlEncode(id), StringComparison.OrdinalIgnoreCase);

        token = "$(MailboxId)";
        json = json.Replace(token, mailboxId, StringComparison.OrdinalIgnoreCase);

        token = "$(SenderEmailAddress)";
        json = json.Replace(token, senderEmailAddress, StringComparison.OrdinalIgnoreCase);

        token = "$(SenderName)";
        json = json.Replace(token, senderName, StringComparison.OrdinalIgnoreCase);

        token = "$(Subject)";
        json = json.Replace(token, subject, StringComparison.OrdinalIgnoreCase);

        token = "$(ETag)";
        string encodedETag = eTag.Replace("\"", "\\u0022", StringComparison.Ordinal);
        json = json.Replace(token, encodedETag, StringComparison.OrdinalIgnoreCase);

        if (hasAttachments)
        {
            token = "\"hasAttachments\": false";
            json = json.Replace(token, "\"hasAttachments\": true", StringComparison.OrdinalIgnoreCase);
        }

        using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(json));
        IParseNode parseNode = ParseNodeFactory.GetRootParseNodeAsync(CoreConstants.MimeTypeNames.Application.Json, memoryStream).ConfigureAwait(true).GetAwaiter().GetResult();
        Message? message = parseNode.GetObjectValue(Message.CreateFromDiscriminatorValue);
        return message!;
    }

    /// <summary>
    /// Creates a <see cref="Message"/> from <c>EmailTemplateJson</c>.
    /// </summary>
    /// <param name="id">The ID of the message.</param>
    /// <param name="mailboxId">The ID of the mailbox.</param>
    /// <param name="eTag">The message's ETag.</param>
    /// <param name="attachmentNames">The names to use when attaching files. This also dictates the number of attachments.</param>
    /// <param name="attachmentContentTypes">The contentTypes to use when attaching files. This also dictates the number of attachments.</param>
    /// <param name="senderEmailAddress">The from email address.</param>
    /// <param name="senderName">The from name.</param>
    /// <param name="subject">The email subject. Defaults to "test email".</param>
    /// <returns>The <see cref="Message"/>.</returns>
    public static Message CreateMessageFromEmailTemplateWithAttachmentsJson(
    string id,
    string mailboxId,
    string eTag,
    string[] attachmentNames,
    string[] attachmentContentTypes,
    string senderEmailAddress = "AdeleV@zpkw1.onmicrosoft.com",
    string senderName = "Adele Vance",
    string subject = "Test email")
    {
        string token;
        token = "$(Id)";
        string json = EmailTemplateJson.Replace(token, id, StringComparison.OrdinalIgnoreCase);

        token = "$(IdUrlEncoded)";
        json = json.Replace(token, System.Net.WebUtility.UrlEncode(id), StringComparison.OrdinalIgnoreCase);

        token = "$(MailboxId)";
        json = json.Replace(token, mailboxId, StringComparison.OrdinalIgnoreCase);

        token = "$(SenderEmailAddress)";
        json = json.Replace(token, senderEmailAddress, StringComparison.OrdinalIgnoreCase);

        token = "$(SenderName)";
        json = json.Replace(token, senderName, StringComparison.OrdinalIgnoreCase);

        token = "$(Subject)";
        json = json.Replace(token, subject, StringComparison.OrdinalIgnoreCase);

        token = "$(ETag)";
        string encodedETag = eTag.Replace("\"", "\\u0022", StringComparison.Ordinal);
        json = json.Replace(token, encodedETag, StringComparison.OrdinalIgnoreCase);

        // Check if we have attachments to add
        if (attachmentNames.Length > 0)
        {
            // Update hasAttachments to true
            json = json.Replace("\"hasAttachments\": false", "\"hasAttachments\": true", StringComparison.OrdinalIgnoreCase);

            // Start building the attachments JSON array
            StringBuilder attachmentsJson = new StringBuilder("\"attachments\": [");

            for (int i = 0; i < attachmentNames.Length; i++)
            {
                string? attachmentName = attachmentNames[i];
                string? attachmentContentType = attachmentContentTypes[i];

                string base64Content = PdfAttachmentBase64;
                string attachmentJson = $"{{\"@odata.type\": \"#microsoft.graph.fileAttachment\",\"name\": \"{attachmentName}\",\"contentBytes\": \"{base64Content}\",\"contentType\": \"{attachmentContentType}\"}},";

                attachmentsJson.Append(attachmentJson);
            }

            // Remove the trailing comma and close the array
            if (attachmentsJson[attachmentsJson.Length - 1] == ',')
            {
                attachmentsJson.Length--;
            }

            attachmentsJson.Append(']');

            // Insert the attachments array before the final closing brace in the JSON
            json = json.Insert(json.LastIndexOf("}"), $",{attachmentsJson}");
        }

        using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(json));
        IParseNode parseNode = ParseNodeFactory.GetRootParseNodeAsync(CoreConstants.MimeTypeNames.Application.Json, memoryStream)
            .ConfigureAwait(true)
            .GetAwaiter()
            .GetResult();

        Message? message = parseNode.GetObjectValue(Message.CreateFromDiscriminatorValue);
        return message!;
    }
}
