// <copyright file="FakeRabbitMQClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Emulation;

using Cps.Fct.EasShared.ServiceClient.RabbitMQ;
using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;
using Cps.Fct.EasShared.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The fake RabbitMQ client.
/// </summary>
public class FakeRabbitMQClient : RabbitMQClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeRabbitMQClient"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="connectionOptions">The RabbitMQ connection options.</param>
    /// <param name="retryOptions">The retry options.</param>
    public FakeRabbitMQClient(
        ConnectionFactory connectionFactory,
        ILogger<RabbitMQClient> logger,
        IOptions<RabbitMQConnectionOptions> connectionOptions,
        IOptions<RetryOptions> retryOptions)
            : base(connectionFactory, logger, connectionOptions, retryOptions)
    {
    }

    /// <inheritdoc/>
    public override IConnection CreateConnection()
    {
        var result = new Mock<IConnection>();
        return result.Object;
    }

    /// <inheritdoc/>
    public override IModel CreateModelPostConnection()
    {
        var result = new Mock<IModel>();
        return result.Object;
    }
}
