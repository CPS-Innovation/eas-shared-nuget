// <copyright file="MailboxOptionsS3Test.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Tests.Configuration;

using System;
using System.Data;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

/// <summary>
/// Unit tests for <see cref="MailboxOptions"/> using S3 storage with mocked dependencies.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class MailboxOptionsS3Test
{
    private readonly Mock<IAmazonS3ClientFactory> mockFactory;
    private readonly Mock<IStorageClient> mockStorageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxOptionsS3Test"/> class.
    /// </summary>
    public MailboxOptionsS3Test()
    {
        this.mockStorageClient = new Mock<IStorageClient>();
        this.mockFactory = new Mock<IAmazonS3ClientFactory>();

        this.mockFactory
            .Setup(x => x.GetMailboxConfigurationClient())
            .Returns(this.mockStorageClient.Object);
    }

    /// <summary>
    /// Tests that the constructor requires a non-null factory.
    /// </summary>
    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MailboxOptions(null!));
    }

    /// <summary>
    /// Tests that MailboxOptions loads mailboxes from S3 successfully.
    /// </summary>
    [Fact]
    public void Constructor_WithValidS3Data_LoadsMailboxes()
    {
        // Arrange
        string sampleJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""a1b2c3d4-e5f6-4a5b-8c9d-123456789abc"",
                    ""EmailAddress"": ""test.mailbox1@example.com"",
                    ""Name"": ""Test Mailbox One"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Development"",
                    ""DefaultParty"": ""Defence"",
                    ""DefaultPartyDescription"": ""DEF/TST"",
                    ""CreateCommunicationTask"": true,
                    ""MarkCommunicationAsRead"": false
                }
            ]
        }";

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        // Act
        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Assert
        mailboxOptions.Should().NotBeNull();
        mailboxOptions.MailboxList.Should().NotBeEmpty();
        mailboxOptions.MailboxList.Should().HaveCountGreaterOrEqualTo(1);
    }

    /// <summary>
    /// Tests that MailboxOptions can identify shared mailboxes from S3 data.
    /// </summary>
    [Fact]
    public void IsSharedMailbox_WithS3LoadedData_ReturnsTrue()
    {
        // Arrange
        string sampleJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""b2c3d4e5-f6a7-4b5c-9d0e-234567890def"",
                    ""EmailAddress"": ""test.mailbox2@example.com"",
                    ""Name"": ""Test Mailbox Two"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Development""
                }
            ]
        }";

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Act
        bool isSharedByEmail = mailboxOptions.IsSharedMailbox("test.mailbox2@example.com");
        bool isSharedByGuid = mailboxOptions.IsSharedMailbox("b2c3d4e5-f6a7-4b5c-9d0e-234567890def");

        // Assert
        isSharedByEmail.Should().BeTrue();
        isSharedByGuid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that MailboxOptions handles missing S3 files gracefully.
    /// </summary>
    [Fact]
    public void Constructor_WithMissingS3File_ContinuesWithoutThrowing()
    {
        // Arrange
        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("File not found in S3"));

        // Act - should not throw
        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Assert
        mailboxOptions.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that MailboxOptions loads multiple environment configurations from S3.
    /// </summary>
    [Fact]
    public void Constructor_WithMultipleEnvironments_LoadsAllMailboxes()
    {
        // Arrange
        string devJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""c3d4e5f6-a7b8-4c5d-0e1f-345678901234"",
                    ""EmailAddress"": ""dev.mailbox@example.com"",
                    ""Name"": ""Dev Test Mailbox"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Development""
                }
            ]
        }";

        string stagingJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""d4e5f6a7-b8c9-4d5e-1f2a-456789012345"",
                    ""EmailAddress"": ""staging.mailbox@example.com"",
                    ""Name"": ""Staging Test Mailbox"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Staging""
                }
            ]
        }";

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Development.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(devJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Staging.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stagingJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(
                It.Is<string>(s => s != "appsettings.Development.json" && s != "appsettings.Staging.json"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        // Act
        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Assert
        mailboxOptions.MailboxList.Should().HaveCountGreaterOrEqualTo(2);
        mailboxOptions.MailboxList.Should().Contain(m => m.Environment == "Development");
        mailboxOptions.MailboxList.Should().Contain(m => m.Environment == "Staging");
    }

    /// <summary>
    /// Tests that MailboxOptions retrieves correct mailbox settings from S3 data.
    /// </summary>
    [Theory]
    [InlineData("test.mailbox3@example.com", "Test Mailbox Three", "Defence", "DEF/TST", true, false)]
    [InlineData("e5f6a7b8-c9d0-4e5f-2a3b-567890123456", "Test Mailbox Three", "Defence", "DEF/TST", true, false)]
    public void GetMailboxSettings_WithS3Data_ReturnsCorrectSettings(
        string id,
        string expectedName,
        string expectedParty,
        string expectedPartyDescription,
        bool createCommunicationTask,
        bool markCommunicationAsRead)
    {
        // Arrange
        string sampleJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""e5f6a7b8-c9d0-4e5f-2a3b-567890123456"",
                    ""EmailAddress"": ""test.mailbox3@example.com"",
                    ""Name"": ""Test Mailbox Three"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Development"",
                    ""DefaultParty"": ""Defence"",
                    ""DefaultPartyDescription"": ""DEF/TST"",
                    ""CreateCommunicationTask"": true,
                    ""MarkCommunicationAsRead"": false
                }
            ]
        }";

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Act
        IMailboxSettings mailboxSettings = mailboxOptions.GetMailboxSettings(id);

        // Assert
        mailboxSettings.Should().NotBeNull();
        mailboxSettings.Name.Should().Be(expectedName);
        mailboxSettings.CreateCommunicationTask.Should().Be(createCommunicationTask);
        mailboxSettings.MarkCommunicationAsRead.Should().Be(markCommunicationAsRead);
        mailboxSettings.DefaultParty.Should().Be(expectedParty);
        mailboxSettings.DefaultPartyDescription.Should().Be(expectedPartyDescription);
    }

    /// <summary>
    /// Tests that factory's GetMailboxConfigurationClient is called during construction.
    /// </summary>
    [Fact]
    public void Constructor_CallsGetMailboxConfigurationClient()
    {
        // Arrange
        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        // Act
        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Assert
        this.mockFactory.Verify(x => x.GetMailboxConfigurationClient(), Times.Once);
    }

    /// <summary>
    /// Tests that S3 client GetObjectAsync is called for each environment file.
    /// </summary>
    [Fact]
    public void Constructor_CallsGetObjectAsyncForEachEnvironment()
    {
        // Arrange
        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        // Act
        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Assert
        // Should be called for each environment (Default, Development, Staging, Production.*)
        this.mockStorageClient.Verify(
            x => x.GetObjectAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(23)); // 23 environments in the array
    }

    /// <summary>
    /// Tests Proceeds of Crime mailbox functionality with S3 data.
    /// </summary>
    [Fact]
    public void RequiresProceedsOfCrimeUrnSupport_WithS3Data_ReturnsTrue()
    {
        // Arrange
        string sampleJson = @"{
            ""MailboxList"": [
                {
                    ""MailboxId"": ""f6a7b8c9-d0e1-4f5a-3b4c-678901234567"",
                    ""EmailAddress"": ""poc.test@example.com"",
                    ""Name"": ""POC Test Mailbox"",
                    ""Region"": ""Test Region"",
                    ""Environment"": ""Production"",
                    ""ProceedsOfCrimeUrnSupport"": {
                        ""Supported"": true,
                        ""AcceptOtherAreaUrns"": true
                    }
                }
            ]
        }";

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleJson);

        this.mockStorageClient
            .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"{ ""MailboxList"": [] }");

        var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

        // Act
        bool requiresPOC = mailboxOptions.RequiresProceedsOfCrimeUrnSupport("poc.test@example.com");
        bool acceptsNonPOC = mailboxOptions.AcceptNonProceedsOfCrimeUrn("poc.test@example.com");

        // Assert
        requiresPOC.Should().BeTrue();
        acceptsNonPOC.Should().BeTrue();
    }

