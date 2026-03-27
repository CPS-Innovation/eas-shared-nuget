// <copyright file="MailboxServiceCollectionExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering mailbox configuration services.
/// </summary>
public static class MailboxServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MailboxOptions"/> as a singleton and exposes the narrow
    /// consumer-specific interfaces (<see cref="IUrnSupportLookup"/>,
    /// <see cref="IEmailStorageMailboxLookup"/>, <see cref="IMailboxLookup"/>,
    /// <see cref="IMailboxCatalog"/>) backed by the same instance.
    /// <para>
    /// <see cref="Cps.Fct.EasShared.ServiceClient.Aws.IAmazonS3ClientFactory"/> must
    /// already be registered before calling this method.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMailboxConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<MailboxOptions>();
        services.AddSingleton<IUrnSupportLookup>(sp => sp.GetRequiredService<MailboxOptions>());
        services.AddSingleton<IEmailStorageMailboxLookup>(sp => sp.GetRequiredService<MailboxOptions>());
        services.AddSingleton<IMailboxLookup>(sp => sp.GetRequiredService<MailboxOptions>());
        services.AddSingleton<IMailboxCatalog>(sp => sp.GetRequiredService<MailboxOptions>());

        return services;
    }
}
