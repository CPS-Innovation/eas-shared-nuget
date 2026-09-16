// <copyright file="GraphApiEmailClientTest.Folders.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// Tests for the Graph API email client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiEmailClientTest
{
    /// <summary>
    /// Get mailbox folder identities succeeds.
    /// </summary>
    /// <param name="mailboxId">The mailbox identity.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData("61c52668-bc1c-47b9-bfb8-2614a4aa1c11")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com")]
    [InlineData("stagingsmb1@zpkw1.onmicrosoft.com")]
    public async Task GetMailboxFolderIdentitiesSucceeds(string mailboxId)
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        MailboxFolderOptions mailboxFolderOptions = this.ServiceProvider.GetRequiredService<IOptions<MailboxFolderOptions>>().Value;
        string[] folderNames = new string[]
        {
            mailboxFolderOptions.ExceptionsFolderName,
            mailboxFolderOptions.ExcludedFolderName,
            mailboxFolderOptions.ProcessedFolderName,
            mailboxFolderOptions.ProcessingFolderName,
            mailboxFolderOptions.UrgentFolderName,
            "Inbox",
        };
        foreach (string folderName in folderNames)
        {
            string? folderId = await client.GetMailboxFolderIdentityAsync(mailboxId, folderName).ConfigureAwait(true);
            if (folderId is null)
            {
                await client.CreateFolderAsync(mailboxId, folderName).ConfigureAwait(true);
            }
        }

        // Act
        IReadOnlyDictionary<string, string> identifiers = await client.GetMailboxFolderIdentitiesAsync(mailboxId, folderNames).ConfigureAwait(true);

        // Assert
        identifiers.Should().NotBeEmpty();
        identifiers.Count.Should().Be(folderNames.Length);
        foreach (string folderName in folderNames)
        {
            identifiers.Should().ContainKey(folderName);
            identifiers[folderName].Should().NotBeNullOrWhiteSpace();
        }
    }

    /// <summary>
    /// Moving an email message succeeds for a message that exists.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task MovingAnEmailMessageSucceedsForAMessageThatExists()
    {
        // Assemble
        string id = AddTestMessageToCache();
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Message? actualMessage = await client.MoveMessageToFolderAsync(MailMessageIdentifier.Parse(id), "archive").ConfigureAwait(true);

        // Assert
        actualMessage.Should().NotBeNull();
    }

    /// <summary>
    /// Moving an email message fails for a message that does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task MovingAnEmailMessageFailsForAMessageThatDoesNotExist()
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        ODataError exception = await Assert.ThrowsAsync<ODataError>(async () =>
        {
            await client.MoveMessageToFolderAsync(MailMessageIdentifier.Parse($"Users/{Guid.Empty}/Messages/does.not.exist"), "archive").ConfigureAwait(true);
        })
        .ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
        exception.ResponseStatusCode.Should().Be((int)System.Net.HttpStatusCode.NotFound);
    }
}
