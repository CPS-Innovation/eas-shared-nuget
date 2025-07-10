// <copyright file="IgnoreCaseTextMatcher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Text;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft;

/// <summary>
/// A text matcher that ignores case.
/// </summary>
public class IgnoreCaseTextMatcher : ITextMatcher
{
    /// <inheritdoc/>
    public bool IsMatch(
        IEnumerable<TextMatchInfo> criteria,
        string textToMatch,
        [NotNullWhen(true)] out string? outcome)
    {
        Requires.NotNullOrEmpty(criteria);
        Requires.NotNull(textToMatch);

        // Exact matches must come before other matches
        foreach (TextMatchInfo info in criteria.Where(c => c.CriteriaForMatching == CriteriaForMatching.Exact))
        {
            bool found = info.CriteriaForMatching == CriteriaForMatching.Exact && textToMatch.Equals(info.SnippetToMatch, StringComparison.OrdinalIgnoreCase);

            if (found)
            {
                outcome = info.Outcome;
                return true;
            }
        }

        foreach (TextMatchInfo info in criteria.Where(c => c.CriteriaForMatching != CriteriaForMatching.Exact))
        {
            bool found = info.CriteriaForMatching == CriteriaForMatching.EndsWith && textToMatch.EndsWith(info.SnippetToMatch, StringComparison.OrdinalIgnoreCase);

            if (!found)
            {
                found = info.CriteriaForMatching == CriteriaForMatching.StartsWith && textToMatch.StartsWith(info.SnippetToMatch, StringComparison.OrdinalIgnoreCase);
            }

            if (!found)
            {
                found = info.CriteriaForMatching == CriteriaForMatching.Contains && textToMatch.Contains(info.SnippetToMatch, StringComparison.OrdinalIgnoreCase);
            }

            if (found)
            {
                outcome = info.Outcome;
                return true;
            }
        }

        outcome = null;
        return false;
    }
}