/// <summary>
/// Tests that <see cref="MailboxOptions.IsSharedMailbox(string)"/> returns <c>false</c> with S3 data.
/// </summary>
/// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData(" ")]
[InlineData("unknown@example.com")]
[InlineData("00000000-0000-0000-0000-000000000000")]
public void IsSharedMailbox_WithS3Data_ReturnsFalse(string? id)
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""a1b2c3d4-e5f6-4a5b-8c9d-123456789abc"",
                ""EmailAddress"": ""test.mailbox@example.com"",
                ""Name"": ""Test Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Development""
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool isSharedMailbox = mailboxOptions.IsSharedMailbox(id!);

    // Assert
    isSharedMailbox.Should().BeFalse();
}

/// <summary>
/// Tests that a mailbox is not a Proceeds of Crime mailbox with S3 data.
/// </summary>
/// <param name="mailboxId">The mailbox ID to test.</param>
[Theory]
[InlineData("test.regular@example.com")]
[InlineData("a1b2c3d4-e5f6-4a5b-8c9d-123456789abc")]
[InlineData("unknown@example.com")]
public void RequiresProceedsOfCrimeUrnSupport_WithS3Data_ReturnsFalse(string mailboxId)
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""a1b2c3d4-e5f6-4a5b-8c9d-123456789abc"",
                ""EmailAddress"": ""test.regular@example.com"",
                ""Name"": ""Regular Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Development""
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresProceedsOfCrimeUrnSupport(mailboxId);

    // Assert
    result.Should().BeFalse();
}

