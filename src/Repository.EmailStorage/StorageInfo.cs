// <copyright file="StorageInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage;

/// <summary>
/// Storage information.
/// </summary>
/// <param name="ContentType">The content type of the file stored.</param>
/// <param name="DeleteUrl">The URL that can be used to delete an object.</param>
/// <param name="GetUrl">The URL that can be used to get an object.</param>
/// <param name="Name">The name of the file stored.</param>
/// <param name="Size">The size of the file stored in bytes.</param>
internal sealed record StorageInfo(
    string ContentType,
    string DeleteUrl,
    string GetUrl,
    string Name,
    int Size);
