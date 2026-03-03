// <copyright file="MailboxOptionsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Tests.Configuration;

using System;
using System.Data;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Unit tests for <see cref="MailboxOptions"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class MailboxOptionsTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxOptionsTest"/> class.
    /// </summary>
    public MailboxOptionsTest()
    {
        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<IAmazonS3ClientFactory, FakeS3ClientFactory>()
                    .AddSingleton<MailboxOptions>();
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MailboxOptions(null!));
    }

    /// <summary>
    /// Tests that the expected number of mailboxes per environment.
    /// </summary>
    /// <param name="environment">The environment to check.</param>
    /// <param name="expectedMailboxCount">The expected number of mailboxes per environment.</param>
    [Theory]
    [InlineData("N/A", 3)]
    [InlineData("Half unit tests", 3)]
    //[InlineData("Development", 1)] // dev integration test
    //[InlineData("Staging", 2)] // staging integration test
    //[InlineData("Production", 190)] // prod integration test
    public void ExpectedEnvironmentCount(string environment, int expectedMailboxCount)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IEnumerable<IdentifiableMailboxSettings> mailboxes = mailboxOptions.MailboxList
            .Where(m => m.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

        // Assert
        mailboxes.Should().HaveCount(expectedMailboxCount);
    }

    /// <summary>
    /// Tests that the expected number of mailboxes are loaded for a given region.
    /// </summary>
    /// <param name="region">The region to check.</param>
    /// <param name="expectedMailboxCount">The expected number of mailboxes.</param>
    [Theory]
    [InlineData("Unit test", 6)]
    //[InlineData("Cymru-Wales", 6)] // prod integration test
    //[InlineData("Default", 1)] // prod integration test
    //[InlineData("East Midlands", 6)] // prod integration test
    //[InlineData("London", 1)] // prod integration test
    //[InlineData("North West", 24)] // prod integration test
    //[InlineData("Proceeds of Crime", 7)] // prod integration test
    //[InlineData("West Midlands", 14)] // prod integration test
    //[InlineData("Wessex", 12)] // prod integration test
    //[InlineData("Extradition", 4)] // prod integration test
    //[InlineData("Yorkshire and Humberside", 12)] // prod integration test
    //[InlineData("Mersey Cheshire", 21)] // prod integration test
    //[InlineData("CPS Direct", 3)] // prod integration test
    //[InlineData("South West", 12)] // prod integration test
    //[InlineData("South East", 10)] // prod integration test
    //[InlineData("East of England", 9)] // prod integration test
    //[InlineData("London North", 2)] // prod integration test
    //[InlineData("London South", 10)] // prod integration test
    //[InlineData("North East", 13)] // prod integration test
    //[InlineData("Thames and Chiltern", 20)] // prod integration test
    //[InlineData("ARU", 3)] // prod integration test
    //[InlineData("VTP", 1)] // prod integration test
    //[InlineData("Testing development", 1)] // dev integration test
    //[InlineData("Testing staging", 2)] // staging integration test
    public void ExpectedRegionCount(string region, int expectedMailboxCount)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IEnumerable<IdentifiableMailboxSettings> mailboxes = mailboxOptions.MailboxList
            .Where(m => m.Region.Equals(region, StringComparison.OrdinalIgnoreCase));

        // Assert
        mailboxes.Should().HaveCount(expectedMailboxCount);
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.IsSharedMailbox(string)"/> returns <c>true</c>.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    [Theory]
    [InlineData("unittest1@mailbox.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039")]
    [InlineData("unittest2@mailbox.com")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4")]
    public void IsASharedMailbox(string id)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool isSharedMailbox = mailboxOptions.IsSharedMailbox(id);

        // Assert
        isSharedMailbox.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.IsSharedMailbox(string)"/> returns <c>false</c>.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("shared1@b.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c030")]
    [InlineData("shared2@b.com")]
    [InlineData("a849ab7c-0000-0000-a546-377a44be0c11")]
    public void NotASharedMailbox(string? id)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool isSharedMailbox = mailboxOptions.IsSharedMailbox(id!);

        // Assert
        isSharedMailbox.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a mailbox is not a Proceeds of Crime mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("a@b.com")]
    [InlineData("123")]
    [InlineData("unittest1@mailbox.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039")]
    public void NotAProceedsOfCrimeMailbox(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresProceedsOfCrimeUrnSupport(mailboxId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a mailbox is a Proceeds of Crime mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("unittest2@mailbox.com")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4")]
    //[InlineData("email.automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13")] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602")] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a")] // prod integration test
    public void IsAProceedsOfCrimeMailbox(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresProceedsOfCrimeUrnSupport(mailboxId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests <c>AcceptNonProceedsOfCrimeUrn</c> returns <c>false</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("notfound@mailbox.com")]
    [InlineData("unittest1@mailbox.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039")]
    //[InlineData("Email.Automation2@cps.gov.uk")] // staging integration test
    //[InlineData("ef0ef3d7-9373-4f61-91a9-37fe5982abef")] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13")] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602")] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a")] // prod integration test
    public void AcceptNonProceedsOfCrimeUrnIsFalse(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.AcceptNonProceedsOfCrimeUrn(mailboxId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests <c>AcceptNonProceedsOfCrimeUrn</c> returns <c>true</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("b@test.com")]
    [InlineData("d9862eb7-4560-4b8c-b462-a9baacb33f38")]
    //[InlineData("Email.Automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    public void AcceptNonProceedsOfCrimeUrnIsTrue(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.AcceptNonProceedsOfCrimeUrn(mailboxId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests <c>RequiresEmailAsMsgFile</c> returns <c>false</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("a@b.com")] // should return false because its default if mailbox not found
    [InlineData("123")] // should return false because its default if mailbox not found
    [InlineData("unittest1@mailbox.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039")]
    public void RequiresEmailAsMsgFileIsFalse(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresEmailAsMsgFile(mailboxId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests <c>RequiresEmailAsMsgFile</c> returns <c>true</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("unittest2@mailbox.com")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4")]
    //[InlineData("email.automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13")] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602")] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a")] // prod integration test
    public void RequiresEmailAsMsgFileIsTrue(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresEmailAsMsgFile(mailboxId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests <c>RequiresEmailAsMsgFileIncludeAttachments</c> returns <c>false</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("a@b.com")]
    [InlineData("123")]
    [InlineData("unittest1@mailbox.com")]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039")]
    [InlineData("unittest2@mailbox.com")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4")]
    public void RequiresEmailAsMsgFileIncludeAttachmentsIsFalse(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresEmailAsMsgFileIncludeAttachments(mailboxId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests <c>RequiresEmailAsMsgFileIncludeAttachments</c> returns <c>true</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("shared4@mailbox.com")]
    [InlineData("9013883a-81e2-494b-8207-bb055bf76b8f")]
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13")] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602")] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a")] // prod integration test
    //[InlineData("email.automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    public void RequiresEmailAsMsgFileIncludeAttachmentsIsTrue(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresEmailAsMsgFileIncludeAttachments(mailboxId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests <c>RequiresIgnoreAttachmentsIsTrue</c> returns <c>true</c>.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("shared3@mailbox.com")]
    [InlineData("2d39b093-548f-444d-84ca-0e4b92f40961")]
    //[InlineData("email.automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    public void RequiresIgnoreAttachmentsIsTrue(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresIgnoreAttachments(mailboxId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.GetMailboxSettings(string)"/> returns the expected settings.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    /// <param name="expectedName">The expected name.</param>
    /// <param name="expectedParty">The expected party.</param>
    /// <param name="expectedPartyDescription">The expected party description.</param>
    /// <param name="createCommunicationTask">The expected value for <see cref="IMailboxSettings.CreateCommunicationTask"/>.</param>
    /// <param name="markCommunicationAsRead">The expected value for <see cref="IMailboxSettings.MarkCommunicationAsRead"/>.</param>
    /// <param name="dummyUrnPrefix">The expected value for <see cref="IMailboxSettings.DummyUrnPrefix"/>.</param>
    [Theory]
    [InlineData(null, "Crown Prosecution Service", "Defence", "DEF/CSL", true, false, null)]
    [InlineData("", "Crown Prosecution Service", "Defence", "DEF/CSL", true, false, null)]
    [InlineData(" ", "Crown Prosecution Service", "Defence", "DEF/CSL", true, false, null)]
    [InlineData("unittest1@mailbox.com", "Crown Prosecution Service", "Defence", "DEF/CSL", true, false, null)]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039", "Crown Prosecution Service", "Defence", "DEF/CSL", true, false, null)]
    [InlineData("unittest2@mailbox.com", "Shared mailbox #2", "Counsel", "CSL", false, true, "99")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4", "Shared mailbox #2", "Counsel", "CSL", false, true, "99")]
    public void GetMailboxSettingsReturnsExpectedSettings(
        string? id,
        string expectedName,
        string expectedParty,
        string expectedPartyDescription,
        bool createCommunicationTask,
        bool markCommunicationAsRead,
        string? dummyUrnPrefix)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IMailboxSettings mailboxSettings = mailboxOptions.GetMailboxSettings(id!);

        // Assert
        mailboxSettings.Should().NotBeNull();
        mailboxSettings.Name.Should().Be(expectedName);
        mailboxSettings.CreateCommunicationTask.Should().Be(createCommunicationTask);
        mailboxSettings.MarkCommunicationAsRead.Should().Be(markCommunicationAsRead);
        mailboxSettings.DefaultParty.Should().Be(expectedParty);
        mailboxSettings.DefaultPartyDescription.Should().Be(expectedPartyDescription);
        mailboxSettings.DummyUrnPrefix.Should().Be(dummyUrnPrefix);
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.GetMailboxSettings(string)"/> returns the expected settings.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    /// <param name="treatZipAttachmentEmailsAsExceptions">The expected value for <see cref="IMailboxSettings.TreatZipAttachmentEmailsAsExceptions"/>.</param>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("unittest1@mailbox.com", false)]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039", false)]
    [InlineData("unittest2@mailbox.com", true)]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4", true)]
    //[InlineData("email.automation3@cps.gov.uk", true)] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4", true)] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13", true)] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602", true)] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a", true)] // prod integration test
    public void GetMailboxSettingsReturnsExpectedTreatZipAttachmentEmailsAsExceptionsSettings(
        string? id,
        bool treatZipAttachmentEmailsAsExceptions)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IMailboxSettings mailboxSettings = mailboxOptions.GetMailboxSettings(id!);

        // Assert
        mailboxSettings.Should().NotBeNull();
        mailboxSettings.TreatZipAttachmentEmailsAsExceptions.Should().Be(treatZipAttachmentEmailsAsExceptions);
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.GetMailboxSettings(string)"/> returns the expected settings.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    /// <param name="disableInvalidUrnAutoReply">The expected value for <see cref="IMailboxSettings.DisableInvalidUrnAutoReply"/>.</param>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("unittest1@mailbox.com", false)]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039", false)]
    [InlineData("unittest2@mailbox.com", true)]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4", true)]
    //[InlineData("email.automation3@cps.gov.uk", false)] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4", false)] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13", false)] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602", false)] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a", false)] // prod integration test
    public void GetMailboxSettingsReturnsExpectedDisableInvalidUrnAutoReplySettings(
        string? id,
        bool disableInvalidUrnAutoReply)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IMailboxSettings mailboxSettings = mailboxOptions.GetMailboxSettings(id!);

        // Assert
        mailboxSettings.Should().NotBeNull();
        mailboxSettings.DisableInvalidUrnAutoReply.Should().Be(disableInvalidUrnAutoReply);
    }

    /// <summary>
    /// Tests that <see cref="MailboxOptions.GetMailboxSettings(string)"/> returns the expected settings.
    /// </summary>
    /// <param name="id">A mailbox identifier, either a GUID or an email address.</param>
    /// <param name="disableAddToFinalisedCase">The expected value for <see cref="IMailboxSettings.DisableAddToFinalisedCase"/>.</param>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("unittest1@mailbox.com", false)]
    [InlineData("b3d00586-8786-4f64-b341-20e515f9c039", false)]
    [InlineData("unittest2@mailbox.com", true)]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4", true)]
    //[InlineData("email.automation3@cps.gov.uk", true)] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4", true)] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13", false)] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602", false)] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a", false)] // prod integration test
    public void GetMailboxSettingsReturnsExpectedDisableAddToFinalisedCase(
        string? id,
        bool disableAddToFinalisedCase)
    {
        // Arrange
        MailboxOptions mailboxOptions = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IMailboxSettings mailboxSettings = mailboxOptions.GetMailboxSettings(id!);

        // Assert
        mailboxSettings.Should().NotBeNull();
        mailboxSettings.DisableAddToFinalisedCase.Should().Be(disableAddToFinalisedCase);
    }

    /// <summary>
    /// Tests that a mailbox requires the secure mailbox queue.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to test.</param>
    [Theory]
    [InlineData("unittest2@mailbox.com")]
    [InlineData("d302b81d-e137-4478-a306-d400216707d4")]
    //[InlineData("email.automation3@cps.gov.uk")] // staging integration test
    //[InlineData("2e256642-b947-4da3-a987-0233f165eff4")] // staging integration test
    //[InlineData("0724d778-579d-423a-a83a-a1f092257b13")] // prod integration test
    //[InlineData("6868c72f-8c7d-434e-b9c8-6cd76778e602")] // prod integration test
    //[InlineData("8c5e56db-11b3-4a75-aedb-70ec5d18750a")] // prod integration test
    //[InlineData("253ebcbf-c354-46c5-91e0-791581ac276e")] // prod integration test
    //[InlineData("da0e9987-5098-482a-9c5f-bdcf5d8e930e")] // prod integration test
    //[InlineData("e281f282-6773-4a85-a215-6bc40fc026a9")] // prod integration test
    public void IsASecureMailbox(string mailboxId)
    {
        // Arrange
        MailboxOptions options = this.ServiceProvider.GetRequiredService<MailboxOptions>();

        // Act
        bool result = options.RequiresSecureMailbox(mailboxId);

        // Assert
        result.Should().BeTrue();
    }
}
