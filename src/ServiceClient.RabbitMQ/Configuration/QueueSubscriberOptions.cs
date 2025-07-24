// <copyright file="QueueSubscriberOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// RabbitMQ queue options for a publisher.
/// </summary>
public class QueueSubscriberOptions : QueueOptionsBase
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = $"{ParentSectionName}:Subscriber";

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueSubscriberOptions"/> class.
    /// </summary>
    public QueueSubscriberOptions()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueSubscriberOptions"/> class.
    /// </summary>
    /// <param name="queueName">The name of the queue to publish messages to.</param>
    [JsonConstructor]
    public QueueSubscriberOptions(string queueName)
        : base(queueName)
    {
    }
}
