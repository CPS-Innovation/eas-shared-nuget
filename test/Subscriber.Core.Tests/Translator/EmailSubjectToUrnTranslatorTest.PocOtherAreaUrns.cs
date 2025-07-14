// <copyright file="EmailSubjectToUrnTranslatorTest.PocOtherAreaUrns.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Translator;

using Cps.Fct.EasShared.Subscriber.Core.Translator;

/// <summary>
/// Tests <see cref="EmailSubjectToUrnTranslator"/>. Standard URN(s) should not be accepted for Proceeds of Crime.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class EmailSubjectToUrnTranslatorTest
{
    /// <summary>
    /// Email subject to URN translation fails for well defined URN(s) that are not Proceeds of Crime URN(s).
    /// Examples of well defined URN(s) are: 12AB0123424, 12A10123424, 12AB0123424(M), 12A10123424(M) , 12AB0123424/1, 12A10123424/2.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("12AB0123424")]
    [InlineData("12A10123424")]
    [InlineData("12AB0123424(M)")]
    [InlineData("12A10123424(M)")]
    [InlineData("12AB0123424  ")]
    [InlineData("Subject with URN 12AB0123424")]
    [InlineData("URN 12AB0123424 in the subject")]
    [InlineData("URN 12ab0123424 in the subject")]
    [InlineData("URN 12ab0123424/1 in the subject")]
    [InlineData("12AB0123424-T2023/08/30")]
    [InlineData("12AB0123424(M)-T2023/08/30")]
    [InlineData("12AB0123424,T2023/08/30  ")]
    [InlineData("Subject with URN 12AB0123424;T2023/08/30")]
    [InlineData("Subject with URN 12AB0123424 2023/08/30")]
    [InlineData("Subject with URN 12AB0123424 2023-08-30")]
    [InlineData("URN 12AB0123424-T2023/08/30 in the subject")]
    [InlineData("URN 12ab0123424-T2023/08/30 in the subject")]
    [InlineData("URN 12ab0123424/1-T2023/08/30 in the subject")]
    [InlineData("(URN 12AB0123424)")]
    [InlineData("(URN 12AB0123424(M))")]
    public void EmailSubjectToUrnTranslationFailsForProceedsOfCrimeForWellDefinedOtherAreaUrns(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for well defined URN(s) that are not Proceeds of Crime URN(s).
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
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeForWellDefinedOtherAreaUrns(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, ProceedsOfCrimeDummyEmailAddressB);
    }

    /// <summary>
    /// Email subject to URN translation bug fixes fails for URN(s) that are not Proceeds of Crime URN(s).
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("FW: MAGISTRATES : doh 06.03.24 Cvp application 2400006001 12AB0123424 trial")]
    [InlineData("FW: 12AB0123424   FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980 pncid 95/266814Z  - Deceased")]
    [InlineData("FW: 12AB0123424  FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980  - Deceased")]
    [InlineData("[CJSM] 12AB0123424/Some Person/HRS attached - Next hearing 1/4/24")]
    [InlineData("[CJSM] 12AB0123424/Longfirstname Person/HRS attached - Next hearing 1/4/24")]
    [InlineData("[CJSM] 12AB0123424/1/Some Person/HRS attached - Next hearing 1/4/24")]
    [InlineData("[CJSM] 12AB0123424/15/Longfirstname Person/HRS attached - Next hearing 1/4/24")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424: Slip-Rule Application")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424, Slip-Rule Application")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424; Slip-Rule Application")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424/ Slip-Rule Application")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424. Slip-Rule Application")]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424... Slip-Rule Application")]
    public void EmailSubjectToUrnTranslationBugFixesFailsForProceedsOfCrimeForOtherAreaUrns(
        string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation fails for delimited URN(s) that are not Proceeds of Crime URN(s).
    /// Examples of delimited URN(s) are: 12 AB 01234 24, 12/AB/0123424(M), 12 AB 01234 24  , 12/AB/01234/24/1, 12/AB/0123424/1.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    [Theory]
    [InlineData("12 AB 01234 24")]
    [InlineData("12/AB/0123424(m)")]
    [InlineData("12/AB/0123424(m)/1")]
    [InlineData("12 AB 01234 24  ")]
    [InlineData("Subject with URN 12/AB/01234/24")]
    [InlineData("URN 12 AB 01234 24 in the subject")]
    [InlineData("URN 12/ab/0123424 in the subject")]
    [InlineData("URN 12 ab 01234 24/1 in the subject")]
    [InlineData("URN 12/ab/0123424/1 in the subject")]
    [InlineData("URN 12/ab/01234/24/1 in the subject")]
    [InlineData("12 AB 01234 24-T2023/08/30")]
    [InlineData("12/AB/0123424(M)-T2023/08/30")]
    [InlineData("12/a1/0123424(M)-T2023/08/30")]
    [InlineData("12/AB/0123424(M)/1-T2023/08/30")]
    [InlineData("12/a1/0123424(M)/1-T2023/08/30")]
    [InlineData("12 AB 01234 24-T2023/08/30   ")]
    [InlineData("Subject with URN 12/AB/01234/24-T2023/08/30")]
    [InlineData("URN 12 AB 01234 24-T2023/08/30 in the subject")]
    [InlineData("URN 12/ab/0123424-T2023/08/30 in the subject")]
    [InlineData("URN 12 ab 01234 24/1-T2023/08/30 in the subject")]
    [InlineData("URN 12/ab/0123424/1-T2023/08/30 in the subject")]
    [InlineData("URN 12/ab/01234/24/1*T2023/08/30 in the subject")]
    [InlineData("URN 12.AB.01234.24-T2023/08/30 in the subject")]
    [InlineData("URN 12,ab,0123424-T2023/08/30 in the subject")]
    [InlineData("URN 12.ab.01234.24/1-T2023/08/30 in the subject")]
    [InlineData("URN 12,ab,0123424/1-T2023/08/30 in the subject")]
    [InlineData("URN 12,ab,01234,24/1*T2023/08/30 in the subject")]
    [InlineData("URN 12;ab;0123424-T2023/08/30 in the subject")]
    [InlineData("URN 12;ab;0123424/1-T2023/08/30 in the subject")]
    [InlineData("URN 12;ab;01234;24/1*T2023/08/30 in the subject")]
    [InlineData("(URN 12AB0123424)")]
    public void EmailSubjectToUrnTranslationFailsForProceedsOfCrimeForDelimitedUrnsForOtherAreas(string emailSubject)
    {
        this.EmailSubjectToUrnTranslationFails(emailSubject, ProceedsOfCrimeDummyMailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for delimited URN(s) that are not Proceeds of Crime URN(s).
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("FW: MAGISTRATES : doh 06.03.24 Cvp application 2400006001 12AB0123424 trial", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("FW: 12AB0123424   FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980 pncid 95/266814Z  - Deceased", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("FW: 12AB0123424  FW: Txxxx Gxxxxx XXXXXXXX X. 27/12/1980  - Deceased", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("[CJSM] 12AB0123424/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("[CJSM] 12AB0123424/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("[CJSM] 12AB0123424/1/Some Person/HRS attached - Next hearing 1/4/24", "12AB0123424/1", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("[CJSM] 12AB0123424/15/Longfirstname Person/HRS attached - Next hearing 1/4/24", "12AB0123424/15", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424: Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424, Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424; Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424/ Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424. Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    [InlineData("RE: T2024 9876 - XXXXXXXXXX Xxxxx 12AB0123424... Slip-Rule Application", "12AB0123424", ProceedsOfCrimeDummyEmailAddressB)]
    public void EmailSubjectToUrnTranslationBugFixesSucceedForProceedsOfCrimeForOtherAreaUrns(
        string emailSubject,
        string expectedUrn,
        string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for well defined URN(s) that are not Proceeds of Crime URN(s).
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
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeForDelimitedUrnsForOtherAreas(string emailSubject, string expectedUrn)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, ProceedsOfCrimeDummyEmailAddressB);
    }
}
