// <copyright file="UrnTranslator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using System;
using System.Text.RegularExpressions;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Helps translating an email subject to a URN.
/// </summary>
public partial class UrnTranslator
{
    /// <summary>
    /// The token used to identify a merged case.
    /// </summary>
    protected const string MergeToken = "(M)";

    /// <summary>
    /// The max length of a split merge token.
    /// </summary>
    protected const int MaxSplitMergeTokenLength = 6; // "(M)" + "/" + "12"

    /// <summary>
    /// The token regex used to identify a split merged case.
    /// </summary>
    protected static readonly Regex SplitMergeTokenRegex = new Regex($@"{Regex.Escape(MergeToken)}/\d{{1,2}}$");

    /// <summary>
    /// The maximum length of a URN.
    /// </summary>
    protected static readonly int MaxUrnLength = StandardUrnLength + 6 + MaxSplitMergeTokenLength;

    /// <summary>
    /// The length of a standard URN.
    /// </summary>
    protected const int StandardUrnLength = 11;

    /// <summary>
    /// The pattern to identify a name.
    /// </summary>
    protected static readonly Regex NamePattern = IdentifyNamePattern();

    /// <summary>
    /// The URN delimiters.
    /// </summary>
    protected static readonly List<string> UrnDelimiters = new() { " ", ".", ",", ";" };

    /// <summary>
    /// Initializes a new instance of the <see cref="UrnTranslator"/> class.
    /// </summary>
    /// <param name="mailboxOptions">The mailbox options.</param>
    protected UrnTranslator(
        MailboxOptions mailboxOptions)
    {
        Requires.NotNull(mailboxOptions);

        this.MailboxOptions = mailboxOptions;
    }

    /// <summary>
    /// Gets the mailbox options.
    /// </summary>
    protected MailboxOptions MailboxOptions { get; }

    /// <summary>
    /// Determines the URN matches.
    /// </summary>
    /// <param name="subject">The email subject.</param>
    /// <param name="urnPattern">The URN pattern.</param>
    /// <param name="minUrnLength">The minimum URN length.</param>
    /// <returns>Returns the URN matches.</returns>
    protected static SortedSet<string> DetermineUrnMatches(string subject, Regex urnPattern, int minUrnLength)
    {
        Requires.NotNull(subject);
        Requires.NotNull(urnPattern);

        MatchCollection matches = urnPattern.Matches(subject);
        SortedSet<string> result = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < matches.Count; i++)
        {
            string match = matches[i].Value.ToUpperInvariant();
            if (!DetermineUrnContainingSpaceEdgeCase(result, match) &&
                !DetermineUrnDelimitedByForwardSlashEdgeCase(result, match, minUrnLength))
            {
                result.Add(match);
            }
        }

