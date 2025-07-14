// <copyright file="ProceedsOfCrimeUrnSupport.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Determines whether Proceeds of Crime URN(s) are supported.
/// </summary>
/// <param name="Supported">Indicates whether Proceeds of Crime URN(s) are supported.</param>
/// <param name="AcceptOtherAreaUrns">Indicates whether other area URN(s) are accepted. Only applicable if <c>Supported == true</c>.</param>
public record ProceedsOfCrimeUrnSupport(
    bool Supported,
    bool? AcceptOtherAreaUrns = null)
{
}
