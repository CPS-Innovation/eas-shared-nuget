// <copyright file="IEntityValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Interfaces;

/// <summary>
/// An interface for entity validators.
/// </summary>
/// <typeparam name="TSource">The source entity type that requires validation.</typeparam>
public interface IEntityValidator<in TSource>
{
    /// <summary>
    /// Validates <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to translate.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(TSource source);
}
