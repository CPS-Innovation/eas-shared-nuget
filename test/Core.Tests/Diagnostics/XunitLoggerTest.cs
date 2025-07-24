// <copyright file="XunitLoggerTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Diagnostics;

using System;
using Cps.Fct.EasShared.Core.Xunit;
using FluentAssertions;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// XunitLogger test.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class XunitLoggerTest : IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLoggerTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public XunitLoggerTest(ITestOutputHelper output)
    {
        Requires.NotNull(output);

        string[] args = Array.Empty<string>();
        this.TestHost = Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITestOutputHelper>(output);
            })
            .Build();

        IServiceScope serviceScope = this.TestHost.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; set; }

    private IHost TestHost { get; set; }

    /// <summary>
    /// Test that a logger can be instantiated and disposed.
    /// </summary>
    [Fact]
    public void LoggerInstantiateAndDispose()
    {
        // Act
        ILogger<XunitLoggerTest> logger = this.ServiceProvider.GetRequiredService<ILogger<XunitLoggerTest>>();
        IDisposable? scope = logger.BeginScope(nameof(this.LoggerInstantiateAndDispose));

        // Assert
        logger.Should().NotBeNull();
        logger.LogInformation("A sample log event\n - Time: {Time}", TimeUtility.GetUKLocalTime(DateTime.UtcNow));
        Assumes.NotNull(scope);
        scope.Dispose();
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
            if (disposing)
            {
                this.TestHost.Dispose();
            }
        }
    }
}
