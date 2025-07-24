// <copyright file="RabbitMQPublisher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Net.Sockets;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Publishes events to RabbitMQ.
/// </summary>
public class RabbitMQPublisher :
    IRabbitMQPublisher
{
    private readonly ILogger<RabbitMQPublisher> logger;
    private readonly IRabbitMQClient client;
    private readonly JsonEventFormatter jsonEventFormatter = new();
    private readonly RetryOptions retryOptions;
    private readonly RetryPolicy retryPolicy;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQPublisher"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="client">The RabbitMQ client.</param>
    /// <param name="retryOptions">The retry options.</param>
    public RabbitMQPublisher(
        ILoggerFactory loggerFactory,
        IRabbitMQClient client,
        IOptions<RetryOptions> retryOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(client);
        Requires.NotNull(retryOptions);

        this.logger = loggerFactory.CreateLogger<RabbitMQPublisher>();
        this.client = client;
        this.retryOptions = retryOptions.Value;

        this.retryPolicy = this.CreateRetryPolicy();
    }

    /// <inheritdoc/>
    public IModel CreateChannel(string queueName, bool forceClientCreate = false)
    {
        Requires.NotNullOrWhiteSpace(queueName);

        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        var watch = Stopwatch.StartNew();
        string outcome = "Failed";

        IModel channel;
        try
        {
            channel = this.client.CreateModel(forceClientCreate);
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            channel.ConfirmSelect();
            outcome = "Succeeded";
        }
        finally
        {
            this.logger.LogInformation(
                "RabbitMQ client attempted to create a channel for queue '{QueueName}' - Outcome: {Outcome} - Duration {Duration}",
                queueName,
                outcome,
                watch.Elapsed);
        }

        return channel;
    }

    /// <inheritdoc/>
    public IModel PublishEvent(string queueName, CloudEvent cloudEvent)
    {
        Requires.NotNull(cloudEvent);

        return this.PublishEvents(queueName, new CloudEvent[] { cloudEvent });
    }

    /// <inheritdoc/>
    public virtual IModel PublishEvents(string queueName, IEnumerable<CloudEvent> cloudEvents)
    {
        Requires.NotNullOrEmpty(cloudEvents);

        IModel channel = this.CreateChannel(queueName);
        foreach (CloudEvent cloudEvent in cloudEvents)
        {
            this.retryPolicy.Execute(() =>
            {
                this.PublishRawEvent(queueName, channel, this.jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out ContentType _));
            });
        }

        return channel;
    }

    /// <summary>
    /// Creates the basic properties for a channel.
    /// </summary>
    /// <param name="channel">The AMQP channel.</param>
    /// <returns>The basic properties.</returns>
    public virtual IBasicProperties CreateChannelBasicProperties(IModel channel)
    {
        return channel.CreateBasicProperties();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the current instance.
    /// </summary>
    /// <param name="disposing">Indicates whether this was called by the <c>Dispose()</c> method.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            this.disposedValue = true;
            if (disposing)
            {
                this.client.Dispose();
            }
        }
    }

    private RetryPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<SocketException>()
            .Or<IOException>()
            .WaitAndRetry(this.retryOptions.RetryCount, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (ex, time) =>
            {
                this.logger.LogWarning(ex, DiagnosticsUtility.Warning + "RabbitMQ publisher retry attempt failed after {TimeOut}", time);
            });
    }

    private void PublishRawEvent(string queueName, IModel channel, ReadOnlyMemory<byte> bytes)
    {
        string outcome = "Failed";
        try
        {
            IBasicProperties properties = this.CreateChannelBasicProperties(channel);
            properties.Persistent = true;
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: properties,
                body: bytes);
            channel.WaitForConfirmsOrDie(this.client.ConnectionOptions.ConfirmationTimeout);
            outcome = "Succeeded";
        }
        finally
        {
            this.logger.LogInformation(
                "Metric: RabbitMQ client published an event to queue '{QueueName}' of size {EventSize}. Outcome: {Outcome}",
                queueName,
                bytes.Length,
                outcome);
        }
    }
}
