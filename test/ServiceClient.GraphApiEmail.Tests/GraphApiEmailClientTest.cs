// <copyright file="GraphApiEmailClientTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System;
using System.IO;
using System.Reflection;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Emulation;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Tests for the Graph API email client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public partial class GraphApiEmailClientTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiEmailClientTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public GraphApiEmailClientTest(ITestOutputHelper output)
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
                builder
                    .AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<TestOptions>(TestOptions.DefaultSectionName)
                    .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<MailboxFolderOptions>(MailboxFolderOptions.DefaultSectionName);

                services
                    .AddSingleton(output)
                    .AddSingleton<GraphApiClient>()
                    .AddSingleton<FakeGraphApiClient>()
                    .AddSingleton(CreateGraphApiClient)
                    .AddSingleton<GraphApiEmailClient>()
                    .AddSingleton<FakeGraphApiEmailClient>()
                    .AddSingleton(CreateGraphApiEmailClient);
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Getting an email message succeeds for a message that exists.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnEmailMessageSucceedsForAMessageThatExists()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Message? actualMessage = await client.GetEmailMessageAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().NotBeNull();
    }

    /// <summary>
    /// Getting an email message succeeds for a message that does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnEmailMessageSucceedsForAMessageThatDoesNotExist()
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<IGraphApiEmailClient>();
        MailMessageIdentifier identifier = new(Guid.Empty.ToString(), "does.not.exist");

        // Act
        Message? actualMessage = await client.GetEmailMessageAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().BeNull();
    }

    /// <summary>
    /// Getting email message information succeeds for a message that exists.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingEmailMessageInfoSucceedsForAMessageThatExists()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        MailMessageInfo? actualMessage = await client.GetEmailMessageInfoAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().NotBeNull();
        actualMessage!.HasAttachments.Should().BeFalse();
    }

    /// <summary>
    /// Getting email message information succeeds for a message that does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingEmailMessageInfoSucceedsForAMessageThatDoesNotExist()
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<IGraphApiEmailClient>();
        MailMessageIdentifier identifier = new(Guid.Empty.ToString(), "does.not.exist");

        // Act
        MailMessageInfo? actualMessage = await client.GetEmailMessageInfoAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().BeNull();
    }

    /// <summary>
    /// Getting the MIME content of an email message succeeds for a message that exists.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingTheMimeContentOfAnEmailMessageSucceedsForAMessageThatExists()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Stream? actualMessage = await client.GetEmailMimeContentAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().NotBeNull();
    }

    /// <summary>
    /// Getting the MIME content of an email message succeeds for a message that does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingTheMimeContentOfAnEmailMessageSucceedsForAMessageThatDoesNotExist()
    {
        // Assemble
        var identifier = MailMessageIdentifier.Parse($"Users/{Guid.Empty}/Messages/does.not.exist");
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Stream? actualMessage = await client.GetEmailMimeContentAsync(identifier).ConfigureAwait(true);

        // Assert
        actualMessage.Should().BeNull();
    }

    /// <summary>
    /// Reply succeeds without throwing an exception.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task ReplySucceedsWithoutThrowingAnException()
    {
        // Assemble
        string resource = AddTestMessageToCache();
        var identifier = MailMessageIdentifier.Parse(resource);
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        Exception? exception = null;

        // Act
        try
        {
            await client.ReplyAsync(identifier, "This is reply message text").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        exception.Should().BeNull();
    }

    /// <summary>
    /// Converting a MIME message to an Outlook message succeeds.
    /// </summary>
    [Fact]
    public void ConvertingFromMimeToOutlookMsgSucceeds()
    {
        // Assemble
        IGraphApiEmailClient client = this.ServiceProvider.GetRequiredService<FakeGraphApiEmailClient>();

        // Act
        Stream msgStreamWitAttachments = client.ConvertMimeToOutlookMsg(EmbeddedResources.MimeAttachmentContent, removeAttachments: false);
        Stream msgStreamNoAttachments = client.ConvertMimeToOutlookMsg(EmbeddedResources.MimeAttachmentContent, removeAttachments: true);

        // Assert
        msgStreamWitAttachments.Should().NotBeNull();
        msgStreamWitAttachments.Position.Should().Be(0);
        msgStreamWitAttachments.Length.Should().BeGreaterThan(0);
        msgStreamNoAttachments.Should().NotBeNull();
        msgStreamNoAttachments.Position.Should().Be(0);
        msgStreamNoAttachments.Length.Should().BeGreaterThan(0);
        msgStreamNoAttachments.Length.Should().BeLessThan(msgStreamWitAttachments.Length);

        // Export streams so that we can view them in outlook
        DirectoryInfo dirInfo = new(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../Output"));
        dirInfo.Exists.Should().BeTrue();

        string filename = Path.Combine(dirInfo.FullName, "msgStreamWitAttachments.msg");
        using FileStream fileStream1 = new(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.None);
        msgStreamWitAttachments.CopyTo(fileStream1);

        filename = Path.Combine(dirInfo.FullName, "msgStreamNoAttachments.msg");
        using FileStream fileStream2 = new(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.None);
        msgStreamNoAttachments.CopyTo(fileStream2);
    }

    private static IGraphApiClient CreateGraphApiClient(IServiceProvider serviceProvider)
    {
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        IGraphApiClient graphApiClient = serviceProvider.GetRequiredService<GraphApiClient>();
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            graphApiClient = serviceProvider.GetRequiredService<FakeGraphApiClient>();
        }

        return graphApiClient;
    }

    private static IGraphApiEmailClient CreateGraphApiEmailClient(IServiceProvider serviceProvider)
    {
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        IGraphApiEmailClient graphApiEmailClient = serviceProvider.GetRequiredService<GraphApiEmailClient>();
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            graphApiEmailClient = serviceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        }

        return graphApiEmailClient;
    }

    private static string AddTestMessageToCache(bool hasAttachments = false)
    {
        string id = "AAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
        if (hasAttachments)
        {
            id = "BAMkAGQ2MzNkMWQ3LTM1OGUtNGU1NS1iODdlLWRkNDU3ZjRlMWIwNgBGAAAAAABe5joI-SlXRIPXzvoBzcGwBwBzLStb6tWjRoczUrTG4kEKAAAAAAEMAABzLStb6tWjRoczUrTG4kEKAAAMXGS6AAA=";
            FakeGraphApiEmailClient.MimeContentCache.TryAdd($"{MimeAttachmentKey}\\{MimeAttachmentId}", EmbeddedResources.MimeAttachmentContent);
        }

        string mailboxId = Guid.NewGuid().ToString();
        string eTag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";
        Message expectedMessage = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: id,
            mailboxId: mailboxId,
            eTag: eTag,
            hasAttachments: hasAttachments);
        string key = $"Users/{mailboxId}/Messages/{id}";
        FakeGraphApiEmailClient.MessageCache.TryAdd(key, expectedMessage);
        FakeGraphApiEmailClient.MimeContentCache.TryAdd(key, EmbeddedResources.MimeEmailContent);
        return key;
    }
}
