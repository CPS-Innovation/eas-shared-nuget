// <copyright file="Validator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Interfaces;

using Cps.Fct.EasShared.Core.Interfaces;

/// <summary>
/// A validator for testing.
/// </summary>
internal sealed class Validator : IEntityValidator<object>
{
    /// <inheritdoc/>
    public ValidationResult Validate(object source)
    {
        if (source is null)
        {
            return new ValidationResult(false, "Source is null");
        }

        return new ValidationResult(true, null);
    }
}
