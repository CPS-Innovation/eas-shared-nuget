// <copyright file="XunitLogger.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Xunit;

using System;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Surfaces <see cref="ILogger"/> output in an xUnit test.
/// </summary>
public class XunitLogger : ILogger, IDisposable
{
    private readonly XunitLoggerOptions loggerOptions;
    private readonly ITestOutputHelper output;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLogger"/> class.
    /// </summary>
    /// <param name="options">The logger options.</param>
    /// <param name="output">Provides test output.</param>
    public XunitLogger(
        IOptionsMonitor<XunitLoggerOptions> options,
        ITestOutputHelper output)
    {
        Requires.NotNull(options);
        Requires.NotNull(output);

        this.output = output;
        this.loggerOptions = options.Get(XunitLoggerOptions.DefaultSectionName);
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= this.loggerOptions.Default;
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (this.IsEnabled(logLevel) && state is not null)
        {
            try
            {
                this.output.WriteLine(state.ToString());
            }
            catch
            {
                // Suppress exception - The active test run was aborted. Reason: Test host process crashed : Unhandled exception. Unhandled exception. System.AggregateException: An error occurred while writing to logger(s). (There is no currently active test.)
            }
        }
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
        if (!this.disposedValue)
        {
            this.disposedValue = true;
        }
    }
}
