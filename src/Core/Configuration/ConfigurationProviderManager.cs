// <copyright file="ConfigurationProviderManager.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using Microsoft;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Used to provision configuration providers for an application.
/// </summary>
internal sealed class ConfigurationProviderManager : IConfigurationProviderManager
{
    /// <inheritdoc/>
    public void ProvisionConfigurationProviders(IConfigurationBuilder configuration, ConfigurationProviderInfo providerInfo)
    {
        Requires.NotNull(configuration);
        Requires.NotNull(providerInfo);

        ClearExistingSources(configuration);
        foreach (JsonSettingsFileInfo fileInfo in providerInfo.JsonSettingsFiles)
        {
            configuration.AddJsonFile(path: fileInfo.FileName, optional: fileInfo.Optional, reloadOnChange: fileInfo.ReloadOnChange);
        }

        configuration
            .AddEnvironmentVariables()
            .AddCommandLine(providerInfo.CommandLineArguments);
    }

    private static void ClearExistingSources(IConfigurationBuilder configuration)
    {
        int count = configuration.Sources.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            IConfigurationSource source = configuration.Sources[i];
            if (source is not ChainedConfigurationSource)
            {
                configuration.Sources.RemoveAt(i);
            }
        }
    }
}