/// <summary>
/// Tests <c>AcceptNonProceedsOfCrimeUrn</c> returns <c>false</c> with S3 data.
/// </summary>
/// <param name="mailboxId">The mailbox ID to test.</param>
[Theory]
[InlineData("poc.noacceptother@example.com")]
[InlineData("a1b2c3d4-e5f6-4a5b-8c9d-111111111111")]
public void AcceptNonProceedsOfCrimeUrn_WithS3Data_ReturnsFalse(string mailboxId)
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""a1b2c3d4-e5f6-4a5b-8c9d-111111111111"",
                ""EmailAddress"": ""poc.noacceptother@example.com"",
                ""Name"": ""POC Mailbox No Accept"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""ProceedsOfCrimeUrnSupport"": {
                    ""Supported"": true,
                    ""AcceptOtherAreaUrns"": false
                }
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.AcceptNonProceedsOfCrimeUrn(mailboxId);

    // Assert
    result.Should().BeFalse();
}

/// <summary>
/// Tests <c>RequiresEmailAsMsgFile</c> returns <c>false</c> with S3 data.
/// </summary>
[Fact]
public void RequiresEmailAsMsgFile_WithS3Data_ReturnsFalse()
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""a1b2c3d4-e5f6-4a5b-8c9d-123456789abc"",
                ""EmailAddress"": ""test.nomsg@example.com"",
                ""Name"": ""No MSG File Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Development""
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresEmailAsMsgFile("test.nomsg@example.com");

    // Assert
    result.Should().BeFalse();
}

/// <summary>
/// Tests <c>RequiresEmailAsMsgFile</c> returns <c>true</c> with S3 data.
/// </summary>
[Fact]
public void RequiresEmailAsMsgFile_WithS3Data_ReturnsTrue()
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""b2c3d4e5-f6a7-4b5c-9d0e-234567890def"",
                ""EmailAddress"": ""test.withmsg@example.com"",
                ""Name"": ""MSG File Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""EmailAsMsgFileSupport"": {
                    ""Supported"": true,
                    ""IncludeAttachments"": false
                }
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresEmailAsMsgFile("test.withmsg@example.com");

    // Assert
    result.Should().BeTrue();
}

