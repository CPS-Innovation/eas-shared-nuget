// <copyright file="IRabbitMQClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

/// <summary>
/// An interface for a RabbitMQ client.
/// </summary>
public interface IRabbitMQClient : IDisposable
{
    /// <summary>
    /// Gets the RabbitMQ connection options.
    /// </summary>
    public RabbitMQConnectionOptions ConnectionOptions { get; }

    /// <summary>
    /// Gets a value indicating whether there is an open connection.
    /// </summary>
    bool IsConnectionOpen { get; }

    /// <summary>
    /// Connects to the RabbitMQ server if not already connected if <paramref name="forceConnect"/> is <c>false</c>.
    /// if <paramref name="forceConnect"/> is <c>true</c>, will create a new connection.
    /// </summary>
    /// <param name="forceConnect">if <c>true</c>, will create a new connection. Otherwise, will only create a connection if there is not already one.</param>
    /// <exception cref="ObjectDisposedException">If this instance is already disposed.</exception>
    void Connect(bool forceConnect = false);

    /// <summary>
    /// Connects to RabbitMQ and creates an AMQP model.
    /// </summary>
    /// <param name="forceConnect">if <c>true</c>, will create a new connection. Otherwise, will only create a connection if there is not already one.</param>
    /// <returns>The AMQP model.</returns>
    IModel CreateModel(bool forceConnect = false);

    /// <summary>
    /// Called when an exception occurs in a callback invoked by the connection.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    void OnCallbackException(object? sender, CallbackExceptionEventArgs e);

    /// <summary>
    /// Called when a connection is blocked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e);

    /// <summary>
    /// Called when a connection is destroyed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="reason">The event arguments.</param>
    void OnConnectionShutdown(object? sender, ShutdownEventArgs reason);
}
