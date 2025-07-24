// <copyright file="TextMatchInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Text;

using Microsoft;

/// <summary>
/// Text matching information.
/// </summary>
public class TextMatchInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextMatchInfo"/> class.
    /// </summary>
    public TextMatchInfo()
        : this(
              CriteriaForMatching.StartsWith,
              string.Empty,
              string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextMatchInfo"/> class.
    /// </summary>
    /// <param name="criteriaForMatching">The criteria for matching.</param>
    /// <param name="snippetToMatch">The snippet to match.</param>
    /// <param name="outcome">The outcome.</param>
    public TextMatchInfo(
        CriteriaForMatching criteriaForMatching,
        string snippetToMatch,
        string outcome)
    {
        Requires.NotNull(snippetToMatch);
        Requires.NotNull(outcome);

        this.CriteriaForMatching = criteriaForMatching;
        this.SnippetToMatch = snippetToMatch;
        this.Outcome = outcome;
    }

    /// <summary>
    /// Gets the criteria for matching.
    /// </summary>
    public CriteriaForMatching CriteriaForMatching { get; init; }

    /// <summary>
    /// Gets the snippet to match.
    /// </summary>
    public string SnippetToMatch { get; init; }

    /// <summary>
    /// Gets the outcome.
    /// </summary>
    public string Outcome { get; init; }
}