/// <summary>
/// Tests <c>RequiresEmailAsMsgFileIncludeAttachments</c> with S3 data.
/// </summary>
[Theory]
[InlineData("test.withattachments@example.com", true)]
[InlineData("test.noattachments@example.com", false)]
public void RequiresEmailAsMsgFileIncludeAttachments_WithS3Data_ReturnsExpectedValue(string mailboxId, bool expectedResult)
{
    // Arrange
    string sampleJson = $@"{{
        ""MailboxList"": [
            {{
                ""MailboxId"": ""c3d4e5f6-a7b8-4c5d-0e1f-345678901234"",
                ""EmailAddress"": ""test.withattachments@example.com"",
                ""Name"": ""With Attachments"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""EmailAsMsgFileSupport"": {{
                    ""Supported"": true,
                    ""IncludeAttachments"": true
                }}
            }},
            {{
                ""MailboxId"": ""d4e5f6a7-b8c9-4d5e-1f2a-456789012345"",
                ""EmailAddress"": ""test.noattachments@example.com"",
                ""Name"": ""No Attachments"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""EmailAsMsgFileSupport"": {{
                    ""Supported"": true,
                    ""IncludeAttachments"": false
                }}
            }}
        ]
    }}";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresEmailAsMsgFileIncludeAttachments(mailboxId);

    // Assert
    result.Should().Be(expectedResult);
}

/// <summary>
/// Tests <c>RequiresIgnoreAttachments</c> with S3 data.
/// </summary>
[Fact]
public void RequiresIgnoreAttachments_WithS3Data_ReturnsTrue()
{
    // Arrange
    string sampleJson = @"{
        ""MailboxList"": [
            {
                ""MailboxId"": ""e5f6a7b8-c9d0-4e5f-2a3b-567890123456"",
                ""EmailAddress"": ""test.ignoreattach@example.com"",
                ""Name"": ""Ignore Attachments Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""IgnoreAttachments"": true
            }
        ]
    }";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresIgnoreAttachments("test.ignoreattach@example.com");

    // Assert
    result.Should().BeTrue();
}

/// <summary>
/// Tests <c>RequiresSecureMailbox</c> with S3 data.
/// </summary>
[Theory]
[InlineData("test.secure@example.com", true)]
[InlineData("test.notsecure@example.com", false)]
public void RequiresSecureMailbox_WithS3Data_ReturnsExpectedValue(string mailboxId, bool expectedResult)
{
    // Arrange
    string sampleJson = $@"{{
        ""MailboxList"": [
            {{
                ""MailboxId"": ""f6a7b8c9-d0e1-4f5a-3b4c-678901234567"",
                ""EmailAddress"": ""test.secure@example.com"",
                ""Name"": ""Secure Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""IsSecureMailbox"": true
            }},
            {{
                ""MailboxId"": ""a7b8c9d0-e1f2-4a5b-4c5d-789012345678"",
                ""EmailAddress"": ""test.notsecure@example.com"",
                ""Name"": ""Not Secure Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""IsSecureMailbox"": false
            }}
        ]
    }}";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    bool result = mailboxOptions.RequiresSecureMailbox(mailboxId);

    // Assert
    result.Should().Be(expectedResult);
}

