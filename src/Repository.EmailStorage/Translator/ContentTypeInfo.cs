// <copyright file="ContentTypeInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Translator;

/// <summary>
/// Content type information.
/// </summary>
/// <param name="ContentType">Content type of file.</param>
/// <param name="Filename">Name of file.</param>
public record ContentTypeInfo(
    string? ContentType,
    string Filename)
{
}
