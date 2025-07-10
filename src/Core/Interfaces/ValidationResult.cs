// <copyright file="ValidationResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Interfaces;

/// <summary>
/// The result of a validation.
/// </summary>
/// <param name="Valid">The validation outcome; <c>true</c> for valid or <c>false</c> for invalid.</param>
/// <param name="InvalidReason">The reason why the result was invalid.</param>
public record ValidationResult(
    bool Valid,
    string? InvalidReason);