/// <summary>
/// Tests TreatZipAttachmentEmailsAsExceptions with S3 data.
/// </summary>
[Theory]
[InlineData("test.treatzip@example.com", true)]
[InlineData("test.notreatzip@example.com", false)]
public void TreatZipAttachmentEmailsAsExceptions_WithS3Data_ReturnsExpectedValue(string mailboxId, bool expectedResult)
{
    // Arrange  
    string sampleJson = $@"{{
        ""MailboxList"": [
            {{
                ""MailboxId"": ""b8c9d0e1-f2a3-4b5c-5d6e-890123456789"",
                ""EmailAddress"": ""test.treatzip@example.com"",
                ""Name"": ""Treat Zip Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""TreatZipAttachmentEmailsAsExceptions"": true
            }},
            {{
                ""MailboxId"": ""c9d0e1f2-a3b4-4c5d-6e7f-901234567890"",
                ""EmailAddress"": ""test.notreatzip@example.com"",
                ""Name"": ""No Treat Zip Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""TreatZipAttachmentEmailsAsExceptions"": false
            }}
        ]
    }}";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    IMailboxSettings settings = mailboxOptions.GetMailboxSettings(mailboxId);

    // Assert
    settings.TreatZipAttachmentEmailsAsExceptions.Should().Be(expectedResult);
}

/// <summary>
/// Tests DisableInvalidUrnAutoReply with S3 data.
/// </summary>
[Theory]
[InlineData("test.disablereply@example.com", true)]
[InlineData("test.nodisablereply@example.com", false)]
public void DisableInvalidUrnAutoReply_WithS3Data_ReturnsExpectedValue(string mailboxId, bool expectedResult)
{
    // Arrange
    string sampleJson = $@"{{
        ""MailboxList"": [
            {{
                ""MailboxId"": ""d0e1f2a3-b4c5-4d5e-7f8a-012345678901"",
                ""EmailAddress"": ""test.disablereply@example.com"",
                ""Name"": ""Disable Reply Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""DisableInvalidUrnAutoReply"": true
            }},
            {{
                ""MailboxId"": ""e1f2a3b4-c5d6-4e5f-8a9b-123456789012"",
                ""EmailAddress"": ""test.nodisablereply@example.com"",
                ""Name"": ""No Disable Reply Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""DisableInvalidUrnAutoReply"": false
            }}
        ]
    }}";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    IMailboxSettings settings = mailboxOptions.GetMailboxSettings(mailboxId);

    // Assert
    settings.DisableInvalidUrnAutoReply.Should().Be(expectedResult);
}

/// <summary>
/// Tests DisableAddToFinalisedCase with S3 data.
/// </summary>
[Theory]
[InlineData("test.disablefinalised@example.com", true)]
[InlineData("test.nodisablefinalised@example.com", false)]
public void DisableAddToFinalisedCase_WithS3Data_ReturnsExpectedValue(string mailboxId, bool expectedResult)
{
    // Arrange
    string sampleJson = $@"{{
        ""MailboxList"": [
            {{
                ""MailboxId"": ""f2a3b4c5-d6e7-4f5a-9b0c-234567890123"",
                ""EmailAddress"": ""test.disablefinalised@example.com"",
                ""Name"": ""Disable Finalised Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""DisableAddToFinalisedCase"": true
            }},
            {{
                ""MailboxId"": ""a3b4c5d6-e7f8-4a5b-0c1d-345678901234"",
                ""EmailAddress"": ""test.nodisablefinalised@example.com"",
                ""Name"": ""No Disable Finalised Mailbox"",
                ""Region"": ""Test Region"",
                ""Environment"": ""Production"",
                ""DisableAddToFinalisedCase"": false
            }}
        ]
    }}";

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>("appsettings.Default.json", It.IsAny<CancellationToken>()))
        .ReturnsAsync(sampleJson);

    this.mockStorageClient
        .Setup(x => x.GetObjectAsync<string>(It.Is<string>(s => s != "appsettings.Default.json"), It.IsAny<CancellationToken>()))
        .ReturnsAsync(@"{ ""MailboxList"": [] }");

    var mailboxOptions = new MailboxOptions(this.mockFactory.Object);

    // Act
    IMailboxSettings settings = mailboxOptions.GetMailboxSettings(mailboxId);

    // Assert
    settings.DisableAddToFinalisedCase.Should().Be(expectedResult);
}
}