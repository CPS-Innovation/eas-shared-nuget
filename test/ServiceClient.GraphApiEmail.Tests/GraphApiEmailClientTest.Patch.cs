// <copyright file="GraphApiEmailClientTest.Patch.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System;
using System.Net;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models.ODataErrors;

/// <summary>
/// Tests for the Graph API email client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiEmailClientTest
{
    /// <summary>
    /// Marking an email as read succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task MarkingAnEmailAsReadSucceeds()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Message message = await client.SetMessageReadStatusAsync(identifier, isRead: true).ConfigureAwait(true);

        // Assert
        message.Should().NotBeNull();
        message.IsRead.Should().BeTrue();
    }

    /// <summary>
    /// Marking an email as read fails when the message does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task MarkingAnEmailAsReadFailsWhenTheMessageDoesNotExist()
    {
        // Assemble
        MailMessageIdentifier identifier = new(Guid.NewGuid().ToString(), "Does.not.exist");
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        ODataError exception = await Assert.ThrowsAsync<ODataError>(async () =>
        {
            await client.SetMessageReadStatusAsync(identifier, isRead: true).ConfigureAwait(true);
        }).ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
}
