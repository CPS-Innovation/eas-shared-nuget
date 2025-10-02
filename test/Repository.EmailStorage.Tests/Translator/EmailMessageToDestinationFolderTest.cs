// <copyright file="EmailMessageToDestinationFolderTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Tests.Translator;

using System;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Text;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Translator;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph.Models;
using Moq;

/// <summary>
/// Tests the <see cref="EmailMessageToDestinationFolder"/> class.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EmailMessageToDestinationFolderTest
{
    private readonly Mock<MailboxOptions> mailboxOptionsMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailMessageToDestinationFolderTest"/> class.
    /// </summary>
    public EmailMessageToDestinationFolderTest()
    {
        this.mailboxOptionsMock = new Mock<MailboxOptions>();

        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<EmailAddressExclusionOptions>("EmailAddressMatching")
                    .AddServiceOptions<EmailSubjectExclusionOptions>("EmailSubjectMatching");

                services
                    .AddSingleton<ITextMatcher, IgnoreCaseTextMatcher>()
                    .AddSingleton<EmailSubjectToUrnTranslator>()
                    .AddSingleton<EmailMessageToDestinationFolder>()
                    .AddSingleton<MailboxOptions>(this.mailboxOptionsMock.Object);
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Tests that the <see cref="EmailMessageToDestinationFolder"/> translates the email message to the correct mailbox folder name.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="expectedDestinationFolder">The expected destination folder.</param>
    /// <param name="reason">The reason to move the email to the folder.</param>
    /// <param name="urn">The expected URN.</param>
    [Theory]
    [InlineData("xhibitlists@justice.gov.uk", "N/A subject", "CMS.Excluded", "excluded email address", "Unspecified or invalid")]
    [InlineData("donotreply-cjsexhibit@cjsm.net", "N/A subject", "CMS.Excluded", "excluded email address", "Unspecified or invalid")]
    [InlineData("ccdcssupport@caselines.co.uk", "N/A subject", "CMS.Excluded", "excluded email address", "Unspecified or invalid")]
    [InlineData("a@news.richmondshire.gov.uk", "N/A subject", "CMS.Excluded", "excluded email address", "Unspecified or invalid")]
    [InlineData("michaellees679@gmail.com", "N/A subject", "CMS.Excluded", "excluded email address", "Unspecified or invalid")]

    [InlineData(null, "Auto: do not respond", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("", "Auto: do not respond", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData(" ", "Auto: do not respond", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]

    [InlineData("auto@b.com", "Auto: do not respond", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "Auto reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "Auto response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "Automatic reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "Automatic response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "Out of office: I'm out-of-office", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Auto reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Auto response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "[CJSM] Automatic reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Automatic response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "[CJSM] Out of office: I'm out-of-office", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "Auto-response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "Automatic-reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "Automatic-response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "I'm out-of-office", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Auto-reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Auto-response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "[CJSM] Automatic-reply: email", "CMS.Excluded", "excluded subject", "Unspecified or invalid")]
    [InlineData("auto@b.com", "[CJSM] Automatic-response: response URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]
    [InlineData("auto@b.com", "important please read and follow the instruction herein - do not just delete URN included 12AB1234523", "CMS.Excluded", "excluded subject", "12AB1234523")]

    [InlineData(null, "No URN in subject", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData("", "No URN in subject", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData("  ", "No URN in subject", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData("a@gmail.com", "No URN in subject", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]

    [InlineData(null, "Multiple URN's in subject 12AB1234523 13AB1234523", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData("", "Multiple URN's in subject 12AB1234523 13AB1234523", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData(" ", "Multiple URN's in subject 12AB1234523 13AB1234523", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]
    [InlineData("a@gmail.com", "Multiple URN's in subject 12AB1234523 13AB1234523", "CMS.Exceptions", "no URN or more than one found in subject", "Unspecified or invalid")]

    [InlineData("a@gmail.com", "A URN 12AB1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("a@gmail.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523")]

    [InlineData(null, "A URN 12ab1234523 in the subject", "CMS.Exceptions", "from email address unspecified", "12AB1234523")]
    [InlineData("", "A URN 12AB1234523 in the subject", "CMS.Exceptions", "from email address unspecified", "12AB1234523")]
    [InlineData(" ", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "from email address unspecified", "12AB1234523")]

    [InlineData("exception.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "exception email address", "12AB1234523")]

    [InlineData("postmaster@cps.gov.uk", "Auto: do not respond URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("postmaster@cps.gov.uk", "Auto reply: email URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("postmaster@cps.gov.uk", "Auto-response: response URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("postmaster@cps.gov.uk", "Automatic reply: email URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("postmaster@cps.gov.uk", "Automatic response: response URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("postmaster@cps.gov.uk", "Out of office: I'm out-of-office URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]

    [InlineData("MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk", "[CJSM] Auto reply: email URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk", "[CJSM] Auto response: response URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk", "[CJSM] Automatic reply: email URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk", "[CJSM] Automatic response: response URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    [InlineData("MicrosoftExchange329e71ec88ae4615bbc36ab6ce41109e@cps.gov.uk", "[CJSM] Out of office: I'm out-of-office URN included 12AB1234523", "CMS.Processing", "further processing required", "12AB1234523")]
    public void MailMessageTranslatesToTheCorrectMailboxFolderName(
        string? emailAddress,
        string subject,
        string expectedDestinationFolder,
        string reason,
        string urn)
    {
        // Arrange
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
           id: id,
           mailboxId: mailboxId,
           eTag: eTag,
           senderEmailAddress: emailAddress!,
           subject: subject);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        destinationFolderResult.FolderName.Should().Be(expectedDestinationFolder);
        destinationFolderResult.Reason.Should().Contain(reason);
        destinationFolderResult.Urn.Should().Be(urn);
    }

    /// <summary>
    /// Tests that the <see cref="EmailMessageToDestinationFolder"/> translates the email message to the exceptions mailbox folder name when attachment filename or content type is invalid.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="expectedDestinationFolder">The expected destination folder.</param>
    /// <param name="reason">The reason to move the email to the folder.</param>
    /// <param name="urn">The expected URN.</param>
    /// <param name="attachmentNames">A comma separated string for the names types of the attachments. This also dictates the number of attachments.</param>
    /// <param name="attachmentContentTypes">A comma separated string for the content types of the attachments. This also dictates the number of attachments.</param>
    [Theory]
    [InlineData("nofilename.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with no name", "12AB1234523", "", "application/pdf")]
    [InlineData("nofilename.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with no name", "12AB1234523", ",sample.attachment.pdf", "application/pdf,application/pdf")]
    [InlineData("nofilename.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with no name", "12AB1234523", ",sample.attachment.pdf,", "application/pdf,application/pdf,application/pdf")]
    [InlineData("incorrectcontenttype.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523", "sample.attachment.pdf", "")]
    [InlineData("incorrectcontenttype.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with unsupported content type", "12AB1234523", "sample.attachment.pdf", "text/rfc822-headers")]
    [InlineData("nofilename.test@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with no name", "12AB1234523", "", "text/rfc822-headers")]
    public void MailMessageTranslatesToExceptionsMailboxFolderNameWhenAttachmentNameOrContentTypeIsInvalid(
        string? emailAddress,
        string subject,
        string expectedDestinationFolder,
        string reason,
        string urn,
        string attachmentNames,
        string attachmentContentTypes)
    {
        // Arrange
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        string[] attachmentNamesArray = attachmentNames.Split(',');
        string[] attachmentContentTypesArray = attachmentContentTypes.Split(',');

        Message message = EmbeddedResources.CreateMessageFromEmailTemplateWithAttachmentsJson(
           id: id,
           mailboxId: mailboxId,
           eTag: eTag,
           senderEmailAddress: emailAddress!,
           subject: subject,
           attachmentNames: attachmentNamesArray,
           attachmentContentTypes: attachmentContentTypesArray);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        destinationFolderResult.FolderName.Should().Be(expectedDestinationFolder);
        destinationFolderResult.Reason.Should().Contain(reason);
        destinationFolderResult.Urn.Should().Be(urn);
    }

    /// <summary>
    /// Mail message translates to an exception for an insecure URL in the email body.
    /// </summary>
    /// <param name="insecureUrl">The insecure URL to search for in the email body that is the cause of the exception.</param>
    [Theory]
    [InlineData("http://www.cps.gov.uk")]
    [InlineData("http://www.cps.gov.uk/London")]
    [InlineData("HTTP://WWW.CPS.GOV.UK")]
    public void MailMessageTranslatesToExceptionFolderForInsecureUrlInTheEmailBodyWhenMailboxRequiresEmailAsMsgFileIsTrue(string insecureUrl)
    {
        // Arrange
        this.mailboxOptionsMock
            .Setup(m => m.RequiresEmailAsMsgFile(It.IsAny<string>()))
            .Returns(() => true);

        string emailAddress = "a@b.com";
        string subject = "A URN 12AB1234523 in the subject";

        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            senderEmailAddress: emailAddress,
            subject: subject);
        message.Body!.Content = message.Body!.Content!.Replace("writing guidelines", insecureUrl, StringComparison.OrdinalIgnoreCase);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        message.Body!.Content.Should().Contain(insecureUrl);
        destinationFolderResult.FolderName.Should().Be("CMS.Exceptions");
        destinationFolderResult.Reason.Should().Contain("insecure URL detected in message body");
    }

    /// <summary>
    /// Mail message translates to an exception for an insecure URL in the email body.
    /// </summary>
    /// <param name="insecureUrl">The insecure URL to search for in the email body that is the cause of the exception.</param>
    [Theory]
    [InlineData("http://www.cps.gov.uk")]
    [InlineData("http://www.cps.gov.uk/London")]
    [InlineData("HTTP://WWW.CPS.GOV.UK")]
    public void MailMessageTranslatesToProcesingFolderForInsecureUrlInTheEmailBodyWhenMailboxRequiresEmailAsMsgFileIsFalse(string insecureUrl)
    {
        // Arrange
        string emailAddress = "a@b.com";
        string subject = "A URN 12AB1234523 in the subject";

        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message message = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            senderEmailAddress: emailAddress,
            subject: subject);
        message.Body!.Content = message.Body!.Content!.Replace("writing guidelines", insecureUrl, StringComparison.OrdinalIgnoreCase);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        message.Body!.Content.Should().Contain(insecureUrl);
        destinationFolderResult.FolderName.Should().Be("CMS.Processing");
        destinationFolderResult.Reason.Should().Contain("further processing required");
    }

    /// <summary>
    /// Tests that the <see cref="EmailMessageToDestinationFolder"/> translates the email message to the correct mailbox folder name when attachment filename extension length is checked.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="expectedDestinationFolder">The expected destination folder.</param>
    /// <param name="reason">The reason to move the email to the folder.</param>
    /// <param name="urn">The expected URN.</param>
    /// <param name="attachmentNames">A comma separated string for the names types of the attachments. This also dictates the number of attachments.</param>
    /// <param name="attachmentContentTypes">A comma separated string for the content types of the attachments. This also dictates the number of attachments.</param>
    [Theory]
    [InlineData("incorrectfileextensionlength@b.com", "A URN 12ab1234523 in the subject", "CMS.Exceptions", "email has attachments with file extension longer than 4 characters", "12AB1234523", "sample.attachment.pdf12", "application/pdf")]
    [InlineData("validfileextensionlength@b.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523", "sample.attachment.xlsx", "application/pdf")]
    [InlineData("anothervalidfileextensionlength@b.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523", "sample.attachment.xls", "application/pdf")]
    [InlineData("validfileWithOutlookAttachment@b.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523", "Re: CPS ref: 88ST3997714E1. Bob v Peter", "application/vnd.ms-outlook")]
    public void MailMessageTranslatesToCorrectMailboxFolderNameWhenAttachmentFileExtensionLengthIsChecked(
        string? emailAddress,
        string subject,
        string expectedDestinationFolder,
        string reason,
        string urn,
        string attachmentNames,
        string attachmentContentTypes)
    {
        // Arrange
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        string[] attachmentNamesArray = attachmentNames.Split(',');
        string[] attachmentContentTypesArray = attachmentContentTypes.Split(',');

        Message message = EmbeddedResources.CreateMessageFromEmailTemplateWithAttachmentsJson(
           id: id,
           mailboxId: mailboxId,
           eTag: eTag,
           senderEmailAddress: emailAddress!,
           subject: subject,
           attachmentNames: attachmentNamesArray,
           attachmentContentTypes: attachmentContentTypesArray);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        destinationFolderResult.FolderName.Should().Be(expectedDestinationFolder);
        destinationFolderResult.Reason.Should().Contain(reason);
        destinationFolderResult.Urn.Should().Be(urn);
    }

    /// <summary>
    /// Tests that the <see cref="EmailMessageToDestinationFolder"/> translates the email message to the correct mailbox folder name when attachment total size is checked.
    /// </summary>
    /// <param name="emailAddress">The from email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="expectedDestinationFolder">The expected destination folder.</param>
    /// <param name="reason">The reason to move the email to the folder.</param>
    /// <param name="urn">The expected URN.</param>
    /// <param name="attachmentNames">A comma separated string for the names types of the attachments. This also dictates the number of attachments.</param>
    /// <param name="attachmentContentTypes">A comma separated string for the content types of the attachments. This also dictates the number of attachments.</param>
    [Theory]
    [InlineData("attachmentFilesizeExceeded@b.com", "A URN 12ab1234521 in the subject", "CMS.Exceptions", "email has attachments with size", "12AB1234521", "sample.attachment.pdf1", "application/pdf", "22000000")]
    [InlineData("attachmentFilesizeTotalExceeded@b.com", "A URN 12ab1234522 in the subject", "CMS.Exceptions", "email has attachments with size", "12AB1234522", "sample.attachment.pdf1,myNotes.pdf", "application/pdf,application/pdf", "15000000,8000000")]
    [InlineData("attachmentFilesizeTotalValid@b.com", "A URN 12ab1234523 in the subject", "CMS.Processing", "further processing required", "12AB1234523", "sample.attachment.xlsx, myNotes.pdf", "application/pdf, application/pdf", "15000000,5000000")]
    [InlineData("attachmentFilesizeValid@b.com", "A URN 12ab1234524 in the subject", "CMS.Processing", "further processing required", "12AB1234524", "sample.attachment.xls", "application/pdf", "20000000")]
    [InlineData("AnotherAttachmentFilesizeValid@b.com", "A URN 12ab1234525 in the subject", "CMS.Processing", "further processing required", "12AB1234525", "sample.attachment.xls", "application/pdf", "19000000")]
    public void MailMessageTranslatesToCorrectMailboxFolderNameWhenAttachmentTotalSizeIsChecked(
        string? emailAddress,
        string subject,
        string expectedDestinationFolder,
        string reason,
        string urn,
        string attachmentNames,
        string attachmentContentTypes,
        string attachmentSizes)
    {
        // Arrange
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        string[] attachmentNamesArray = attachmentNames.Split(',');
        string[] attachmentContentTypesArray = attachmentContentTypes.Split(',');
        string[] attachmentSizesArray = attachmentSizes.Split(',');

        Message message = EmbeddedResources.CreateMessageFromEmailTemplateWithAttachmentsJson(
           id: id,
           mailboxId: mailboxId,
           eTag: eTag,
           senderEmailAddress: emailAddress!,
           subject: subject,
           attachmentNames: attachmentNamesArray,
           attachmentContentTypes: attachmentContentTypesArray,
           attachmentSizes: attachmentSizesArray);
        EmailMessageToDestinationFolder translator = this.ServiceProvider.GetRequiredService<EmailMessageToDestinationFolder>();
        EmailMessageInfo emailMessageInfo = new(Email: message, MailboxId: mailboxId);

        // Act
        DestinationFolderResult destinationFolderResult = translator.Translate(emailMessageInfo);

        // Assert
        destinationFolderResult.FolderName.Should().Be(expectedDestinationFolder);
        destinationFolderResult.Reason.Should().Contain(reason);
        destinationFolderResult.Urn.Should().Be(urn);
    }
}
