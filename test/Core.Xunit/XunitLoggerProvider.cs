// <copyright file="XunitLoggerProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Xunit;

using System;
using System.Collections.Concurrent;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The logging provider for xUnit.
/// </summary>
[ProviderAlias("XunitLogger")]
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly IOptionsMonitor<XunitLoggerOptions> loggerOptions;
    private readonly ITestOutputHelper output;
    private readonly ConcurrentDictionary<string, XunitLogger> loggers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLoggerProvider"/> class.
    /// </summary>
    /// <param name="options">The logger options.</param>
    /// <param name="output">Provides test output.</param>
    public XunitLoggerProvider(
        IOptionsMonitor<XunitLoggerOptions> options,
        ITestOutputHelper output)
    {
        Requires.NotNull(options);
        Requires.NotNull(output);

        this.output = output;
        this.loggerOptions = options;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return this.loggers.GetOrAdd(categoryName, name => new XunitLogger(this.loggerOptions, this.output));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the current instance.
    /// </summary>
    /// <param name="disposing">Indicates whether this was called by the <c>Dispose()</c> method.</param>
    protected virtual void Dispose(bool disposing)
    {
        this.loggers.Clear();
    }
}
