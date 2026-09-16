// <copyright file="RabbitMQPublisherTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Tests;

using System;
using System.Globalization;
using CloudNative.CloudEvents;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// Tests the RabbitMQ publisher.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class RabbitMQPublisherTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQPublisherTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public RabbitMQPublisherTest(ITestOutputHelper output)
    {
        Requires.NotNull(output);

        string[] args = Array.Empty<string>();
        this.TestHost = Host
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
                    .AddServiceOptions<RabbitMQConnectionOptions>(RabbitMQConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<RetryOptions>(RabbitMQConnectionOptions.DefaultSectionName)
                    .AddServiceOptions<QueuePublisherOptions>($"{QueuePublisherOptions.ParentSectionName}:EmailNotificationReceived");

                services
                    .AddSingleton(output)
                    .AddSingleton<ConnectionFactory>()
                    .AddSingleton<RabbitMQClient>()
                    .AddSingleton<FakeRabbitMQClient>()
                    .AddSingleton(RabbitMQClientTest.CreateRabbitMQClient)
                    .AddSingleton<RabbitMQPublisher>()
                    .AddSingleton<FakeRabbitMQPublisher>()
                    .AddSingleton(CreateRabbitMQPublisher);
            })
            .Build();

        IServiceScope serviceScope = this.TestHost.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    private IHost TestHost { get; }

    /// <summary>
    /// Creating an AMQP channel succeeds.
    /// </summary>
    [Fact]
    public void CreatingAnAmqpChannelSucceeds()
    {
        // Assemble
        QueuePublisherOptions queueOptions = this.ServiceProvider.GetRequiredService<IOptions<QueuePublisherOptions>>().Value;
        IRabbitMQPublisher publisher = this.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();

        // Act
        using IModel channel = publisher.CreateChannel(queueOptions.QueueName);

        // Assert
        channel.Should().NotBeNull();
    }

    /// <summary>
    /// Creating an AMQP channel fails when the publisher is disposed.
    /// </summary>
    [Fact]
    public void CreatingAnAmqpChannelFailsWhenThePublisherIsDisposed()
    {
        // Assemble
        QueuePublisherOptions queueOptions = this.ServiceProvider.GetRequiredService<IOptions<QueuePublisherOptions>>().Value;
        IRabbitMQPublisher publisher = this.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();
        publisher.Dispose();
        IModel? channel = null;

        // Act
        ObjectDisposedException exception = Assert.Throws<ObjectDisposedException>(() => channel = publisher.CreateChannel(queueOptions.QueueName, true));

        // Assert
        exception.Should().NotBeNull();
        channel.Should().BeNull();
    }

    /// <summary>
    /// Publishing an event succeeds.
    /// </summary>
    [Fact]
    public void PublishingAnEventSucceeds()
    {
        // Assemble
        QueuePublisherOptions queueOptions = this.ServiceProvider.GetRequiredService<IOptions<QueuePublisherOptions>>().Value;
        IRabbitMQPublisher publisher = this.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();

        // Act
        using IModel channel = publisher.PublishEvent(queueOptions.QueueName, CreateTestEvent());

        // Assert
        channel.Should().NotBeNull();
    }

    /// <summary>
    /// Creates a test event.
    /// </summary>
    /// <returns>The test event.</returns>
    internal static CloudEvent CreateTestEvent()
    {
        string mailboxId = "b3d00586-8786-4f64-b341-20e515f9c039";
        string source = $"Users/{mailboxId}/Messages";
        string messageId = "AAMkADYyNDU0NWEwLTJlMzYtNGExZC1hZTJmLTMxYzY5ZTBkNDMxZABGAAAAAABdhU36QI-kTLa-1b3sH5CzBwDlGkianql-Rr9Dp21TeC_GAAAAAAEKAADlGkianql-Rr9Dp21TeC_GAAAHlKtcAAA=";

        NewEmailNotificationReceived payload = new(
            CorrespondenceId: Guid.NewGuid(),
            ETag: "W/\"CQAAABYAAADlGkianql/Rr9Dp21TeC+GAAAHk7N4\"",
            EventTime: DateTimeOffset.Parse("2023-03-01T14:11:48.3343433Z", CultureInfo.InvariantCulture),
            MailboxId: mailboxId,
            MessageId: messageId);

        CloudEvent result = new()
        {
            Type = "#Microsoft.Graph.Message",
            Source = new Uri(source, UriKind.Relative),
            Subject = mailboxId,
            Id = $"{messageId} {payload.ETag}",
            Time = payload.EventTime,
            DataContentType = "application/vnd.Cps.Fct.EasShared.NewEmailNotificationReceived\u002Bjson",
            Data = payload,
        };

        result["changetype"] = "Created";
        return result;
    }

    /// <summary>
    /// Creates the RabbitMQ publisher.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The RabbitMQ publisher.</returns>
    private static IRabbitMQPublisher CreateRabbitMQPublisher(IServiceProvider serviceProvider)
    {
        IRabbitMQPublisher serviceClient = serviceProvider.GetRequiredService<RabbitMQPublisher>();
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            serviceClient = serviceProvider.GetRequiredService<FakeRabbitMQPublisher>();
        }

        return serviceClient;
    }
}
