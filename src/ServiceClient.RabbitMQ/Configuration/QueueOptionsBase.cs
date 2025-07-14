// <copyright file="QueueOptionsBase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// RabbitMQ queue options.
/// </summary>
public class QueueOptionsBase
{
    /// <summary>
    /// The default parent section name in the appsettings.json file.
    /// </summary>
    public const string ParentSectionName = "Queues";

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueOptionsBase"/> class.
    /// </summary>
    protected QueueOptionsBase()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueOptionsBase"/> class.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    protected QueueOptionsBase(string queueName)
    {
        this.QueueName = queueName;
    }

    /// <summary>
    /// Gets the name of the queue.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string QueueName { get; init; }
}
