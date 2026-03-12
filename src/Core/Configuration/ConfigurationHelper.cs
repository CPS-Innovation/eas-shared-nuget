// <copyright file="ConfigurationHelper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using System;

/// <summary>
/// Helpers for configuration value checks.
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// Returns whether a configuration value is considered configured (non-empty and not a placeholder).
    /// Use for keys, secrets, and other values that may be placeholders or empty until properly set.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is non-empty and does not contain "placeholder" (case-insensitive); otherwise, <c>false</c>.</returns>
    public static bool IsValueConfigured(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !value.Contains("placeholder", StringComparison.OrdinalIgnoreCase);
}
