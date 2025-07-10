// <copyright file="ConfigurationProviderInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Configuration;

using System.Collections.Generic;

/// <summary>
/// Used at application startup to provide information for configuration provider setup.
/// </summary>
/// <param name="CommandLineArguments">
/// The command line arguments for the application.
/// </param>
/// <param name="JsonSettingsFiles">
/// A list of JSON settings files.
/// </param>
/// <param name="EnvironmentName">
/// The name of the environment as defined by the environment variables -
/// ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT.
/// </param>
internal sealed record ConfigurationProviderInfo(
    string[] CommandLineArguments,
    IReadOnlyCollection<JsonSettingsFileInfo> JsonSettingsFiles,
    string? EnvironmentName)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationProviderInfo"/> class.
    /// Defaults the list of JSON settings files supported.
    /// </summary>
    /// <param name="commandLineArguments">
    /// The command line arguments for the application.
    /// </param>
    /// <param name="environmentName">
    /// The name of the environment as defined by the environment variables -
    /// ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT.
    /// </param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    public ConfigurationProviderInfo(
        string[] commandLineArguments,
        string? environmentName,
        bool reloadOnChange)
        : this(
            CommandLineArguments: commandLineArguments,
            JsonSettingsFiles: GetDefaultJsonSettingsFiles(environmentName, reloadOnChange),
            EnvironmentName: environmentName)
    {
    }

    /// <summary>
    /// Gets the default JSON settings file collection.
    /// </summary>
    /// <param name="environmentName">The "environment" key as specified in configuration automatically set by the <c>IHostEnvironment</c>.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The default JSON settings file collection.</returns>
    private static IReadOnlyCollection<JsonSettingsFileInfo> GetDefaultJsonSettingsFiles(string? environmentName, bool reloadOnChange)
    {
        List<JsonSettingsFileInfo> result = new()
        {
            new JsonSettingsFileInfo(FileName: "appsettings.json", Optional: false, ReloadOnChange: reloadOnChange),
        };

        if (environmentName is not null)
        {
            result.Add(new JsonSettingsFileInfo(FileName: $"appsettings.{environmentName}.json", Optional: true, ReloadOnChange: reloadOnChange));
        }

        result.Add(new JsonSettingsFileInfo(FileName: "privatesettings.json", Optional: true, ReloadOnChange: reloadOnChange));

        return result;
    }
}
