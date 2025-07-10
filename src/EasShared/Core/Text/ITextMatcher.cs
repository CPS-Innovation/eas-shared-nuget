// <copyright file="ITextMatcher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Text;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// An interface for text matcher.
/// </summary>
public interface ITextMatcher
{
    /// <summary>
    /// Determines whether the text matches the criteria.
    /// </summary>
    /// <param name="criteria">The match criteria.</param>
    /// <param name="textToMatch">The test to match against.</param>
    /// <param name="outcome">The outcome if there is a match.</param>
    /// <returns>Returns <c>true</c> if there is a match; otherwise <c>false</c>.</returns>
    bool IsMatch(IEnumerable<TextMatchInfo> criteria, string textToMatch, [NotNullWhen(true)] out string? outcome);
}
