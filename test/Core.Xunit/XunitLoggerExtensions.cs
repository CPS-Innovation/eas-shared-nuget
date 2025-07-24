// <copyright file="XunitLoggerExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Xunit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
/// Extension method for adding a xUnit logger provider.
/// </summary>
public static class XunitLoggerExtensions
{
    /// <summary>
    /// Adds xUnit logging as a logger provider.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <returns>Fluently returns the current logging builder.</returns>
    public static ILoggingBuilder AddXunitLogger(
        this ILoggingBuilder builder)
    {
        builder
            .AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, XunitLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<XunitLoggerOptions, XunitLoggerProvider>(
            builder.Services);

        return builder;
    }
}