        return result;
    }

    /// <summary>
    /// Identifies potential URN's.
    /// </summary>
    /// <param name="potentialUrn">The potential URN.</param>
    /// <param name="wellFormedUrn">The well-formed URN pattern.</param>
    /// <param name="minUrnLength">The minimum URN length.</param>
    /// <returns>Returns <c>true</c> if a potential URN is identified; otherwise, <c>false</c>.</returns>
    protected static bool IsPotentialUrn(string potentialUrn, Regex wellFormedUrn, int minUrnLength)
    {
        bool valid = true;

        /* Standard URN's are 11 characters long. CMS Modern only accepts 11 character URN's.
         * Proceeds of Crime URN's can have 6 appended alpha-numeric and can be split or merged - max merged is 11 + 6 + 3 + 2 = 22.
         * Proceeds of Crime URN max URN example: 12AB1234524ABCDEF(M)/1
         * Merged cases have (M) appended.
         * Split merged cases have (M)/1 appended.
         * Split cases have /N appended 1 <= N < 100.
         */
        if (potentialUrn.Length > MaxUrnLength || potentialUrn.Length < minUrnLength || !wellFormedUrn.IsMatch(potentialUrn))
        {
            valid = false;
        }

        return valid;
    }

    /// <summary>
    /// Determines if the end text identifies a split case.
    /// </summary>
    /// <param name="endText">The end text.</param>
    /// <returns>Returns <c>true</c> if the end text identifies a split case; otherwise, <c>false</c>.</returns>
    protected static bool IsValidSplitCase(string endText)
    {
        Requires.NotNullOrWhiteSpace(endText);

        bool valid = false;
        if (endText.Length > 1 && endText[0] == '/' && int.TryParse(endText[1..], out int splitCaseNum))
        {
            if (splitCaseNum > 0 && splitCaseNum < 100)
            {
                valid = true;
            }
        }

        return valid;
    }

    /// <summary>
    /// Determines if the URN is valid.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <param name="wellFormedUrn">The well-formed URN pattern.</param>
    /// <param name="minUrnLength">The minimum URN length.</param>
    /// <returns>Returns <c>true</c> if the URN is valid; otherwise, <c>false</c>.</returns>
    protected static bool IsValidUrn(string urn, Regex wellFormedUrn, int minUrnLength)
    {
        bool valid = IsPotentialUrn(urn, wellFormedUrn, minUrnLength);
        if (valid && urn.Length > minUrnLength)
        {
            valid = false;
            string endText = urn[minUrnLength..];
            if (endText.StartsWith("/", StringComparison.Ordinal))
            {
                return IsValidSplitCase(endText);
            }

            if (endText.Equals(MergeToken, StringComparison.Ordinal) ||
                SplitMergeTokenRegex.IsMatch(endText))
            {
                return true;
            }
        }

        return valid;
    }

    /// <summary>
    /// Formats the URN removing delimiters and trailing punctuation.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>Returns the formatted URN.</returns>
    protected static string FormatUrn(string urn)
    {
        Requires.NotNullOrWhiteSpace(urn);

        urn = RemoveUrnDelimiters(urn);
        if (urn.Contains('/', StringComparison.Ordinal))
        {
            urn = RemoveForwardSlashDelimiter(urn);
        }

        urn = RemoveTrailingPunctuation(urn);
        return urn;
    }

    [GeneratedRegex(@"[0-9]{2}[ /.,;]{0,1}[0-9a-zA-Z]{2}[ /.,;]{0,1}[0-9]{5}[ /.,;]{0,1}[0-9]{2}[^\s-,;*]*", RegexOptions.Compiled)]
    protected static partial Regex IdentifyUrnPattern();

    [GeneratedRegex(@"^[0-9]{2}[0-9a-zA-Z]{2}[0-9]{7,}[^\s]*$", RegexOptions.Compiled | RegexOptions.Singleline)]
    protected static partial Regex WellFormedUrnPattern();

    /// <summary>
    /// Removes the URN delimiters.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>Returns the URN without delimiters.</returns>
    private static string RemoveUrnDelimiters(string urn)
    {
        foreach (string invalidChar in UrnDelimiters)
        {
            if (urn.Contains(invalidChar, StringComparison.Ordinal))
            {
                urn = urn.Replace(invalidChar, string.Empty);
            }
        }

        return urn;
    }

    /// <summary>
    /// Removes the forward slash delimiter.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>Returns the URN without the forward slash delimiter.</returns>
    private static string RemoveForwardSlashDelimiter(string urn)
    {
        string[] parts = urn.Split('/');
        urn = string.Empty;
        foreach (string part in parts)
        {
            if (urn.Length < 11)
            {
                urn += part;
            }
            else
            {
                urn += $"/{part}";
            }
        }

        return urn;
    }

    /// <summary>
    /// Removes trailing punctuation from the URN.
    /// </summary>
    /// <param name="urn">The URN.</param>
    /// <returns>Returns the URN without trailing punctuation.</returns>
    private static string RemoveTrailingPunctuation(string urn)
    {
        while (urn.Length >= 2 && urn[^2] != 'M' && urn[^1] == ')')
        {
            /* Remove trailing bracket - need to consider merged cases (M).
             * Have seen subject lines that look like this '(Some URN 12AB1234523)'.
             */
            urn = urn[..^1];
        }

        while (urn[^1] != ')' && char.IsPunctuation(urn, urn.Length - 1))
        {
            // Remove trailing punctuation if its not a closing bracket.
            urn = urn[..^1];
        }

        return urn;
    }

    private static bool DetermineUrnContainingSpaceEdgeCase(SortedSet<string> result, string match)
    {
        bool isEdgeCase = false;
        string[] parts = match.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            // Copes with edge case match == "2400006001 12AB0123424"
            result.Add(parts[0]);
            result.Add(parts[1]);
            isEdgeCase = true;
        }

        return isEdgeCase;
    }

    private static bool DetermineUrnDelimitedByForwardSlashEdgeCase(SortedSet<string> result, string match, int minUrnLength)
    {
        bool isEdgeCase = false;
        string[] parts = match.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            // Copes with edge case subject == "[CJSM] 12AB0123424/Some Person/HRS attached - Next hearing 1/4/24"
            if (parts[0].Length >= minUrnLength && NamePattern.IsMatch(parts[1]))
            {
                result.Add(parts[0]);
                isEdgeCase = true;
            }
        }

        if (parts.Length == 3)
        {
            // Copes with edge case subject == "[CJSM] 12AB0123424/15/Some Person/HRS attached - Next hearing 1/4/24"
            if (parts[0].Length >= minUrnLength &&
                int.TryParse(parts[1], out int number) &&
                number > 0 &&
                number < 100 &&
                NamePattern.IsMatch(parts[2]))
            {
                result.Add($"{parts[0]}/{parts[1]}");
                isEdgeCase = true;
            }
        }

        return isEdgeCase;
    }

    [GeneratedRegex("^[a-zA-Z]{3,}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex IdentifyNamePattern();
}
