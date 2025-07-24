// <copyright file="GraphApiSubscriptionClientTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Emulation;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

/// <summary>
/// Test connectivity with the Graph API and the management of notification change subscriptions.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiSubscriptionClientTest
{
    private const string SharedMailBox1 = nameof(SharedMailBox1);
    private const string SharedMailBox2 = nameof(SharedMailBox2);

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiSubscriptionClientTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public GraphApiSubscriptionClientTest(ITestOutputHelper output)
    {
        Requires.NotNull(output);

        string[] args = Array.Empty<string>();
        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
            })
            .ConfigureLogging(builder =>
            {
                builder
                    .AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<TestOptions>(TestOptions.DefaultSectionName)
                    .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<GraphApiIntegrationOptions>("GraphApiIntegration:Default")
                    .AddNamedServiceOptions<SubscriptionOptions>("Mailboxes");

                services
                    .AddSingleton(output)
                    .AddSingleton<IGraphApiClient, GraphApiClient>();

                services.AddSingleton(serviceProvider =>
                {
                    ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    IGraphApiClient client = serviceProvider.GetRequiredService<IGraphApiClient>();
                    TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
                    GraphApiSubscriptionClient subscriptionClient = new(
                            graphApiClient: client,
                            loggerFactory: loggerFactory);
                    if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
                    {
                        subscriptionClient = new FakeGraphApiSubscriptionClient(
                            graphApiClient: client,
                            loggerFactory: loggerFactory);
                    }

                    return subscriptionClient;
                });
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; set; }

    private string? SubscriptionId { get; set; }

    private GraphApiSubscriptionClient Client =>
        this.ServiceProvider.GetRequiredService<GraphApiSubscriptionClient>();

    private IOptionsMonitor<SubscriptionOptions> SubscriptionOptions =>
        this.ServiceProvider.GetRequiredService<IOptionsMonitor<SubscriptionOptions>>();

    private IOptions<GraphApiIntegrationOptions> IntegrationOptions =>
        this.ServiceProvider.GetRequiredService<IOptions<GraphApiIntegrationOptions>>();

    /// <summary>
    /// Getting a folder resource succeeds when a folder exists in a mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID.</param>
    /// <param name="folderName">The folder name.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData("61c52668-bc1c-47b9-bfb8-2614a4aa1c11", "Inbox")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Exceptions")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Excluded")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Inbox")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Processed")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Processing")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "CMS.Urgent")]
    public async Task GettingAFolderResourceSucceedsWhenAFolderExistsInAMailbox(
        string mailboxId,
        string folderName)
    {
        // Act
        MailFolderResource resource = await this.Client.GetFolderResourceAsync(
            folderIdentifier: new MailFolderIdentifier(mailboxId, folderName))
                .ConfigureAwait(true);

        // Assert
        resource.Should().NotBeNull();
        resource.MailboxId.Should().Be(mailboxId);
        resource.FolderId.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Getting a folder identity fails when a folder does not exists in a mailbox.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID.</param>
    /// <param name="folderName">The folder name.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData("61c52668-bc1c-47b9-bfb8-2614a4aa1c11", "DoesNotExist")]
    [InlineData("developmentsmb1@zpkw1.onmicrosoft.com", "DoesNotExist")]
    [InlineData("developmentsmb2@zpkw1.onmicrosoft.com", "CMS.DoesNotExist")]
    public async Task GettingAFolderIdentityFailsWhenAFolderDoesNotExistsInAMailbox(
        string mailboxId,
        string folderName)
    {
        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.Client.GetFolderResourceAsync(folderIdentifier: new MailFolderIdentifier(mailboxId, folderName)))
                .ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
        this.Client.LogError(exception);
        exception.Message.Should().Contain(mailboxId);
        exception.Message.Should().Contain(folderName);
        exception.Message.Should().Contain("was not found in the mailbox");
    }
}
