// <copyright file="DocumentDetail.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// Document detail.
/// </summary>
/// <param name="ContentType">The content MIME type.</param>
/// <param name="Name">The name of the document.</param>
/// <param name="Size">The size in bytes of the document.</param>
/// <param name="ReadUrl">The pre-signed GET url used to read the document from storage.</param>
/// <param name="DeleteUrl">The pre-signed DELETE url used to delete the document from storage.</param>
public record DocumentDetail(
    string ContentType,
    string Name,
    int Size,
    string ReadUrl,
    string DeleteUrl);
