// <copyright file="RabbitMQSubscriber.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using System;
using System.Diagnostics;
using System.Net.Mime;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// RabbitMQ events subscriber.
/// </summary>
public abstract class RabbitMQSubscriber
    : IRabbitMQSubscriber
{
    private readonly ILogger<RabbitMQSubscriber> logger;
    private readonly IRabbitMQClient client;
    private readonly JsonEventFormatter jsonEventFormatter = new();
    private readonly object sync = new();

    private IModel? channel;
    private bool subscribed;
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQSubscriber"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="client">The RabbitMQ client.</param>
    /// <param name="queueOptions">The queue options.</param>
    protected RabbitMQSubscriber(
        ILoggerFactory loggerFactory,
        IRabbitMQClient client,
        IOptions<QueueSubscriberOptions> queueOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(client);
        Requires.NotNull(queueOptions);
        Requires.NotNull(queueOptions.Value);
        Requires.NotNullOrWhiteSpace(queueOptions.Value.QueueName);

        this.logger = loggerFactory.CreateLogger<RabbitMQSubscriber>();
        this.client = client;
        this.QueueOptions = queueOptions.Value;
    }

    /// <summary>
    /// Gets the queue options.
    /// </summary>
    public QueueSubscriberOptions QueueOptions { get; }

    /// <summary>
    /// Gets a value indicating whether the subscriber has started subscribing.
    /// </summary>
    public bool IsSubscribed => !this.disposedValue && this.subscribed;

    /// <summary>
    /// The subscription event handler.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event delivery arguments.</param>
    public void ReceiveEvent(object? sender, BasicDeliverEventArgs e)
    {
        Requires.NotNull(e);

        if (!this.disposedValue && this.channel != null)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                byte[] body = e.Body.ToArray();
                CloudEvent cloudEvent = this.jsonEventFormatter.DecodeStructuredModeMessage(body, new ContentType("application/json"), null);
                if (this.CanHandle(cloudEvent))
                {
                    this.Process(cloudEvent);
                }

                this.channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                string outcome = "Succeeded";
                this.logger.LogInformation(
                    "RabbitMQ subscriber processed event for queue '{QueueName}' - Outcome: {Outcome} - Duration {Duration}",
                    this.QueueOptions.QueueName,
                    outcome,
                    watch.Elapsed);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, DiagnosticsUtility.Error + "The RabbitMQ subscription handler failed. Failure: {Reason}", ex.ToAggregatedMessage());
                try
                {
                    this.channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: true);
                }
                catch (Exception nackEx)
                {
                    // Suppress this exception as breaks the background service from processing subscriptions.
                    this.logger.LogError(nackEx, DiagnosticsUtility.Error + "The RabbitMQ subscription handler failed to nack the message. Failure: {Reason}", nackEx.ToAggregatedMessage());
                }
            }
        }
    }

    /// <summary>
    /// Starts subscribing to events.
    /// </summary>
    /// <param name="forceSubscribe">if <c>true</c>, will resubscribe. Otherwise, will only subscribe if not already subscribed.</param>
    /// <exception cref="ObjectDisposedException">If this instance is already disposed.</exception>
    public virtual void StartSubscribing(bool forceSubscribe = false)
    {
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        if (forceSubscribe || !this.subscribed)
        {
            this.subscribed = false;
            var watch = Stopwatch.StartNew();
            string outcome = "Failed";

            try
            {
                lock (this.sync)
                {
                    if (!this.subscribed)
                    {
                        this.channel = this.client.CreateModel(forceSubscribe);
                        this.channel.QueueDeclare(
                            queue: this.QueueOptions.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);
                        this.channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        EventingBasicConsumer consumer = new(this.channel);
                        consumer.Received += this.ReceiveEvent;
                        this.channel.BasicConsume(
                            queue: this.QueueOptions.QueueName,
                            autoAck: false,
                            consumer: consumer);

                        this.subscribed = true;
                    }
                }

                outcome = "Succeeded";
            }
            finally
            {
                this.logger.LogInformation(
                    "Milestone: RabbitMQ client attempted to create a subscription channel for queue '{QueueName}' - Outcome: {Outcome} - Duration {Duration}",
                    this.QueueOptions.QueueName,
                    outcome,
                    watch.Elapsed);
            }
        }
    }

    /// <summary>
    /// Checks whether the cloud event can be handled by the subscription.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to handle.</param>
    /// <returns>Returns <c>true</c> if the cloud event should be handled; otherwise returns <c>false</c>.</returns>
    public abstract bool CanHandle(CloudEvent cloudEvent);

    /// <summary>
    /// Process the cloud event.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to process.</param>
    public abstract void Process(CloudEvent cloudEvent);

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
                if (this.channel is not null)
                {
                    this.channel.Dispose();
                    this.channel = null;
                }

                this.client.Dispose();
            }
        }
    }
}
