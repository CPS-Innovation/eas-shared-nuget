// <copyright file="MailboxFolderOptionsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests.Configuration;

using System;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Mailbox folder options test.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class MailboxFolderOptionsTest
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxFolderOptionsTest"/> class.
    /// </summary>
    public MailboxFolderOptionsTest()
    {
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
                    .AddServiceOptions<MailboxFolderOptions>(MailboxFolderOptions.DefaultSectionName);
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Tests the default constructor.
    /// </summary>
    [Fact]
    public void CheckExpectedDefaultFolders()
    {
        // Act
        MailboxFolderOptions options = new();

        // Assert
        options.Should().NotBeNull();
        options.ExceptionsFolderName.Should().Be("CMS.Exceptions");
        options.ExcludedFolderName.Should().Be("CMS.Excluded");
        options.InboxFolderName.Should().Be("CMS.Inbox");
        options.ProcessedFolderName.Should().Be("CMS.Processed");
        options.InvalidUrnFolderName.Should().Be("CMS.InvalidURN");
        options.ProcessingFolderName.Should().Be("CMS.Processing");
        options.UrgentFolderName.Should().Be("CMS.Urgent");

        options.ExcludedFolderExpiry.Should().Be(DefaultExpiry);
        options.ProcessedFolderExpiry.Should().Be(DefaultExpiry);
        options.UrgentFolderExpiry.Should().Be(DefaultExpiry);
    }

    /// <summary>
    /// Tests the configuration from app settings file.
    /// </summary>
    [Fact]
    public void CheckConfiguredFolders()
    {
        // Act
        MailboxFolderOptions options = this.ServiceProvider.GetRequiredService<IOptions<MailboxFolderOptions>>().Value;

        // Assert
        options.Should().NotBeNull();
        options.ExceptionsFolderName.Should().Be("CMS.Exceptions");
        options.ExcludedFolderName.Should().Be("CMS.Excluded");
        options.InboxFolderName.Should().Be("CMS.Inbox");
        options.ProcessedFolderName.Should().Be("CMS.Processed");
        options.InvalidUrnFolderName.Should().Be("CMS.InvalidURN");
        options.ProcessingFolderName.Should().Be("CMS.Processing");
        options.UrgentFolderName.Should().Be("CMS.Urgent");

        options.ExcludedFolderExpiry.Should().Be(DefaultExpiry);
        options.ProcessedFolderExpiry.Should().Be(DefaultExpiry);
        options.UrgentFolderExpiry.Should().Be(DefaultExpiry);
    }

    /// <summary>
    /// Getting all folder names succeeds.
    /// </summary>
    [Fact]
    public void GettingAllFolderNamesSucceeds()
    {
        // Assemble
        MailboxFolderOptions options = new();

        // Act
        IEnumerable<string> folders = options.GetAllFolderNames();

        // Assert
        folders.Should().NotBeNull();
        folders.Should().HaveCount(7);
        folders.Should().Contain("CMS.Exceptions");
        folders.Should().Contain("CMS.Excluded");
        folders.Should().Contain("CMS.Inbox");
        folders.Should().Contain("CMS.Processed");
        folders.Should().Contain("CMS.InvalidURN");
        folders.Should().Contain("CMS.Processing");
        folders.Should().Contain("CMS.Urgent");
    }
}
