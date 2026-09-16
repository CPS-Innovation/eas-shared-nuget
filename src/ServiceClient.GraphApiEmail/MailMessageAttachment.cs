// <copyright file="MailMessageAttachment.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

/// <summary>
/// Basic information about an email attachment.
/// </summary>
/// <param name="ContentType">The content MIME type.</param>
/// <param name="Name">The name of the attachment.</param>
/// <param name="ODataType">The OData type of the attachment.</param>
/// <param name="Size">The size in bytes of the attachment.</param>
public record MailMessageAttachment(
    string ContentType,
    string Name,
    string ODataType,
    int Size)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MailMessageAttachment"/> class.
    /// </summary>
    /// <param name="attachment">The Graph API attachment.</param>
    public MailMessageAttachment(Attachment attachment)
        : this(ContentType: attachment.ContentType!, Name: attachment.Name!, ODataType: attachment.OdataType!, Size: attachment.Size ?? 0)
    {
    }
}
