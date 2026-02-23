// <copyright file="EmailStorageRepositoryTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Tests;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Repository.EmailStorage.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws.Emulation;
using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Emulation;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail;
using Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Emulation;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;
using Cps.Fct.EasShared.Storage.Core;
using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Tests for <see cref="EmailStorageRepository"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EmailStorageRepositoryTest
{
    private const string MailboxId = "b3d00586-8786-4f64-b341-20e515f9c039";
    private const string MessageId = "AAMkADYyNDU0NWEwLTJlMzYtNGExZC1hZTJmLTMxYzY5ZTBkNDMxZABGAAAAAABdhU36QI-kTLa-1b3sH5CzBwDlGkianql-Rr9Dp21TeC_GAAAAAAEKAADlGkianql-Rr9Dp21TeC_GAAAHlKtcAAA=";
    private const string MessageETag = "W/\"CQAAABYAAABzLStb6tWjRoczUrTG4kEKAAAMW2DC\"";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailStorageRepositoryTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public EmailStorageRepositoryTest(ITestOutputHelper output)
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
                    .AddServiceOptions<ApplicationOptions>(ApplicationOptions.DefaultSectionName)
                    .AddServiceOptions<TestOptions>(TestOptions.DefaultSectionName)
                    .AddServiceOptions<GraphApiConnectionOptions>(GraphApiConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<AmazonS3ApiOptions>(AmazonS3ApiOptions.DefaultSectionName)
                    .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName)
                    .AddServiceOptions<RabbitMQConnectionOptions>(RabbitMQConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<RetryOptions>(RabbitMQConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<EmailReceivedQueueOptions>(EmailReceivedQueueOptions.DefaultSectionName)
                    .AddServiceOptions<EmailReceivedProceedsOfCrimeQueueOptions>(EmailReceivedProceedsOfCrimeQueueOptions.DefaultSectionName);

                services
                    .AddSingleton(output)
                    .AddSingleton<MailboxOptions>()
                    .AddSingleton<GraphApiClient>()
                    .AddSingleton<FakeGraphApiClient>()
                    .AddSingleton(CreateGraphApiClient)
                    .AddSingleton<GraphApiEmailClient>()
                    .AddSingleton<FakeGraphApiEmailClient>()
                    .AddSingleton(CreateGraphApiEmailClient)
                    .AddSingleton<IAmazonS3ClientFactory, FakeS3ClientFactory>()
                    .AddSingleton<ConnectionFactory>()
                    .AddSingleton<RabbitMQClient>()
                    .AddSingleton<FakeRabbitMQClient>()
                    .AddSingleton(CreateRabbitMQClient)
                    .AddSingleton<RabbitMQPublisher>()
                    .AddSingleton<FakeRabbitMQPublisher>()
                    .AddSingleton(CreateRabbitMQPublisher)
                    .AddSingleton<EmailSubjectToUrnTranslator>()
                    .AddSingleton<IEmailStorageRepository, EmailStorageRepository>();
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Getting an email storage repository via dependency injection succeeds.
    /// </summary>
    [Fact]
    public void GettingAnEmailStorageRepositoryViaDependencyInjectionSucceeds()
    {
        // Act
        IEmailStorageRepository repository = this.ServiceProvider.GetRequiredService<IEmailStorageRepository>();

        // Assert
        repository.Should().NotBeNull();
    }

    /// <summary>
    /// Persisting an email message succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task PersistingAnEmailMessageSucceeds()
    {
        // Arrange
        IEmailStorageRepository repository = this.ServiceProvider.GetRequiredService<IEmailStorageRepository>();
        NewEmailNotificationReceived newEmailNotificationReceived = AddTestMessageToCache(MailboxId, MessageId, MessageETag);

        // Act
        await repository.PersistEmailMessageAsync(newEmailNotificationReceived).ConfigureAwait(true);

        // Assert
        repository.Should().NotBeNull();
    }

    /// <summary>
    /// Persisting an email message fails when the message does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task PersistingAnEmailMessageFails()
    {
        // Arrange
        IEmailStorageRepository repository = this.ServiceProvider.GetRequiredService<IEmailStorageRepository>();
        NewEmailNotificationReceived newEmailNotificationReceived = new(
            CorrespondenceId: Guid.NewGuid(),
            ETag: MessageETag,
            EventTime: DateTimeOffset.UtcNow,
            MailboxId: MailboxId,
            MessageId: Guid.NewGuid().ToString());

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await repository.PersistEmailMessageAsync(newEmailNotificationReceived).ConfigureAwait(true);
        })
            .ConfigureAwait(true);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Unable to retrieve email message");
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
        IGraphApiEmailClient graphApiClient = serviceProvider.GetRequiredService<GraphApiEmailClient>();
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            graphApiClient = serviceProvider.GetRequiredService<FakeGraphApiEmailClient>();
        }

        return graphApiClient;
    }

    //private static IStorageClient CreateStorageClient(IServiceProvider serviceProvider)
    //{
    //    TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
    //    IStorageClient graphApiClient = serviceProvider.GetRequiredService<AmazonS3ApiClient>();
    //    if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
    //    {
    //        graphApiClient = serviceProvider.GetRequiredService<FakeAmazonS3ApiClient>();
    //    }

    //    return graphApiClient;
    //}

    private static IRabbitMQClient CreateRabbitMQClient(IServiceProvider serviceProvider)
    {
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        IRabbitMQClient serviceClient = serviceProvider.GetRequiredService<RabbitMQClient>();
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            serviceClient = serviceProvider.GetRequiredService<FakeRabbitMQClient>();
        }

        return serviceClient;
    }

    private static IRabbitMQPublisher CreateRabbitMQPublisher(IServiceProvider serviceProvider)
    {
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        IRabbitMQPublisher serviceClient = serviceProvider.GetRequiredService<RabbitMQPublisher>();
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            serviceClient = serviceProvider.GetRequiredService<FakeRabbitMQPublisher>();
        }

        return serviceClient;
    }

    private static NewEmailNotificationReceived AddTestMessageToCache(
        string mailboxId,
        string messageId,
        string eTag)
    {
        Message expectedMessage = EmbeddedResources.CreateMessageFromEmailTemplateJson(
            id: messageId,
            mailboxId: mailboxId,
            eTag: eTag,
            hasAttachments: true,
            subject: "URN: 12AB1234523");
        string key = $"Users/{mailboxId}/Messages/{messageId}";
        FakeGraphApiEmailClient.MessageCache.TryAdd(key, expectedMessage);
        FakeGraphApiEmailClient.MimeContentCache.TryAdd(key, EmbeddedResources.MimeEmailContent);

        return new(
            CorrespondenceId: Guid.NewGuid(),
            ETag: eTag,
            EventTime: expectedMessage.ReceivedDateTime!.Value,
            MailboxId: mailboxId,
            MessageId: messageId);
    }
}
