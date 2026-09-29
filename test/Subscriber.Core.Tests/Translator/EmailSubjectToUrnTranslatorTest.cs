// <copyright file="EmailSubjectToUrnTranslatorTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Translator;

using System;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Emulation;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Tests <see cref="EmailSubjectToUrnTranslator"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class EmailSubjectToUrnTranslatorTest
{
    private const string DefaultMailboxId = "123";
    private const string ProceedsOfCrimeDummyMailboxId = "08009c6d-e840-4df9-b039-1fa1a24c1ae4";
    private const string ProceedsOfCrimeDummyEmailAddress = "a@test.com";
    private const string ProceedsOfCrimeDummyEmailAddressB = "b@test.com";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectToUrnTranslatorTest"/> class.
    /// </summary>
    public EmailSubjectToUrnTranslatorTest()
    {
        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<IAmazonS3ClientFactory, FakeS3ClientFactory>()
                    .AddMailboxConfiguration()
                    .AddSingleton<EmailSubjectToUrnTranslator>();
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Email subject to URN translation succeeds for well defined URN's.
    /// Examples of well defined URN's are: 12AB0123424, 12A10123424, 12AB0123424(M), 12A10123424(M) , 12AB0123424/1, 12A10123424/2.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    [Theory]
    [InlineData("12AB0123424", "12AB0123424")]
    [InlineData("12A10123424", "12A10123424")]
    [InlineData("12AB0123424(M)", "12AB0123424(M)")]
    [InlineData("12A10123424(M)", "12A10123424(M)")]
    [InlineData("12AB0123424  ", "12AB0123424")]
    [InlineData("Subject with URN 12AB0123424", "12AB0123424")]
    [InlineData("URN 12AB0123424 in the subject", "12AB0123424")]
    [InlineData("URN 12ab0123424 in the subject", "12AB0123424")]
    [InlineData("URN 12ab0123424/1 in the subject", "12AB0123424/1")]
    [InlineData("12AB0123424-T2023/08/30", "12AB0123424")]
    [InlineData("12AB0123424(M)-T2023/08/30", "12AB0123424(M)")]
    [InlineData("12AB0123424,T2023/08/30  ", "12AB0123424")]
    [InlineData("Subject with URN 12AB0123424;T2023/08/30", "12AB0123424")]
    [InlineData("Subject with URN 12AB0123424 2023/08/30", "12AB0123424")]
    [InlineData("Subject with URN 12AB0123424 2023-08-30", "12AB0123424")]
    [InlineData("URN 12AB0123424-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12ab0123424-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12ab0123424/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("(URN 12AB0123424)", "12AB0123424")]
    [InlineData("(URN 12AB0123424(M))", "12AB0123424(M)")]
    public void EmailSubjectToUrnTranslationSucceedsForWellDefinedUrns(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, DefaultMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation bug fixes for subject lines that didn't previously succeed.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("ZXXXXX Mxxxxxx Axxxx Lxxxxx 12AB0123424 Witness Summons_20240123012346.pdf", "12AB0123424", DefaultMailboxId)]
    [InlineData("FW: MAGISTRATES : doh 06.03.24 Cvp application 2400006001 12AB0123424 trial", "12AB0123424", DefaultMailboxId)]
    [InlineData("FW: 12AB0123424   FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980 pncid 95/266814Z  - Deceased", "12AB0123424", DefaultMailboxId)]
    [InlineData("FW: 12AB0123424  FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980  - Deceased", "12AB0123424", DefaultMailboxId)]
    [InlineData("[CJSM] 12AB0123424/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424", DefaultMailboxId)]
    [InlineData("[CJSM] 12AB0123424/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424", DefaultMailboxId)]
    [InlineData("[CJSM] 12AB0123424/1/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424/1", DefaultMailboxId)]
    [InlineData("[CJSM] 12AB0123424/15/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424/15", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424: Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424, Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424; Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424/ Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424. Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424... Slip-Rule Application", "12AB0123424", DefaultMailboxId)]
    public void EmailSubjectToUrnTranslationBugFixesForSubjectLinesThatDidntPreviouslySucceed(
        string emailSubject,
        string expectedUrn,
        string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for delimited URN's.
    /// Examples of delimited URN's are: 12 AB 01234 24, 12/AB/0123424(M), 12 AB 01234 24  , 12/AB/01234/24/1, 12/AB/0123424/1.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    [Theory]
    [InlineData("12 AB 01234 24", "12AB0123424")]
    [InlineData("12/AB/0123424(m)", "12AB0123424(M)")]
    [InlineData("12/AB/0123424(m)/1", "12AB0123424(M)/1")]
    [InlineData("12/AB/0123424(m)/99", "12AB0123424(M)/99")]
    [InlineData("12 AB 01234 24  ", "12AB0123424")]
    [InlineData("Subject with URN 12/AB/01234/24", "12AB0123424")]
    [InlineData("URN 12 AB 01234 24 in the subject", "12AB0123424")]
    [InlineData("URN 12/ab/0123424 in the subject", "12AB0123424")]
    [InlineData("URN 12 ab 01234 24/1 in the subject", "12AB0123424/1")]
    [InlineData("URN 12/ab/0123424/1 in the subject", "12AB0123424/1")]
    [InlineData("URN 12/ab/01234/24/1 in the subject", "12AB0123424/1")]
    [InlineData("12 AB 01234 24-T2023/08/30", "12AB0123424")]
    [InlineData("12/AB/0123424(M)-T2023/08/30", "12AB0123424(M)")]
    [InlineData("12/a1/0123424(M)-T2023/08/30", "12A10123424(M)")]
    [InlineData("12/AB/0123424(M)/1-T2023/08/30", "12AB0123424(M)/1")]
    [InlineData("12/a1/0123424(M)/1-T2023/08/30", "12A10123424(M)/1")]
    [InlineData("12 AB 01234 24-T2023/08/30   ", "12AB0123424")]
    [InlineData("Subject with URN 12/AB/01234/24-T2023/08/30", "12AB0123424")]
    [InlineData("URN 12 AB 01234 24-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12/ab/0123424-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12 ab 01234 24/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12/ab/0123424/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12/ab/01234/24/1*T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12.AB.01234.24-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12,ab,0123424-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12.ab.01234.24/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12,ab,0123424/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12,ab,01234,24/1*T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12;ab;0123424-T2023/08/30 in the subject", "12AB0123424")]
    [InlineData("URN 12;ab;0123424/1-T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("URN 12;ab;01234;24/1*T2023/08/30 in the subject", "12AB0123424/1")]
    [InlineData("(URN 12AB0123424)", "12AB0123424")]
    public void EmailSubjectToUrnTranslationSucceedsForDelimitedUrns(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, DefaultMailboxId);
    }

    /// <summary>
    /// Test for duplicate URNs in the subject line.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    [Theory]
    [InlineData("Merged case 12AB0123424(M) 12AB0123424(M)", "12AB0123424(M)")]
    [InlineData("Split case 12AB0123424/2 12AB0123424/2", "12AB0123424/2")]
    [InlineData("12AB0123424 12AB0123424", "12AB0123424")]
    public void EmailSubjectToUrnTranslationSucceedsForDuplicates(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, DefaultMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails because the URN is too short or non-existent.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12AB012346")]
    [InlineData("A subject line with no URN")]
    public void EmailSubjectToUrnTranslationFailsBecauseTheUrnIsTooShortOrNonExistent(string? emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, DefaultMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails because the URN is too long.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("12AB01234248")]
    [InlineData("12AB0123424A012346")]
    [InlineData("12AB0123424/123")]
    [InlineData("12AB0123424M(12)")]
    public void EmailSubjectToUrnTranslationFailsBecauseTheUrnIsTooLong(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, DefaultMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails because of multiple URN's.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("A subject line with multiple URNs 12AA0123424 12AB0123424")]
    [InlineData("A subject line with multiple URNs 12AA0123424/12AB0123424")]
    [InlineData("A subject line with multiple URNs 12AA0123424/12AB0123424/12A00123424")]
    [InlineData("A subject line with multiple URNs 12AA0123424/1 12AB0123424")]
    [InlineData("A subject line with multiple URNs 12AA0123424/1 12AB0123424/2")]
    [InlineData("A subject line with multiple URNs 12 AA 0123424 12AB0123424 and more")]
    [InlineData("A subject line with multiple URNs 12/AA/01234/24/1 12 AB 0123424")]
    [InlineData("A subject line with multiple URNs 12AA0123424/1 12AB01234/24/2 2008-10-29")]
    public void EmailSubjectToUrnTranslationFailsBecauseOfMultipleUrns(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, DefaultMailboxId);
    }

    private void EmailSubjectToUrnTranslationFails(string? emailSubject, string mailboxId)
    {
        // Assemble
        EmailSubjectToUrnTranslator translator = this.ServiceProvider.GetRequiredService<EmailSubjectToUrnTranslator>();
        EmailSubjectInfo subjectInfo = new(MailboxId: mailboxId, Subject: emailSubject);

        // Act
        string? urn = translator.Translate(subjectInfo);

        // Assert
        urn.Should().BeNull();
    }

    private void EmailSubjectToUrnTranslationSucceeds(string emailSubject, string expectedUrn, string mailboxId)
    {
        // Assemble
        EmailSubjectToUrnTranslator translator = this.ServiceProvider.GetRequiredService<EmailSubjectToUrnTranslator>();
        EmailSubjectInfo subjectInfo = new(MailboxId: mailboxId, Subject: emailSubject);

        // Act
        string? urn = translator.Translate(subjectInfo);

        // Assert
        urn.Should().Be(expectedUrn);
    }
}
