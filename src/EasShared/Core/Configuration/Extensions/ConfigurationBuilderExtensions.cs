// <copyright file="ConfigurationBuilderExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Configuration.Extensions;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Extension method(s) for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Provisions the default configuration providers. Primarily used on application startup.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="commandLineArguments">The applications command line arguments.</param>
    /// <param name="environmentName">
    /// The name of the environment as defined by the environment variables -
    /// ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT.
    /// </param>
    /// <param name="reloadJsonSettingsOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>Fluently returns the <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder ProvisionDefaultConfigurationProviders(
        this IConfigurationBuilder configurationBuilder,
        string[] commandLineArguments,
        string environmentName,
        bool reloadJsonSettingsOnChange)
    {
        Requires.NotNull(configurationBuilder);

        ConfigurationProviderManager manager = new();
        manager.ProvisionConfigurationProviders(
            configurationBuilder,
            new ConfigurationProviderInfo(commandLineArguments, environmentName, reloadJsonSettingsOnChange));

        return configurationBuilder;
    }
}
