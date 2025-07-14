// <copyright file="FakeRabbitMQSubscriber.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;

using System;
using System.Net.Mime;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The fake RabbitMQ subscriber.
/// </summary>
public class FakeRabbitMQSubscriber : RabbitMQSubscriber
{
    private readonly JsonEventFormatter jsonEventFormatter = new();

    private bool disposedValue;
    private Timer? timer;
    private ulong deliveryTag;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeRabbitMQSubscriber"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="client">The RabbitMQ client.</param>
    /// <param name="queueOptions">The queue options.</param>
    public FakeRabbitMQSubscriber(
        ILoggerFactory loggerFactory,
        IRabbitMQClient client,
        IOptions<QueueSubscriberOptions> queueOptions)
            : base(loggerFactory, client, queueOptions)
    {
    }

    /// <inheritdoc/>
    public override bool CanHandle(CloudEvent cloudEvent)
    {
        return true;
    }

    /// <inheritdoc/>
    public override void Process(CloudEvent cloudEvent)
    {
        Requires.NotNull(cloudEvent);
    }

    /// <inheritdoc/>
    public override void StartSubscribing(bool forceSubscribe = false)
    {
        base.StartSubscribing(forceSubscribe);
        if (this.timer is null)
        {
            this.timer = new(
                callback: this.CheckForEvents,
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMilliseconds(300));
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            this.disposedValue = true;
            if (disposing)
            {
                if (this.timer is not null)
                {
                    this.timer.Change(Timeout.Infinite, 0);
                    this.timer.Dispose();
                    this.timer = null;
                }
            }

            base.Dispose(disposing);
        }
    }

    private void CheckForEvents(object? state)
    {
        bool processEvents = true;
        while (processEvents)
        {
            if (FakeRabbitMQPublisher.PublishedCloudEvents.TryDequeue(out CloudEvent? cloudEvent))
            {
                BasicDeliverEventArgs args = Mock.Of<BasicDeliverEventArgs>();
                args.Body = this.jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out ContentType _);
                args.DeliveryTag = ++this.deliveryTag;
                this.ReceiveEvent(null, args);
            }
            else
            {
                processEvents = false;
            }
        }
    }
}
