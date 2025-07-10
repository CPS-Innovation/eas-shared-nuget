// <copyright file="ApplicationOptionsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Configuration;

using System;
using System.Text.Json;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Tests for <c>ApplicationOptions</c>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ApplicationOptionsTests
{
    private enum Outcome
    {
        Succeeded,
        Failed,
    }

    /// <summary>
    /// Serialize object using <c>ApplicationSerializerOptions</c>.
    /// </summary>
    [Fact]
    public void SerializeObjectUsingApplicationSerializerOptions()
    {
        // Assemble
        TestObject obj = new();

        // Act
        string json = JsonSerializer.Serialize(obj, ApplicationOptions.ApplicationSerializerOptions);

        // Assert
        json.Should().NotContain(nameof(TestObject.Count));
        json.Should().Contain(nameof(Outcome.Succeeded));
    }

    /// <summary>
    /// Getting the assembly version succeeds.
    /// </summary>
    [Fact]
    public void GettingTheAssemblyFileVersionSucceeds()
    {
        // Act
        string version = ApplicationOptions.GetAssemblyFileVersion(this.GetType());

        // Assert
        version.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Getting the application name using a service provider succeeds.
    /// </summary>
    [Fact]
    public void GettingTheApplicationNameUsingAServiceProviderSucceeds()
    {
        // Assemble
        string[] args = Array.Empty<string>();

        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<ApplicationOptions>(ApplicationOptions.DefaultSectionName);
            })
            .Build();

        // Act
        string name = ApplicationOptions.GetApplicationName(host.Services);

        // Assert
        name.Should().Be("core-tests-development");
    }

    private sealed class TestObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

        public int? Count { get; set; }

        public Outcome Outcome { get; set; }
    }
}
