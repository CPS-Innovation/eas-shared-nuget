// <copyright file="DiagnosticsUtilityTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Diagnostics;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Diagnostics;
using Cps.Fct.EasShared.Core.Xunit;
using FluentAssertions;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tests for <see cref="DiagnosticsUtility"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class DiagnosticsUtilityTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsUtilityTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public DiagnosticsUtilityTest(ITestOutputHelper output)
    {
        Requires.NotNull(output);

        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITestOutputHelper>(output);

                services.AddServiceOptions<ApplicationOptions>(ApplicationOptions.DefaultSectionName);
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Logging the application information succeeds.
    /// </summary>
    [Fact]
    public void LoggingApplicationInfoSucceeds()
    {
        // Arrange
        ILogger<DiagnosticsUtilityTest> logger = this.ServiceProvider.GetRequiredService<ILogger<DiagnosticsUtilityTest>>();

        // Act
        DiagnosticsUtility.LogApplicationInfo(this.ServiceProvider, logger, "Test", typeof(DiagnosticsUtilityTest));

        // Assert
        logger.Should().NotBeNull();
    }

    /// <summary>
    /// Deriving a log message for an email attachment succeeds.
    /// </summary>
    [Fact]
    public void ToAttachmentStringSucceeds()
    {
        // Arrange
        string name = "test.txt";
        long size = 1234567;
        string contentType = "text/plain";

        // Act
        string result = DiagnosticsUtility.ToAttachmentText(name, size, contentType);

        // Assert
        result.Should().Be($"'{name}' ({size:N0}) Content type - '{contentType}'");
    }

    /// <summary>
    /// Deriving a log message for an email attachment with an OData type succeeds.
    /// </summary>
    [Fact]
    public void ToAttachmentStringWithODataTypeSucceeds()
    {
        // Arrange
        string name = "test.txt";
        long size = 1234567;
        string contentType = "text/plain";
        string oDataType = "#microsoft.graph.fileAttachment";

        // Act
        string result = DiagnosticsUtility.ToAttachmentText(name, size, contentType, oDataType);

        // Assert
        result.Should().Be($"'{name}' ({size:N0}) OData type - '{oDataType}' Content type - '{contentType}'");
    }
}
