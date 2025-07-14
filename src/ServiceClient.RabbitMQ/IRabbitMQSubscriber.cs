// <copyright file="IRabbitMQSubscriber.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using CloudNative.CloudEvents;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

/// <summary>
/// An interface for a RabbitMQ subscriber.
/// </summary>
public interface IRabbitMQSubscriber : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the subscriber has started subscribing.
    /// </summary>
    bool IsSubscribed { get; }

    /// <summary>
    /// Gets the queue options.
    /// </summary>
    QueueSubscriberOptions QueueOptions { get; }

    /// <summary>
    /// Checks whether the cloud event can be handled by the subscription.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to handle.</param>
    /// <returns>Returns <c>true</c> if the cloud event should be handled; otherwise returns <c>false</c>.</returns>
    bool CanHandle(CloudEvent cloudEvent);

    /// <summary>
    /// Process the cloud event.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to process.</param>
    void Process(CloudEvent cloudEvent);

    /// <summary>
    /// The subscription event handler.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event delivery arguments.</param>
    void ReceiveEvent(object? sender, BasicDeliverEventArgs e);

    /// <summary>
    /// Starts subscribing to events.
    /// </summary>
    /// <param name="forceSubscribe">if <c>true</c>, will resubscribe. Otherwise, will only subscribe if not already subscribed.</param>
    /// <exception cref="ObjectDisposedException">If this instance is already disposed.</exception>
    void StartSubscribing(bool forceSubscribe = false);
}
