// <copyright file="JsonSettingsFileInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

/// <summary>
/// Information about a JSON settings files.
/// </summary>
/// <param name="FileName">The name of the settings file; for example, appsettings.json.</param>
/// <param name="Optional">Whether the file is optional.</param>
/// <param name="ReloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
internal sealed record JsonSettingsFileInfo(
    string FileName,
    bool Optional,
    bool ReloadOnChange);
