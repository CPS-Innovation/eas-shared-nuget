// <copyright file="EmbeddedResourcesTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft.Kiota.Serialization.Json;

/// <summary>
/// Tests <see cref="EmbeddedResources"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EmbeddedResourcesTest
{
    /// <summary>
    /// Loading the embedded resource 'Email.template.json' succeeds.
    /// </summary>
    [Fact]
    public void LoadingEmbeddedResourceEmailTemplateJsonSucceeds()
    {
        // Act
        string content = EmbeddedResources.EmailTemplateJson;

        // Assert
        content.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Creating a <see cref="Message"/> with no attachments from <c>EmailTemplateJson</c> succeeds.
    /// </summary>
    [Fact]
    public void CreatingAMessageWithNoAttachmentsFromEmailTemplateJsonSucceeds()
    {
        // Assemble
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";

        // Act
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().Be(id);
        message.AdditionalData["@odata.etag"].ToString().Should().Be(eTag);

        using var jsonSerializerWriter = new JsonSerializationWriter();
        jsonSerializerWriter.WriteObjectValue(string.Empty, message);
        using Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader streamReader = new(serializedStream);
        string json = streamReader.ReadToEnd();

        json.Should().Contain(mailboxId);
        json.Should().Contain("\"hasAttachments\":false");
        json.Should().NotContain("$(");
    }

    /// <summary>
    /// Creating a <see cref="Message"/> with attachments from <c>EmailTemplateJson</c> succeeds.
    /// </summary>
    [Fact]
    public void CreatingAMessageWithAttachmentsFromEmailTemplateJsonSucceeds()
    {
        // Assemble
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        string subject = "Custom subject";

        // Act
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            hasAttachments: true,
            subject: subject);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().Be(id);
        message.AdditionalData["@odata.etag"].ToString().Should().Be(eTag);

        using var jsonSerializerWriter = new JsonSerializationWriter();
        jsonSerializerWriter.WriteObjectValue(string.Empty, message);
        using Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader streamReader = new(serializedStream);
        string json = streamReader.ReadToEnd();

        json.Should().Contain(mailboxId);
        json.Should().Contain("\"hasAttachments\":true");
        json.Should().Contain($"\"subject\":\"{subject}\"");
        json.Should().NotContain("$(");
    }

    /// <summary>
    /// Loading the embedded resource 'Sample.MIME.content.eml' succeeds.
    /// </summary>
    [Fact]
    public void LoadingEmbeddedResourceMimeEmailContentSucceeds()
    {
        // Act
        Stream content = EmbeddedResources.MimeEmailContent;

        // Assert
        content.Should().NotBeNull();
        content.Length.Should().BeGreaterThan(0);
    }
}
