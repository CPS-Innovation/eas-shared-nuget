// <copyright file="MailboxOptionsIntegrationTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests.Configuration;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Integration tests for <see cref="MailboxOptions"/> that connect to real S3.
/// </summary>
[Trait("Category", "IntegrationTest")]
[Trait("Status", "Ready")]
public class MailboxOptionsIntegrationTest
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxOptionsIntegrationTest"/> class.
    /// </summary>
    public MailboxOptionsIntegrationTest()
    {
        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
            })
            .ConfigureLogging(builder =>
            {
                builder.AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<AmazonS3ApiOptions>(AmazonS3ApiOptions.DefaultSectionName)
                    .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName)
                    .AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>()
                    .AddSingleton<MailboxOptions>();
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.serviceProvider = serviceScope.ServiceProvider;
    }

    /// <summary>
    /// Tests that S3 access works and configuration can be loaded.
    /// </summary>
    [IntegrationFact]
    public void CanLoadMailboxConfigurationFromS3()
    {
        // Arrange
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act & Assert
        mailboxOptions.Should().NotBeNull();
        mailboxOptions.MailboxList.Should().NotBeNull();
        mailboxOptions.MailboxList.Should().NotBeEmpty("Mailbox configuration should be loaded from S3");
    }

    /// <summary>
    /// Tests that mailbox settings can be retrieved for a known mailbox.
    /// </summary>
    [IntegrationFact]
    public void CanRetrieveMailboxSettings()
    {
        // Arrange
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        IMailboxSettings settings = mailboxOptions.GetMailboxSettings("test@example.com");

        // Assert
        settings.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the mailbox list contains at least one mailbox.
    /// </summary>
    [IntegrationFact]
    public void MailboxListIsNotEmpty()
    {
        // Arrange
        MailboxOptions mailboxOptions = this.serviceProvider.GetRequiredService<MailboxOptions>();

        // Act
        int count = mailboxOptions.MailboxList.Count;

        // Assert
        count.Should().BeGreaterThan(0, "At least one mailbox should be loaded from S3");
    }
}
