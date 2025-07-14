// <copyright file="FakeRabbitMQPublisher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;

using System.Collections.Concurrent;
using System.Collections.Generic;
using CloudNative.CloudEvents;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The fake RabbitMQ publisher.
/// </summary>
public class FakeRabbitMQPublisher : RabbitMQPublisher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeRabbitMQPublisher"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="client">The RabbitMQ client.</param>
    /// <param name="retryOptions">The retry options.</param>
    public FakeRabbitMQPublisher(
        ILoggerFactory loggerFactory,
        IRabbitMQClient client,
        IOptions<RetryOptions> retryOptions)
            : base(loggerFactory, client, retryOptions)
    {
    }

    /// <summary>
    /// Gets the published cloud events.
    /// </summary>
    public static ConcurrentQueue<CloudEvent> PublishedCloudEvents { get; } = new();

    /// <inheritdoc/>
    public override IBasicProperties CreateChannelBasicProperties(IModel channel)
    {
        var result = new Mock<IBasicProperties>();
        return result.Object;
    }

    /// <inheritdoc/>
    public override IModel PublishEvents(string queueName, IEnumerable<CloudEvent> cloudEvents)
    {
        IModel result = base.PublishEvents(queueName, cloudEvents);
        foreach (CloudEvent cloudEvent in cloudEvents)
        {
            PublishedCloudEvents.Enqueue(cloudEvent);
        }

        return result;
    }
}
