// <copyright file="ConfigurationTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Configuration;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Configuration tests.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ConfigurationTests
{
    /// <summary>
    /// Provisioning the default configuration providers and validates that the correct <see cref="ApplicationOptions"/> are loaded.
    /// </summary>
    /// <param name="environmentName">The name of the environment.</param>
    /// <param name="expectedApplicationName">The expected name of the application.</param>
    [Theory]
    [InlineData("", "core-tests")]
    [InlineData("Development", "core-tests-development")]
    [InlineData("Production", "core-tests")]
    public void ProvisionDefaultConfigurationProvidersAndValidateApplicationOptions(
        string environmentName,
        string expectedApplicationName)
    {
        // Arrange
        using IHost host = ProvisionHost(environmentName);

        // Act
        ApplicationOptions applicationConfig = host.Services.GetRequiredService<IOptions<ApplicationOptions>>().Value;

        // Assert
        applicationConfig.Name.Should().Be(expectedApplicationName);
    }

    /// <summary>
    /// Provisioning the default configuration providers and validates that the correct <see cref="TestOptions"/> are loaded.
    /// </summary>
    /// <param name="environmentName">The name of the environment.</param>
    /// <param name="expectedTestCategory">The expected test category name.</param>
    [Theory]
    [InlineData("", "UnitTest")]
    [InlineData("Development", "IntegrationTest")]
    [InlineData("Production", "UnitTest")]
    public void ProvisionDefaultConfigurationProvidersAndValidateTestOptions(
        string environmentName,
        string expectedTestCategory)
    {
        // Arrange
        using IHost host = ProvisionHost(environmentName);

        // Act
        TestOptions testConfig = host.Services.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        testConfig.Category.Should().Be(expectedTestCategory);
    }

    /// <summary>
    /// Provisioning the default configuration providers and validates that the correct <see cref="FakeOptions"/> are loaded.
    /// </summary>
    /// <param name="sectionName">The configuration section name.</param>
    /// <param name="expectedName">The expected name.</param>
    [Theory]
    [InlineData("FakeSection1", "Section1")]
    [InlineData("FakeSection2", "Section2")]
    public void ProvisionDefaultConfigurationProvidersAndValidateFakeOptions(string sectionName, string expectedName)
    {
        // Arrange
        using IHost host = ProvisionHost("Test");

        // Act
        FakeOptions fakeConfig = host.Services.GetRequiredService<IOptionsMonitor<FakeOptions>>().Get(sectionName);

        // Assert
        fakeConfig.Name.Should().Be(expectedName);
    }

    /// <summary>
    /// Provisioning the default configuration providers and validates that the correct <see cref="RetryOptions"/> are loaded.
    /// </summary>
    /// <param name="environmentName">The name of the environment.</param>
    /// <param name="expectedRetryCount">The expected retry count.</param>
    [Theory]
    [InlineData("", 3)]
    [InlineData("Development", 3)]
    [InlineData("Production", 3)]
    public void ProvisionDefaultConfigurationProvidersAndValidateRetryOptions(
        string environmentName,
        int expectedRetryCount)
    {
        // Arrange
        using IHost host = ProvisionHost(environmentName);

        // Act
        RetryOptions config = host.Services.GetRequiredService<IOptions<RetryOptions>>().Value;

        // Assert
        config.RetryCount.Should().Be(expectedRetryCount);
    }

    private static IHost ProvisionHost(string environmentName)
    {
        string[] args = Array.Empty<string>();

        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, environmentName, false);
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<ApplicationOptions>(ApplicationOptions.DefaultSectionName)
                    .AddServiceOptions<TestOptions>(TestOptions.DefaultSectionName)
                    .AddNamedServiceOptions<FakeOptions>(FakeOptions.ParentSectionName);
            })
            .Build();

        return host;
    }
}
