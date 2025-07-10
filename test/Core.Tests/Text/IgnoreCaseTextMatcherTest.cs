// <copyright file="IgnoreCaseTextMatcherTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Text;

using System.Collections.Generic;
using Cps.Fct.EasShared.Core.Text;

/// <summary>
/// Tests <see cref="IgnoreCaseTextMatcher"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class IgnoreCaseTextMatcherTest
{
    /// <summary>
    /// Tests <see cref="IgnoreCaseTextMatcher.IsMatch"/> for matches.
    /// </summary>
    /// <param name="criteriaForMatching">The criteria for matching.</param>
    /// <param name="snippetToMatch">The snippet to match.</param>
    /// <param name="outcome">The expected outcome of the match.</param>
    /// <param name="textToMatch">The text to match.</param>
    [Theory]
    [InlineData(CriteriaForMatching.StartsWith, "abc", "Found", "abc_")]
    [InlineData(CriteriaForMatching.Contains, "abc", "Found", "_abc_")]
    [InlineData(CriteriaForMatching.EndsWith, "abc", "Found", "__abc")]
    [InlineData(CriteriaForMatching.Exact, "abc", "Found", "abc")]
    [InlineData(CriteriaForMatching.StartsWith, "xyz", "Found", "XYZ_")]
    [InlineData(CriteriaForMatching.Contains, "xyz", "Found", "_XYZ_")]
    [InlineData(CriteriaForMatching.EndsWith, "xyz", "Found", "__XyZ")]
    [InlineData(CriteriaForMatching.Exact, "xyz", "Found", "XYZ")]
    public void IsMatch(CriteriaForMatching criteriaForMatching, string snippetToMatch, string outcome, string textToMatch)
    {
        // Arrange
        ITextMatcher textMatcher = new IgnoreCaseTextMatcher();
        List<TextMatchInfo> textMatchInfo = new()
        {
            new TextMatchInfo(criteriaForMatching, snippetToMatch, outcome),
        };

        // Act
        bool isMatch = textMatcher.IsMatch(textMatchInfo, textToMatch, out string? actualMatch);

        // Assert
        isMatch.Should().BeTrue();
        outcome.Should().Be(actualMatch);
    }

    /// <summary>
    /// Tests <see cref="IgnoreCaseTextMatcher.IsMatch"/> for non matches.
    /// </summary>
    [Fact]
    public void IsNoMatch()
    {
        // Arrange
        ITextMatcher textMatcher = new IgnoreCaseTextMatcher();
        TextMatchInfo matchInfo = new()
        {
            CriteriaForMatching = CriteriaForMatching.StartsWith,
            SnippetToMatch = "abc",
            Outcome = "N/A",
        };

        List<TextMatchInfo> textMatchInfo = new()
        {
            matchInfo,
        };

        // Act
        bool isMatch = textMatcher.IsMatch(textMatchInfo, "xyz", out _);

        // Assert
        isMatch.Should().BeFalse();
    }
}
