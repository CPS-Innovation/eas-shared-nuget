// <copyright file="EmailSubjectToStandardUrnTranslator.cs" company="TheCrownProsecutionService">
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
public partial class EmailSubjectToStandardUrnTranslator
    : UrnTranslator, IEntityTranslator<EmailSubjectInfo, string?>
{
    private static readonly Regex UrnPattern = IdentifyUrnPattern();
    private static readonly Regex WellFormedUrn = WellFormedUrnPattern();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectToStandardUrnTranslator"/> class.
    /// </summary>
    /// <param name="urnSupportLookup">The URN support lookup.</param>
    public EmailSubjectToStandardUrnTranslator(
        IUrnSupportLookup urnSupportLookup)
            : base(urnSupportLookup)
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
                if (IsValidUrn(urn, WellFormedUrn, StandardUrnLength))
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
}
