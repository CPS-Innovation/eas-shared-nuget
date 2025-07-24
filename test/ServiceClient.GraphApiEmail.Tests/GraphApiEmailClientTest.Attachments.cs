// <copyright file="GraphApiEmailClientTest.Attachments.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System.Reflection;
using System.Security.Cryptography;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests for the Graph API email client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiEmailClientTest
{
    private const string MimeAttachmentKey = "MimeAttachments";
    private const string MimeAttachmentId = "fe4b69ef-7c18-496a-b8c6-d98f0a38605b";

    /// <summary>
    /// Getting an email message with no attachments succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnEmailMessageWithNoAttachmentsSucceeds()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        Message? message = await client.GetEmailMessageAsync(identifier).ConfigureAwait(true);
        Assumes.NotNull(message);

        // Act
        IReadOnlyCollection<Attachment> attachments = await client.GetEmailMessageAttachmentsAsync(message).ConfigureAwait(true);

        // Assert
        attachments.Should().BeEmpty();
    }

    /// <summary>
    /// Getting an email message with attachments succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnEmailMessageWithAttachmentsSucceeds()
    {
        // Assemble
        string resource = AddTestMessageToCache(hasAttachments: true);
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        Message? message = await client.GetEmailMessageAsync(identifier).ConfigureAwait(true);
        Assumes.NotNull(message);

        // Act
        IReadOnlyCollection<Attachment> attachments = await client.GetEmailMessageAttachmentsAsync(message).ConfigureAwait(true);

        // Assert
        attachments.Should().NotBeEmpty();
    }

    /// <summary>
    /// Getting the file attachment content for an email succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingFileAttachmentContentSucceeds()
    {
        // Assemble
        string resource = AddTestMessageToCache(hasAttachments: true);
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        Message? message = await client.GetEmailMessageAsync(identifier).ConfigureAwait(true);
        Assumes.NotNull(message);
        IReadOnlyCollection<Attachment> attachments = await client.GetEmailMessageAttachmentsAsync(message).ConfigureAwait(true);
        attachments.Should().NotBeEmpty();
        Dictionary<string, byte[]> contents = new();
        Dictionary<string, MailMessageAttachment> attachmentDictionary = new();

        // Act
        foreach (Attachment attachment in attachments)
        {
            string id = attachment.Id!;
            contents.Add(id, await client.GetFileAttachmentContentAsync(identifier, id).ConfigureAwait(true));
            attachmentDictionary.Add(id, new MailMessageAttachment(attachment));
        }

        // Assert
        contents.Should().NotBeEmpty();
        foreach (KeyValuePair<string, byte[]> item in contents)
        {
            item.Value.Length.Should().Be(attachmentDictionary[item.Key].Size);
        }
    }

    /// <summary>
    /// Getting file  attachment content fails.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingFileAttachmentContentFails()
    {
        // Assemble
        string resource = AddTestMessageToCache(hasAttachments: true);
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await client.GetFileAttachmentContentAsync(identifier, "not-supported").ConfigureAwait(true);
        }).ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
    }

    /// <summary>
    /// Getting an attachment that is an email attachment (MIME format) succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAttachmentAsMimeContentSucceeds()
    {
        // Assemble
        string resource = AddTestMessageToCache(hasAttachments: true);
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Stream mimeContent = await client.GetAttachmentMimeContentAsync(identifier, MimeAttachmentId).ConfigureAwait(true);

        // Assert
        mimeContent.Should().NotBeNull();
        mimeContent!.Position.Should().Be(0);
        mimeContent!.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Getting attachment as MIME content fails.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAttachmentAsMimeContentFails()
    {
        // Assemble
        string resource = AddTestMessageToCache(hasAttachments: true);
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await client.GetAttachmentMimeContentAsync(identifier, "not-supported").ConfigureAwait(true);
        }).ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
    }

    /// <summary>
    /// Generating file attachments succeeds.
    /// </summary>
    /// <remarks>This test can be used to create any number of attachments with a specific size.</remarks>
    [Fact]
    public void GenerateFileAttachments()
    {
        // Assemble
        int numberOfFiles = 1;
        int fileSize = 1024;
        DirectoryInfo dirInfo = new(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Output"));
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        // Act
        List<string> filenames = new();
        for (int i = 0; i < numberOfFiles; i++)
        {
            string filename = Path.Combine(dirInfo.FullName, $"test.{i:000}.bin");
            GenerateFile(filename, fileSize);
            filenames.Add(filename);
        }

        // Assert
        filenames.Any(f => !System.IO.File.Exists(f)).Should().BeFalse();
    }

    private static void GenerateFile(string path, long size)
    {
        Requires.NotNullOrWhiteSpace(path);
        Requires.Range(size > 0, nameof(size));

        int bufferSize = 4 * 1024;
        if (bufferSize > size)
        {
            bufferSize = (int)size;
        }

        byte[] buffer = GenerateRandomBuffer(bufferSize);
        long bytesToWrite = size;
        int fileBufferSize = 64 * 1024;
        if (fileBufferSize > size)
        {
            fileBufferSize = (int)size;
        }

        using FileStream fileStream = new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, fileBufferSize, FileOptions.None);
        while (bytesToWrite > 0)
        {
            long batchSize = buffer.Length;
            if (bytesToWrite < batchSize)
            {
                batchSize = bytesToWrite;
            }

            fileStream.Write(buffer, 0, (int)batchSize);
            bytesToWrite -= batchSize;
        }
    }

    private static byte[] GenerateRandomBuffer(int size)
    {
        byte[] result = new byte[size];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(result);
        return result;
    }
}
