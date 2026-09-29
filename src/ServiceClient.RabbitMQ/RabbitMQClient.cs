// <copyright file="RabbitMQClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ;

using System;
using System.Diagnostics;
using System.Net.Sockets;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Cps.Fct.EasShared.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft;

/// <summary>
/// A client to a RabbitMQ instance.
/// </summary>
public class RabbitMQClient :
    IRabbitMQClient
{
    private readonly IConnectionFactory connectionFactory;
    private readonly ILogger logger;
    private readonly RetryOptions retryOptions;
    private readonly RetryPolicy retryPolicy;
    private readonly object sync = new();

    private bool disposedValue;
    private bool connected;
    private IConnection? connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQClient"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="connectionOptions">The RabbitMQ connection options.</param>
    /// <param name="retryOptions">The retry options.</param>
    public RabbitMQClient(
        ConnectionFactory connectionFactory,
        ILogger<RabbitMQClient> logger,
        IOptions<RabbitMQConnectionOptions> connectionOptions,
        IOptions<RetryOptions> retryOptions)
    {
        Requires.NotNull(connectionFactory);
        Requires.NotNull(logger);
        Requires.NotNull(connectionOptions);
        Requires.NotNull(retryOptions);

        this.ConnectionOptions = connectionOptions.Value;
        this.connectionFactory = connectionFactory;
        this.logger = logger;
        this.retryOptions = retryOptions.Value;

        connectionFactory.Port = this.ConnectionOptions.Port;
        connectionFactory.HostName = this.ConnectionOptions.HostName;
        connectionFactory.VirtualHost = this.ConnectionOptions.VirtualHost;
        connectionFactory.UserName = this.ConnectionOptions.UserName;
        connectionFactory.Password = this.ConnectionOptions.Password;
        connectionFactory.Ssl = new SslOption()
        {
            ServerName = this.ConnectionOptions.HostName,
            Enabled = this.ConnectionOptions.SslEnabled,
        };

        this.retryPolicy = this.CreateRetryPolicy();
    }

    /// <inheritdoc/>
    public RabbitMQConnectionOptions ConnectionOptions { get; }

    /// <summary>
    /// Gets a value indicating whether there is an open connection.
    /// </summary>
    public bool IsConnectionOpen => !this.disposedValue && this.connected;

    /// <summary>
    /// Connects to RabbitMQ and creates an AMQP model.
    /// </summary>
    /// <param name="forceConnect">if <c>true</c>, will create a new connection. Otherwise, will only create a connection if there is not already one.</param>
    /// <returns>The AMQP model.</returns>
    public IModel CreateModel(bool forceConnect = false)
    {
        this.Connect(forceConnect);
        return this.CreateModelPostConnection();
    }

    /// <summary>
    /// Creates an AMQP model assuming that a connection has already been established.
    /// </summary>
    /// <returns>The AMQP model.</returns>
    public virtual IModel CreateModelPostConnection()
    {
        return this.connection!.CreateModel();
    }

    /// <summary>
    /// Connects to the RabbitMQ server if not already connected if <paramref name="forceConnect"/> is <c>false</c>.
    /// if <paramref name="forceConnect"/> is <c>true</c>, will create a new connection.
    /// </summary>
    /// <param name="forceConnect">if <c>true</c>, will create a new connection. Otherwise, will only create a connection if there is not already one.</param>
    /// <exception cref="ObjectDisposedException">If this instance is already disposed.</exception>
    public void Connect(bool forceConnect = false)
    {
        ObjectDisposedException.ThrowIf(this.disposedValue, this);

        if (forceConnect || !this.connected)
        {
            this.connected = false;
            var watch = Stopwatch.StartNew();
            string outcome = "Failed";

            try
            {
                lock (this.sync)
                {
                    if (!this.connected)
                    {
                        this.retryPolicy.Execute(() =>
                        {
                            this.connection = this.CreateConnection();
                        });

                        this.connection!.ConnectionShutdown += this.OnConnectionShutdown;
                        this.connection!.CallbackException += this.OnCallbackException;
                        this.connection!.ConnectionBlocked += this.OnConnectionBlocked;
                        this.connected = true;
                    }
                }

                outcome = "Succeeded";

                const string InfoMessage = "RabbitMQ client connection attempted with user {User}. Outcome: {Outcome}; Duration {Duration}; Host: {HostName}; VirtualHost: {VirtualHost}; Port: {Port}";
                this.logger.LogInformation(
                    InfoMessage,
                    this.ConnectionOptions.UserName,
                    outcome,
                    watch.Elapsed,
                    this.ConnectionOptions.HostName,
                    this.ConnectionOptions.VirtualHost,
                    this.ConnectionOptions.Port);
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    DiagnosticsUtility.Error + "RabbitMQ client failed to connect with user {User}; Host: {HostName}; VirtualHost: {VirtualHost}; Port: {Port}",
                    this.ConnectionOptions.UserName,
                    this.ConnectionOptions.HostName,
                    this.ConnectionOptions.VirtualHost,
                    this.ConnectionOptions.Port);
                throw;
            }
        }
    }

    /// <summary>
    /// Creates the connection using a connection factory.
    /// </summary>
    /// <returns>The connection to RabbitMQ.</returns>
    public virtual IConnection CreateConnection()
    {
        return this.connectionFactory.CreateConnection();
    }

    /// <summary>
    /// Called when a connection is blocked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    public void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        if (!this.disposedValue)
        {
            this.logger.LogWarning(DiagnosticsUtility.Warning + "RabbitMQ client connection is blocked. Will try to re-connect.\n - Failure: {Reason}", e.Reason);
            this.Connect(forceConnect: true);
        }
    }

    /// <summary>
    /// Called when an exception occurs in a callback invoked by the connection.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    public void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        if (!this.disposedValue)
        {
            this.logger.LogWarning(DiagnosticsUtility.Warning + "RabbitMQ client exception detected. Will try to re-connect.\n - Failure: {Reason}", e.Exception.ToAggregatedMessage());
            this.Connect(forceConnect: true);
        }
    }

    /// <summary>
    /// Called when a connection is destroyed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="reason">The event arguments.</param>
    public void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
    {
        if (!this.disposedValue)
        {
            this.logger.LogWarning(DiagnosticsUtility.Warning + "RabbitMQ client connection is shutdown. Will try to re-connect.\n - Failure: {Reason}", reason);
            this.Connect(forceConnect: true);
        }
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
            if (disposing && this.connection != null)
            {
                this.connection.ConnectionShutdown -= this.OnConnectionShutdown;
                this.connection.CallbackException -= this.OnCallbackException;
                this.connection.ConnectionBlocked -= this.OnConnectionBlocked;
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }

    private RetryPolicy CreateRetryPolicy()
    {
        return Policy.Handle<SocketException>()
            .Or<IOException>()
            .WaitAndRetry(this.retryOptions.RetryCount, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (ex, time) =>
            {
                this.logger.LogWarning(ex, DiagnosticsUtility.Warning + "RabbitMQ client connection retry attempt failed after {TimeOut}", time);
            });
    }
}
