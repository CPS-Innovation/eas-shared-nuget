// <copyright file="GraphApiEmailClientTest.ExtendedProperties.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System;
using System.Globalization;
using System.Net;
using Cps.Fct.EasShared.Core;
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
    private const string ResourceNamespace = "Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests.Embedded";

    /// <summary>
    /// Getting the extended property correspondence ID expand clause succeeds.
    /// </summary>
    [Fact]
    public void GettingTheExtendedPropertyCorrespondenceIdExpandClauseSucceeds()
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        string expandClause = client.GetExtendedPropertyCorrespondenceIdExpandClause();

        // Assert
        expandClause.Should().NotBeNull();
        expandClause.Should().Be("singleValueExtendedProperties($filter=id eq 'String {80f98438-6260-4f48-baf2-d37336ca4dd7} Name CorrespondenceId_v1')");
    }

    /// <summary>
    /// Getting the extended property correspondence ID from a message succeeds.
    /// </summary>
    /// <param name="resourceName">The JSON embedded resource name.</param>
    /// <param name="eventTime">The expected event time.</param>
    /// <param name="expectedCorrespondenceId">The expected correspondence ID.</param>
    [Theory]
    [InlineData("Message.Empty.json", "2023-03-01T14:11:48.334Z", null)]
    [InlineData("Message.ExtendedProperty.json", "2023-03-01T14:11:48.334Z", "03fa14a6-0d0f-4a2c-8481-05abe73a1f5c")]
    public void GettingTheExtendedPropertyCorrespondenceIdFromAMessageSucceeds(
        string resourceName,
        string eventTime,
        string? expectedCorrespondenceId)
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        string json = EmbeddedResourceUtility.LoadEmbeddedResource($"{ResourceNamespace}.{resourceName}", typeof(GraphApiEmailClientTest));
        using ParseNodeFactory factory = new();
        Message message = factory.CreateParseNode(json).GetObjectValue(Message.CreateFromDiscriminatorValue)!;

        // Act
        CorrespondenceIdExtendedProperty? correspondenceId = client.GetExtendedPropertyCorrespondenceIdFromMessage(message!);

        // Assert
        correspondenceId.Should().Be(expectedCorrespondenceId == null ? null : new CorrespondenceIdExtendedProperty(new Guid(expectedCorrespondenceId), DateTimeOffset.Parse(eventTime, CultureInfo.InvariantCulture)));
    }

    /// <summary>
    /// Setting the correspondence ID extended property succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task SettingTheCorrespondenceIdExtendedPropertySucceeds()
    {
        // Assemble
        CorrespondenceIdExtendedProperty correspondenceId = new(new Guid("c383934f-af75-42e3-beb4-fbda6d99ab0c"), DateTimeOffset.Now);
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Message message = await client.SetExtendedPropertyCorrespondenceIdAsync(identifier, correspondenceId).ConfigureAwait(true);

        // Assert
        message.Should().NotBeNull();
    }

    /// <summary>
    /// Setting the correspondence ID extended property fails when the message does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task SettingTheCorrespondenceIdExtendedPropertyFailsWhenTheMessageDoesNotExist()
    {
        // Assemble
        CorrespondenceIdExtendedProperty correspondenceId = new(new Guid("d383934f-af75-42e3-beb4-fbda6d99ab0c"), DateTimeOffset.Now);
        MailMessageIdentifier identifier = new(correspondenceId.ToString(), "Does.not.exist");
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        ODataError exception = await Assert.ThrowsAsync<ODataError>(async () =>
        {
            await client.SetExtendedPropertyCorrespondenceIdAsync(identifier, correspondenceId).ConfigureAwait(true);
        }).ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
}
