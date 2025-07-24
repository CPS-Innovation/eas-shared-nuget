// <copyright file="QueuePublisherOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// RabbitMQ queue options for a publisher.
/// </summary>
public class QueuePublisherOptions : QueueOptionsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueuePublisherOptions"/> class.
    /// </summary>
    public QueuePublisherOptions()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueuePublisherOptions"/> class.
    /// </summary>
    /// <param name="queueName">The name of the queue to publish messages to.</param>
    [JsonConstructor]
    public QueuePublisherOptions(string queueName)
        : base(queueName)
    {
    }
}
