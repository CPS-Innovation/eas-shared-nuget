// <copyright file="RabbitMQClientTest.cs" company="TheCrownProsecutionService">
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
/// Tests the RabbitMQ client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class RabbitMQClientTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQClientTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public RabbitMQClientTest(ITestOutputHelper output)
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
                    .AddServiceOptions<RetryOptions>(RabbitMQConnectionOptions.DefaultSectionName);

                services
                    .AddSingleton(output)
                    .AddSingleton<ConnectionFactory>()
                    .AddSingleton<RabbitMQClient>()
                    .AddSingleton<FakeRabbitMQClient>()
                    .AddSingleton(CreateRabbitMQClient);
            })
            .Build();

        IServiceScope serviceScope = this.TestHost.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    private IHost TestHost { get; }

    /// <summary>
    /// Connecting to the Rabbit MQ server succeeds.
    /// </summary>
    [Fact]
    public void ConnectingToServerSucceeds()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();

        // Act
        client.Connect();

        // Assert
        client.IsConnectionOpen.Should().BeTrue();
    }

    /// <summary>
    /// Connecting to the Rabbit MQ server fails when the client is disposed.
    /// </summary>
    [Fact]
    public void ConnectingToServerFailsWhenTheClientIsDisposed()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();
        client.Connect();
        client.Dispose();

        // Act
        ObjectDisposedException exception = Assert.Throws<ObjectDisposedException>(() => client.Connect());

        // Assert
        exception.Should().NotBeNull();
        client.IsConnectionOpen.Should().BeFalse();
    }

    /// <summary>
    /// An AMQP model can be created from the client.
    /// </summary>
    [Fact]
    public void AnAmqpModelCanBeCreatedFromTheClient()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();

        // Act
        IModel model = client.CreateModel();

        // Assert
        model.Should().NotBeNull();
        client.IsConnectionOpen.Should().BeTrue();
    }

    /// <summary>
    /// The event handler <c>OnConnectionBlocked</c> creates a new connection when raised.
    /// </summary>
    [Fact]
    public void EventHandlerOnConnectionBlockedCreatesANewConnectionWhenRaised()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();
        ConnectionBlockedEventArgs eventArgs = new("For test purposes");

        // Act
        client.OnConnectionBlocked(null, eventArgs);

        // Assert
        client.IsConnectionOpen.Should().BeTrue();
    }

    /// <summary>
    /// The event handler <c>OnCallbackException</c> creates a new connection when raised.
    /// </summary>
    [Fact]
    public void EventHandlerOnCallbackExceptionCreatesANewConnectionWhenRaised()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();
        CallbackExceptionEventArgs eventArgs = new(new Exception("Bad stuff happened"));

        // Act
        client.OnCallbackException(null, eventArgs);

        // Assert
        client.IsConnectionOpen.Should().BeTrue();
    }

    /// <summary>
    /// The event handler <c>OnConnectionShutdown</c> creates a new connection when raised.
    /// </summary>
    [Fact]
    public void EventHandlerOnConnectionShutdownCreatesANewConnectionWhenRaised()
    {
        // Assemble
        IRabbitMQClient client = this.ServiceProvider.GetRequiredService<IRabbitMQClient>();
        ShutdownEventArgs eventArgs = new(
            initiator: ShutdownInitiator.Library,
            replyCode: 42,
            replyText: "The answer to everything");

        // Act
        client.OnConnectionShutdown(null, eventArgs);

        // Assert
        client.IsConnectionOpen.Should().BeTrue();
    }

    /// <summary>
    /// Creates a RabbitMQ client.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The RabbitMQ client.</returns>
    internal static IRabbitMQClient CreateRabbitMQClient(IServiceProvider serviceProvider)
    {
        IRabbitMQClient serviceClient = serviceProvider.GetRequiredService<RabbitMQClient>();
        TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
        if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
        {
            serviceClient = serviceProvider.GetRequiredService<FakeRabbitMQClient>();
        }

        return serviceClient;
    }
}
