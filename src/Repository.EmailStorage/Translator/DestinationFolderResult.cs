// <copyright file="DestinationFolderResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Translator;

/// <summary>
/// The destination folder result.
/// </summary>
/// <param name="FolderName">The name of the folder to move the email to.</param>
/// <param name="Reason">The reason to move it there.</param>
/// <param name="Urn">The unique reference number of the case related to the email. </param>
public record DestinationFolderResult(
    string FolderName,
    string Reason,
    string Urn)
{
}
