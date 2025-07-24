// <copyright file="RabbitMQConnectionOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// The RabbitMQ connection options.
/// </summary>
public class RabbitMQConnectionOptions
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = "RabbitMQ";

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQConnectionOptions"/> class.
    /// </summary>
    public RabbitMQConnectionOptions()
        : this(
              confirmationTimeout: TimeSpan.FromSeconds(3.5),
              hostName: "localhost",
              password: "guest",
              port: 5672,
              sslEnabled: true,
              userName: "guest",
              virtualHost: string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQConnectionOptions"/> class.
    /// </summary>
    /// <param name="confirmationTimeout">The confirmation timeout.</param>
    /// <param name="hostName">The Rabbit MQ host name.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="port">The TCP port number.</param>
    /// <param name="sslEnabled">A value indicating whether the SSL protocol is used.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="virtualHost">The virtual host.</param>
    [JsonConstructor]
    public RabbitMQConnectionOptions(
        TimeSpan confirmationTimeout,
        string hostName,
        string password,
        int port,
        bool sslEnabled,
        string userName,
        string virtualHost)
    {
        this.ConfirmationTimeout = confirmationTimeout;
        this.HostName = hostName;
        this.Password = password;
        this.Port = port;
        this.SslEnabled = sslEnabled;
        this.UserName = userName;
        this.VirtualHost = virtualHost;
    }

    /// <summary>
    /// Gets the Rabbit MQ host name; for example, b-a0000000-0000-0000-0000-000000000000.mq.eu-west-2.amazonaws.com.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string HostName { get; init; }

    /// <summary>
    /// Gets the user's password. This should come from a secrets store.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Password { get; init; }

    /// <summary>
    /// Gets the TCP port number. The default is 5672.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets a value indicating whether the SSL protocol is used.
    /// </summary>
    public bool SslEnabled { get; init; }

    /// <summary>
    /// Gets the user name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string UserName { get; init; }

    /// <summary>
    /// Gets the virtual host. Leave empty for the root.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string VirtualHost { get; init; }

    /// <summary>
    /// Gets the confirmation timeout.
    /// </summary>
    /// <remarks>
    /// The publisher waits until all messages published since the last call have
    /// been ack'd by the broker. If a nack is received or the timeout
    /// elapses, throws an OperationInterruptedException exception immediately.
    /// </remarks>
    public TimeSpan ConfirmationTimeout { get; init; }
}
