// <copyright file="IConfigurationProviderManager.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using Microsoft.Extensions.Configuration;

/// <summary>
/// An interface for a configuration provider manager.
/// </summary>
internal interface IConfigurationProviderManager
{
    /// <summary>
    /// Provisions the configuration providers for an application at startup.
    /// </summary>
    /// <param name="configuration">The configuration builder.</param>
    /// <param name="providerInfo">The information for the configuration providers to provision.</param>
    void ProvisionConfigurationProviders(IConfigurationBuilder configuration, ConfigurationProviderInfo providerInfo);
}
