// <copyright file="IGraphApiEmailClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System.Threading.Tasks;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Microsoft.Graph.Models;

/// <summary>
/// The interface for a Graph API email client.
/// </summary>
public interface IGraphApiEmailClient
{
    /// <summary>
    /// Gets the Graph API client.
    /// </summary>
    IGraphApiClient GraphApiClient { get; }

    /// <summary>
    /// Converts a MIME stream to an Outlook MSG file stream.
    /// </summary>
    /// <param name="mimeStream">The message as a MIME stream.</param>
    /// <param name="removeAttachments">Set to true to remove attachments; otherwise attachments will be included in the MSG file.</param>
    /// <returns>An Outlook MSG file stream.</returns>
    Stream ConvertMimeToOutlookMsg(Stream mimeStream, bool removeAttachments = false);

    /// <summary>
    /// Creates a folder in a mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox identifier.</param>
    /// <param name="folderName">Then name of the folder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mail folder created.</returns>
    Task<MailFolder> CreateFolderAsync(
        string mailboxId,
        string folderName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the MIME content for an email attachment. MIME content is relevant when an email message is attached to another email message.
    /// The attachment will be in the MIME format.
    /// </summary>
    /// <param name="identifier">The message identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream that can be used to access the attachment content.</returns>
    /// <remarks>The content type of the attachment needs to be message/rfc822 (EML - MIME content) otherwise a <see cref="NotSupportedException"/> is raised.</remarks>
    /// <exception cref="NotSupportedException">Thrown if the attachment content type is not message/rfc822.</exception>
    Task<Stream> GetAttachmentMimeContentAsync(
        MailMessageIdentifier identifier,
        string attachmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an email message if it exists.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The message if it exists; otherwise returns <c>null</c>.</returns>
    Task<Message?> GetEmailMessageAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the information for an email message if it exists.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The message information if it exists; otherwise returns <c>null</c>.</returns>
    Task<MailMessageInfo?> GetEmailMessageInfoAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the email attachments for a message.
    /// </summary>
    /// <param name="message">The message to get attachments for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of attachments.</returns>
    Task<IReadOnlyCollection<Attachment>> GetEmailMessageAttachmentsAsync(
        Message message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the email content in its MIME format if the email exists.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The MIME content as a <see cref="Stream"/> if the email exists; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// MIME is an industry email standard. Many email applications create messages in MIME format and save them in files with the .EML extension.
    /// See: https://learn.microsoft.com/en-us/graph/outlook-get-mime-message.
    /// </remarks>
    Task<Stream?> GetEmailMimeContentAsync(
        MailMessageIdentifier identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the contents of an email file attachment that is not a MIME message file.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="attachmentId">The attachment ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The file attachment as a byte array.</returns>
    /// <exception cref="NotSupportedException">Thrown if the attachment content type is not a file attachment.</exception>
    Task<byte[]> GetFileAttachmentContentAsync(
        MailMessageIdentifier identifier,
        string attachmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the extended property correspondence ID expand clause.
    /// </summary>
    /// <returns>The extended property correspondence ID expand clause.</returns>
    string GetExtendedPropertyCorrespondenceIdExpandClause();

    /// <summary>
    /// Gets the correspondence ID extended property from a message.
    /// </summary>
    /// <param name="message">The message to get the extended property for.</param>
    /// <returns>Returns the correspondence ID if found; otherwise returns <c>null</c>.</returns>
    CorrespondenceIdExtendedProperty? GetExtendedPropertyCorrespondenceIdFromMessage(Message message);

    /// <summary>
    /// Gets the identities of the folders in a mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox identity.</param>
    /// <param name="folderNames">The folder names to identify in the mailbox.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns a dictionary where the key is the folder name, and the value is the folder identity.</returns>
    Task<IReadOnlyDictionary<string, string>> GetMailboxFolderIdentitiesAsync(
        string mailboxId,
        IEnumerable<string> folderNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the identity of a folder in a mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox identity.</param>
    /// <param name="folderName">The folder name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the folder identity if found; otherwise returns <c>null</c>.</returns>
    Task<string?> GetMailboxFolderIdentityAsync(
        string mailboxId,
        string folderName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an email message to a folder.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="folderId">The identity of the folder to move to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The moved email message.</returns>
    Task<Message> MoveMessageToFolderAsync(
        MailMessageIdentifier identifier,
        string folderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reply to an email message.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="messageText">The reply message text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    /// <remarks>When setting <paramref name="messageText"/>; note that the email body is HTML, use breaks (&lt;br&gt;) for end-of-line.</remarks>
    Task ReplyAsync(
        MailMessageIdentifier identifier,
        string messageText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the correspondence ID extended property on an email message.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="correspondenceId">The correspondence ID extended property.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The patched message.</returns>
    Task<Message> SetExtendedPropertyCorrespondenceIdAsync(
        MailMessageIdentifier identifier,
        CorrespondenceIdExtendedProperty correspondenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the read status of an email message.
    /// </summary>
    /// <param name="identifier">The email message identifier.</param>
    /// <param name="isRead">Set to <c>true</c> to mark the email as read; otherwise set to <c>false</c> to mark the email as unread.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The patched message.</returns>
    Task<Message> SetMessageReadStatusAsync(
        MailMessageIdentifier identifier,
        bool isRead,
        CancellationToken cancellationToken = default);
}
