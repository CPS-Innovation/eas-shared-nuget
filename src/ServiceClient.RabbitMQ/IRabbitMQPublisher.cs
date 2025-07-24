// <copyright file="IRabbitMQPublisher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using System.Collections.Generic;
using CloudNative.CloudEvents;

/// <summary>
/// An interface for a RabbitMQ event publisher.
/// </summary>
public interface IRabbitMQPublisher : IDisposable
{
    /// <summary>
    /// Creates an AMQP channel.
    /// </summary>
    /// <param name="queueName">The name of the queue to create the channel for.</param>
    /// <param name="forceClientCreate">if <c>true</c>, will create a new RabbitMQ client. Otherwise, will only create a RabbitMQ client if there is not already one.</param>
    /// <returns>The AMQP channel created.</returns>
    /// <exception cref="ObjectDisposedException">If this instance is already disposed.</exception>
    IModel CreateChannel(string queueName, bool forceClientCreate = false);

    /// <summary>
    /// Publishes a cloud event.
    /// </summary>
    /// <param name="queueName">The name of the queue to publish the event to.</param>
    /// <param name="cloudEvent">A cloud event to publish.</param>
    /// <returns>The AMQP channel created.</returns>
    IModel PublishEvent(string queueName, CloudEvent cloudEvent);

    /// <summary>
    /// Publishes cloud events.
    /// </summary>
    /// <param name="queueName">The name of the queue to publish the events to.</param>
    /// <param name="cloudEvents">The cloud events to publish.</param>
    /// <returns>The AMQP channel created.</returns>
    IModel PublishEvents(string queueName, IEnumerable<CloudEvent> cloudEvents);
}
