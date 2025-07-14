// <copyright file="EmailSubjectToPocUrnTranslator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using System;
using System.Text.RegularExpressions;
using Cps.Fct.EasShared.Core.Interfaces;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Translates an email subject to a URN.
/// </summary>
public partial class EmailSubjectToPocUrnTranslator
    : UrnTranslator, IEntityTranslator<EmailSubjectInfo, string?>
{
    private static readonly Regex UrnPattern = IdentifyUrnPattern();
    private static readonly Regex WellFormedUrn = WellFormedUrnPattern();
    private static readonly Regex ProceedsOfCrimeUrn = ProceedsOfCrimeUrnPattern();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectToPocUrnTranslator"/> class.
    /// </summary>
    /// <param name="mailboxOptions">The mailbox options.</param>
    public EmailSubjectToPocUrnTranslator(
        MailboxOptions mailboxOptions)
            : base(mailboxOptions)
    {
    }

    /// <inheritdoc/>
    public string? Translate(EmailSubjectInfo source)
    {
        Requires.NotNull(source);
        Requires.NotNullOrWhiteSpace(source.MailboxId);

        string? result = null;
        if (source.Subject is not null)
        {
            SortedSet<string> urns = new(StringComparer.OrdinalIgnoreCase);

            IEnumerable<string> matches = DetermineUrnMatches(source.Subject, UrnPattern, StandardUrnLength);
            foreach (string match in matches)
            {
                string urn = FormatUrn(match);
                if (IsProceedsOfCrimeUrn(urn, this.MailboxOptions.AcceptNonProceedsOfCrimeUrn(source.MailboxId)))
                {
                    if (!urns.Contains(urn))
                    {
                        urns.Add(urn);
                    }
                }
            }

            if (urns.Count == 1)
            {
                result = urns.First();
            }
        }

        return result;
    }

    private static bool IsProceedsOfCrimeUrn(string urn, bool acceptNonProceedsOfCrimeUrn)
    {
        bool valid = urn.Length > StandardUrnLength && IsPotentialUrn(urn, WellFormedUrn, StandardUrnLength);
        if (valid)
        {
            string end = urn[StandardUrnLength..];
            if (end.EndsWith(MergeToken, StringComparison.Ordinal) || end.EndsWith(SplitMergeToken, StringComparison.Ordinal))
            {
                valid = IsValidPocMergedCase(end);
            }
            else if (end.Contains('/', StringComparison.Ordinal))
            {
                valid = IsValidPocSplitCase(end);
            }
            else
            {
                valid = ProceedsOfCrimeUrn.IsMatch(end);
            }
        }

        if (!valid && acceptNonProceedsOfCrimeUrn)
        {
            valid = IsValidUrn(urn, WellFormedUrn, StandardUrnLength);
        }

        return valid;
    }

    private static bool IsValidPocSplitCase(string end)
    {
        bool valid;
        string[] parts = end.Split('/');
        valid = parts.Length == 1 || parts.Length == 2;
        if (valid)
        {
            valid = ProceedsOfCrimeUrn.IsMatch(parts[0]);
        }

        if (valid && parts.Length == 2)
        {
            valid = IsValidSplitCase($"/{parts[1]}");
        }

        return valid;
    }

    private static bool IsValidPocMergedCase(string end)
    {
        bool valid = false;
        if (end.EndsWith(MergeToken, StringComparison.Ordinal))
        {
            valid = ProceedsOfCrimeUrn.IsMatch(end[..^MergeToken.Length]);
        }

        if (end.EndsWith(SplitMergeToken, StringComparison.Ordinal))
        {
            valid = ProceedsOfCrimeUrn.IsMatch(end[..^SplitMergeToken.Length]);
        }

        return valid;
    }

    [GeneratedRegex("^[0-9a-zA-Z]{1,6}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ProceedsOfCrimeUrnPattern();
}
