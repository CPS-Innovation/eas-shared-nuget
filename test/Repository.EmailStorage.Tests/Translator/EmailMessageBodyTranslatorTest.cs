// <copyright file="EmailMessageBodyTranslatorTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Tests.Translator;

using System;
using System.Reflection;
using System.Text;
using Cps.Fct.EasShared.Repository.EmailStorage.Translator;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft.Graph.Models;

/// <summary>
/// Tests the <see cref="EmailMessageBodyTranslator"/> class.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EmailMessageBodyTranslatorTest
{
    /// <summary>
    /// Gets or sets a value indicating whether file output is enabled.
    /// </summary>
    public static bool FileOutputEnabled { get; set; } = true;

    /// <summary>
    /// The <see cref="EmailMessageBodyTranslator"/> throws a <see cref="NotSupportedException"/> when translating an email message without any content.
    /// </summary>
    [Fact]
    public void EmailMessageBodyTranslatorThrowsNotSupportedException()
    {
        // Arrange
        EmailMessageBodyTranslator translator = new();

        // Act
        string Act() => translator.Translate(new Message());

        // Assert
        NotSupportedException exception = Assert.Throws<NotSupportedException>((Func<string>)Act);
        exception.Message.Should().Be("Translating an email message without any content is not supported.");
    }

    /// <summary>
    /// The <see cref="EmailMessageBodyTranslator"/> succeeds when translating HTML content.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    [Theory]
    [InlineData("a@b.com", "A URN 12AB1234523 in the subject")]
    public void EmailMessageBodyTranslatorSucceedsWhenTranslatingHtmlContent(string emailAddress, string subject)
    {
        // Arrange
        EmailMessageBodyTranslator translator = new();
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            senderEmailAddress: emailAddress,
            subject: subject);

        // Act
        string bodyContent = translator.Translate(message);

        // Assert
        bodyContent.Should().NotBeNullOrWhiteSpace();
        bodyContent.Should().Contain(emailAddress);
        bodyContent.Should().Contain(subject);
        WriteToFile($"{nameof(this.EmailMessageBodyTranslatorSucceedsWhenTranslatingHtmlContent)}.html", bodyContent);
    }

    /// <summary>
    /// The <see cref="EmailMessageBodyTranslator"/> succeeds when translating Text content.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    [Theory]
    [InlineData("a@b.com", "A URN 12AB1234523 in the subject")]
    public void EmailMessageBodyTranslatorSucceedsWhenTranslatingTextContent(string emailAddress, string subject)
    {
        // Arrange
        EmailMessageBodyTranslator translator = new();
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            senderEmailAddress: emailAddress,
            subject: subject);
        message.Body!.Content = "Body text";

        // Act
        string bodyContent = translator.Translate(message);

        // Assert
        bodyContent.Should().NotBeNullOrWhiteSpace();
        bodyContent.Should().Contain(emailAddress);
        bodyContent.Should().Contain(subject);
        WriteToFile($"{nameof(this.EmailMessageBodyTranslatorSucceedsWhenTranslatingTextContent)}.html", bodyContent);
    }

    /// <summary>
    /// The <see cref="EmailMessageBodyTranslator"/> succeeds when translating Text content with special characters.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    [Theory]
    [InlineData("a@b.com", "A URN 12AB1234523 in the subject")]
    public void EmailMessageBodyTranslatorSucceedsWhenTranslatingTextContentWithSpecialCharacters(string emailAddress, string subject)
    {
        // Arrange
        EmailMessageBodyTranslator translator = new();
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            senderEmailAddress: emailAddress,
            subject: subject);
        message.Body!.Content = @"This test contains text that needs to be HTML encoded:
X & Y
- A < B
- B > A

Unicode text:
 - ©
 - ç
 - €";

        // Act
        string bodyContent = translator.Translate(message);

        // Assert
        bodyContent.Should().NotBeNullOrWhiteSpace();
        bodyContent.Should().Contain(emailAddress);
        bodyContent.Should().Contain(subject);
        bodyContent.Should().Contain("X &amp; Y");
        bodyContent.Should().Contain("- A &lt; B");
        bodyContent.Should().Contain("- B &gt; A");
        bodyContent.Should().NotContain("©");
        bodyContent.Should().NotContain("ç");
        bodyContent.Should().NotContain("€");
        WriteToFile($"{nameof(this.EmailMessageBodyTranslatorSucceedsWhenTranslatingTextContentWithSpecialCharacters)}.html", bodyContent);
    }

    private static void WriteToFile(string filename, string content)
    {
        if (FileOutputEnabled)
        {
            string parentDir = Assembly.GetExecutingAssembly().Location;
            string dirName = Path.Combine(parentDir, $"../../../../Output");
            DirectoryInfo dirInfo = new(dirName);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            filename = Path.Combine(dirInfo.FullName, filename);
            File.WriteAllText(filename, content, encoding: Encoding.UTF8);
        }
    }
}
