// <copyright file="RabbitMQSubscriberTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Tests;

using System;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Tests the RabbitMQ subscriber.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class RabbitMQSubscriberTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQSubscriberTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public RabbitMQSubscriberTest(ITestOutputHelper output)
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
                    .AddServiceOptions<QueueSubscriberOptions>(QueueSubscriberOptions.DefaultSectionName)
                    .AddServiceOptions<QueuePublisherOptions>($"{QueuePublisherOptions.ParentSectionName}:EmailNotificationReceived");

                services
                    .AddSingleton(output)
                    .AddSingleton<ConnectionFactory>()
                    .AddSingleton<IRabbitMQClient, FakeRabbitMQClient>()
                    .AddSingleton<IRabbitMQPublisher, FakeRabbitMQPublisher>()
                    .AddSingleton<IRabbitMQSubscriber, FakeRabbitMQSubscriber>();
            })
            .Build();

        IServiceScope serviceScope = this.TestHost.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    private IHost TestHost { get; }

    /// <summary>
    /// Start subscribing succeeds.
    /// </summary>
    [Fact]
    public void StartSubscribingSucceeds()
    {
        // Assemble
        IRabbitMQSubscriber subscriber = this.ServiceProvider.GetRequiredService<IRabbitMQSubscriber>();

        // Act
        subscriber.StartSubscribing();

        // Assert
        subscriber.IsSubscribed.Should().BeTrue();
    }

    /// <summary>
    /// Start subscribing fails when the subscriber is disposed.
    /// </summary>
    [Fact]
    public void StartSubscribingFailsWhenTheSubscriberIsDisposed()
    {
        // Assemble
        IRabbitMQSubscriber subscriber = this.ServiceProvider.GetRequiredService<IRabbitMQSubscriber>();
        subscriber.Dispose();

        // Act
        ObjectDisposedException exception = Assert.Throws<ObjectDisposedException>(() => subscriber.StartSubscribing(true));

        // Assert
        exception.Should().NotBeNull();
        subscriber.IsSubscribed.Should().BeFalse();
    }

    /// <summary>
    /// Start subscribing succeeds after publishing events.
    /// </summary>
    [Fact]
    public void StartSubscribingSucceedsAfterPublishingEvents()
    {
        // Assemble
        QueuePublisherOptions queueOptions = this.ServiceProvider.GetRequiredService<IOptions<QueuePublisherOptions>>().Value;
        IRabbitMQSubscriber subscriber = this.ServiceProvider.GetRequiredService<IRabbitMQSubscriber>();
        IRabbitMQPublisher publisher = this.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();
        using IModel channel = publisher.PublishEvent(queueOptions.QueueName, RabbitMQPublisherTest.CreateTestEvent());
        int iterations = 0;

        // Act
        subscriber.StartSubscribing();

        // Assert
        subscriber.IsSubscribed.Should().BeTrue();
        while (++iterations < 5)
        {
            if (FakeRabbitMQPublisher.PublishedCloudEvents.IsEmpty)
            {
                iterations = 5;
            }
            else
            {
                Thread.Sleep(100);
            }
        }

        FakeRabbitMQPublisher.PublishedCloudEvents.IsEmpty.Should().BeTrue();
    }
}
