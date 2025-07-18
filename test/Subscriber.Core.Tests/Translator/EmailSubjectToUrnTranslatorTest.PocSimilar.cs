// <copyright file="EmailSubjectToUrnTranslatorTest.PocSimilar.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Translator;

using Cps.Fct.EasShared.Subscriber.Core.Translator;

/// <summary>
/// Tests <see cref="EmailSubjectToUrnTranslator"/>. Similar tests found in EmailSubjectToUrnTranslatorTest.cs.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class EmailSubjectToUrnTranslatorTest
{
    /// <summary>
    /// Email subject to URN translation succeeds for well defined URN's.
    /// Examples of well defined URN's are: 12AB0123424A1, 12A10123424A1, 12AB0123424A1(M), 12A10123424A1(M) , 12AB0123424A1/1, 12A10123424A1/2.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    [Theory]
    [InlineData("12AB0123424A1", "12AB0123424A1")]
    [InlineData("12A10123424A1", "12A10123424A1")]
    [InlineData("12AB0123424A1(M)", "12AB0123424A1(M)")]
    [InlineData("12A10123424A1(M)", "12A10123424A1(M)")]
    [InlineData("12AB0123424A1  ", "12AB0123424A1")]
    [InlineData("Subject with URN 12AB0123424A1", "12AB0123424A1")]
    [InlineData("URN 12AB0123424A1 in the subject", "12AB0123424A1")]
    [InlineData("URN 12ab0123424A1 in the subject", "12AB0123424A1")]
    [InlineData("URN 12ab0123424A1/1 in the subject", "12AB0123424A1/1")]
    [InlineData("12AB0123424A1-T2023/08/30", "12AB0123424A1")]
    [InlineData("12AB0123424A1(M)-T2023/08/30", "12AB0123424A1(M)")]
    [InlineData("12AB0123424A1,T2023/08/30  ", "12AB0123424A1")]
    [InlineData("Subject with URN 12AB0123424A1;T2023/08/30", "12AB0123424A1")]
    [InlineData("Subject with URN 12AB0123424A1 2023/08/30", "12AB0123424A1")]
    [InlineData("Subject with URN 12AB0123424A1 2023-08-30", "12AB0123424A1")]
    [InlineData("URN 12AB0123424A1-T2023/08/30 in the subject", "12AB0123424A1")]
    [InlineData("URN 12ab0123424A1-T2023/08/30 in the subject", "12AB0123424A1")]
    [InlineData("URN 12ab0123424A1/1-T2023/08/30 in the subject", "12AB0123424A1/1")]
    [InlineData("(URN 12AB0123424A1)", "12AB0123424A1")]
    [InlineData("(URN 12AB0123424A1(M))", "12AB0123424A1(M)")]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeForWellDefinedUrns(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, ProceedsOfCrimeDummyEmailAddress);
    }

    /// <summary>
    /// Email subject to URN translation bug fixes for subject lines that didn't previously succeed.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("FW: MAGISTRATES : doh 06.03.24 Cvp application 2400006001 12AB0123424A1 trial", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("FW: 12AB0123424A1   FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980 pncid 95/266814Z  - Deceased", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("FW: 12AB0123424A1  FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980  - Deceased", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("[CJSM] 12AB0123424A1/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("[CJSM] 12AB0123424A1/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("[CJSM] 12AB0123424A1/1/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424A1/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("[CJSM] 12AB0123424A1/15/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424A1/15", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1: Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1, Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1; Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1/ Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1. Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424A1... Slip-Rule Application", "12AB0123424A1", ProceedsOfCrimeDummyMailboxId)]
    public void EmailSubjectToUrnTranslationBugFixesForProceedsOfCrimeForSubjectLinesThatDidntPreviouslySucceed(
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
    [InlineData("12 AB 01234 24A", "12AB0123424A")]
    [InlineData("12/AB/0123424A(m)", "12AB0123424A(M)")]
    [InlineData("12/AB/0123424A(m)/1", "12AB0123424A(M)/1")]
    [InlineData("12/AB/0123424A(m)/99", "12AB0123424A(M)/99")]
    [InlineData("12 AB 01234 24A  ", "12AB0123424A")]
    [InlineData("Subject with URN 12/AB/01234/24A", "12AB0123424A")]
    [InlineData("URN 12 AB 01234 24A in the subject", "12AB0123424A")]
    [InlineData("URN 12/ab/0123424A in the subject", "12AB0123424A")]
    [InlineData("URN 12 ab 01234 24A/1 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12/ab/0123424A/1 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12/ab/01234/24A/1 in the subject", "12AB0123424A/1")]
    [InlineData("12 AB 01234 24A-T2023/08/30", "12AB0123424A")]
    [InlineData("12/AB/0123424A(M)-T2023/08/30", "12AB0123424A(M)")]
    [InlineData("12/a1/0123424A(M)-T2023/08/30", "12A10123424A(M)")]
    [InlineData("12/AB/0123424A(M)/1-T2023/08/30", "12AB0123424A(M)/1")]
    [InlineData("12/a1/0123424A(M)/1-T2023/08/30", "12A10123424A(M)/1")]
    [InlineData("12 AB 01234 24A-T2023/08/30   ", "12AB0123424A")]
    [InlineData("Subject with URN 12/AB/01234/24A-T2023/08/30", "12AB0123424A")]
    [InlineData("URN 12 AB 01234 24A-T2023/08/30 in the subject", "12AB0123424A")]
    [InlineData("URN 12/ab/0123424A-T2023/08/30 in the subject", "12AB0123424A")]
    [InlineData("URN 12 ab 01234 24A/1-T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12/ab/0123424A/1-T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12/ab/01234/24A/1*T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12.AB.01234.24A-T2023/08/30 in the subject", "12AB0123424A")]
    [InlineData("URN 12,ab,0123424A-T2023/08/30 in the subject", "12AB0123424A")]
    [InlineData("URN 12.ab.01234.24A/1-T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12,ab,0123424A/1-T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12,ab,01234,24A/1*T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12;ab;0123424A-T2023/08/30 in the subject", "12AB0123424A")]
    [InlineData("URN 12;ab;0123424A/1-T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("URN 12;ab;01234;24A/1*T2023/08/30 in the subject", "12AB0123424A/1")]
    [InlineData("(URN 12AB0123424A)", "12AB0123424A")]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeForDelimitedUrns(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Test for duplicate URNs in the subject line.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    [Theory]
    [InlineData("Merged case 12AB0123424A(M) 12AB0123424A(M)", "12AB0123424A(M)")]
    [InlineData("Split case 12AB0123424A/2 12AB0123424A/2", "12AB0123424A/2")]
    [InlineData("12AB0123424A 12AB0123424A", "12AB0123424A")]
    [InlineData("12AB0123424A1 12AB0123424A1", "12AB0123424A1")]
    [InlineData("12AB0123424A1B 12AB0123424A1B", "12AB0123424A1B")]
    [InlineData("12AB0123424A1B2 12AB0123424A1B2", "12AB0123424A1B2")]
    [InlineData("12AB0123424A1B2C 12AB0123424A1B2C", "12AB0123424A1B2C")]
    [InlineData("12AB0123424A1B2C3 12AB0123424A1B2C3", "12AB0123424A1B2C3")]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeForDuplicates(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, ProceedsOfCrimeDummyMailboxId);
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
    public void EmailSubjectToUrnTranslationFailsForProceedsOfCrimeBecauseTheUrnIsTooShortOrNonExistent(string? emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails because the URN is too long.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("12AB0123424A012346")]
    [InlineData("12AB0123424A/123")]
    [InlineData("12AB0123424A(M)/123")]
    public void EmailSubjectToUrnTranslationFailsBecauseTheUrnIsTooLongForProceedsOfCrime(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails because of multiple URN's.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("01. A subject line with multiple URNs 12AA0123424A 12AB0123424A")]
    [InlineData("02. A subject line with multiple URNs 12AA0123424A/12AB0123424A")]
    [InlineData("03. A subject line with multiple URNs 12AA0123424A1/12AB0123424A1/12A00123424A1")]
    [InlineData("04. A subject line with multiple URNs 12AA0123424A1B/1 12AB0123424A1B")]
    [InlineData("05. A subject line with multiple URNs 12AA0123424A1B2/1 12AB0123424A1B2/2")]
    [InlineData("06. A subject line with multiple URNs 12 AA 0123424A1B2C 12AB0123424A1B2C and more")]
    [InlineData("07. A subject line with multiple URNs 12/AA/01234/24A1B2C3 12 AB 0123424A1B2C3")]
    [InlineData("08. A subject line with multiple URNs 12AA0123424A1B2C3 12AB01234/24A1B2C3/2 2008-10-29")]
    public void EmailSubjectToUrnTranslationFailsForProceedsOfCrimeBecauseOfMultipleUrns(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }
}
