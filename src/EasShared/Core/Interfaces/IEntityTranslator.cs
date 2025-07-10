// <copyright file="IEntityTranslator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Interfaces;

/// <summary>
/// An interface for entity translators.
/// </summary>
/// <typeparam name="TSource">The source entity type to translate from.</typeparam>
/// <typeparam name="TDestination">The destination entity type to translate to.</typeparam>
public interface IEntityTranslator<in TSource, out TDestination>
{
    /// <summary>
    /// Translates <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to translate.</param>
    /// <returns>A translated entity.</returns>
    TDestination Translate(TSource source);
}
