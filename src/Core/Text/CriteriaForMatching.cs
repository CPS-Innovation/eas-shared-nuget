// <copyright file="CriteriaForMatching.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Text;

/// <summary>
/// Criteria for matching text.
/// </summary>
public enum CriteriaForMatching
{
    /// <summary>
    /// The text must start with the snippet.
    /// </summary>
    StartsWith,

    /// <summary>
    /// The text must contain the snippet.
    /// </summary>
    Contains,

    /// <summary>
    /// The text must end with the snippet.
    /// </summary>
    EndsWith,

    /// <summary>
    /// The text must be an exact match with the snippet.
    /// </summary>
    Exact,

    /// <summary>
    /// The text must end with the snippet will be the last match before going to default mailbox settings.
    /// </summary>
    FinalMatchEndsWith,
}
