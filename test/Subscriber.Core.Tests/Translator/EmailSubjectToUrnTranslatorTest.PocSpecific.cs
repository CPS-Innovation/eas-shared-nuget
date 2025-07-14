// <copyright file="EmailSubjectToUrnTranslatorTest.PocSpecific.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Translator;

using Cps.Fct.EasShared.Subscriber.Core.Translator;

/// <summary>
/// Tests <see cref="EmailSubjectToUrnTranslator"/>. These are tests specific to Proceeds of Crime.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class EmailSubjectToUrnTranslatorTest
{
    /// <summary>
    /// Email subject to URN translation succeeds for proceeds of crime URN(s).
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("12AB01234241", "12AB01234241", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424R", "12AB0123424R", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424C", "12AB0123424C", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248", "12AB01234248", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424E to process", "12AB0123424E", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234241E to process", "12AB01234241E", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UR", "12AB0123424UR", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234242UR", "12AB01234242UR", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424UC", "12AB0123424UC", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234243UC", "12AB01234243UC", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UE to process", "12AB0123424UE", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424EII", "12AB0123424EII", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248EII", "12AB01234248EII", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424RII", "12AB0123424RII", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248RII", "12AB01234248RII", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424UCIO to process", "12AB0123424UCIO", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248UCIO to process", "12AB01234248UCIO", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424C10", "12AB0123424C10", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248C10", "12AB01234248C10", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424CII10", "12AB0123424CII10", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234248CII10", "12AB01234248CII10", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424CIO10 to process", "12AB0123424CIO10", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248CIO10 to process", "12AB01234248CIO10", ProceedsOfCrimeDummyEmailAddress)]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrime(string emailSubject, string expectedUrn, string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for proceeds of crime URN(s) that are for split cases.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("12AB01234241/1", "12AB01234241/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424R/10", "12AB0123424R/10", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424C/1", "12AB0123424C/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248/10", "12AB01234248/10", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424E/2 to process", "12AB0123424E/2", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234241E/3 to process", "12AB01234241E/3", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UR/4", "12AB0123424UR/4", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234242UR/5", "12AB01234242UR/5", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424UC/6", "12AB0123424UC/6", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234243UC/7", "12AB01234243UC/7", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UE/8 to process", "12AB0123424UE/8", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424EII/9", "12AB0123424EII/9", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248EII/10", "12AB01234248EII/10", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424RII/11", "12AB0123424RII/11", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248RII/12", "12AB01234248RII/12", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424UCIO/13 to process", "12AB0123424UCIO/13", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248UCIO/14 to process", "12AB01234248UCIO/14", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424C10/15", "12AB0123424C10/15", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248C10/16", "12AB01234248C10/16", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424CII10/17", "12AB0123424CII10/17", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234248CII10/18", "12AB01234248CII10/18", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424CIO10/19 to process", "12AB0123424CIO10/19", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248CIO10/20 to process", "12AB01234248CIO10/20", ProceedsOfCrimeDummyEmailAddress)]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeSplitCases(string emailSubject, string expectedUrn, string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for proceeds of crime URN(s) that are for merged cases.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("12AB01234241(M)", "12AB01234241(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424R(M)", "12AB0123424R(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424C(M)", "12AB0123424C(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248(M)", "12AB01234248(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424E(M) to process", "12AB0123424E(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234241E(M) to process", "12AB01234241E(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UR(M)", "12AB0123424UR(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234242UR(M)", "12AB01234242UR(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424UC(M)", "12AB0123424UC(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234243UC(M)", "12AB01234243UC(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UE(M) to process", "12AB0123424UE(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424EII(M)", "12AB0123424EII(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248EII(M)", "12AB01234248EII(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424RII(M)", "12AB0123424RII(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248RII(M)", "12AB01234248RII(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424UCIO(M) to process", "12AB0123424UCIO(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248UCIO(M) to process", "12AB01234248UCIO(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424C10(M)", "12AB0123424C10(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248C10(M)", "12AB01234248C10(M)", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424CII10(M)", "12AB0123424CII10(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234248CII10(M)", "12AB01234248CII10(M)", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424CIO10(m) to process", "12AB0123424CIO10(M)", ProceedsOfCrimeDummyEmailAddress)]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfCrimeMergedCases(string emailSubject, string expectedUrn, string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }

    /// <summary>
    /// Email subject to URN translation succeeds for proceeds of crime URN(s) that are for split merged cases.
    /// </summary>
    /// <param name="emailSubject">The email subject.</param>
    /// <param name="expectedUrn">The expected URN.</param>
    /// <param name="mailboxId">The mailbox ID.</param>
    [Theory]
    [InlineData("12AB01234241(M)/1", "12AB01234241(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424R(M)/1", "12AB0123424R(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424C(M)/1", "12AB0123424C(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248(M)/1", "12AB01234248(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424E(M)/1 to process", "12AB0123424E(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234241E(M)/1 to process", "12AB01234241E(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UR(M)/1", "12AB0123424UR(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234242UR(M)/1", "12AB01234242UR(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424UC(M)/1", "12AB0123424UC(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234243UC(M)/1", "12AB01234243UC(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424UE(M)/1 to process", "12AB0123424UE(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424EII(M)/1", "12AB0123424EII(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248EII(M)/1", "12AB01234248EII(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB0123424RII(M)/1", "12AB0123424RII(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB01234248RII(M)/1", "12AB01234248RII(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB0123424UCIO(M)/1 to process", "12AB0123424UCIO(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB01234248UCIO(M)/1 to process", "12AB01234248UCIO(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424C10(M)/1", "12AB0123424C10(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("12AB01234248C10(M)/1", "12AB01234248C10(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    [InlineData("A URN 12AB0123424CII10(M)/1", "12AB0123424CII10(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("A URN 12AB01234248CII10(M)/1", "12AB01234248CII10(M)/1", ProceedsOfCrimeDummyMailboxId)]
    [InlineData("12AB0123424CIO10(m)/1 to process", "12AB0123424CIO10(M)/1", ProceedsOfCrimeDummyEmailAddress)]
    public void EmailSubjectToUrnTranslationSucceedsForProceedsOfSplitMergedCases(string emailSubject, string expectedUrn, string mailboxId)
    {
        this.EmailSubjectToUrnTranslationSucceeds(emailSubject, expectedUrn, mailboxId);
    }
}
