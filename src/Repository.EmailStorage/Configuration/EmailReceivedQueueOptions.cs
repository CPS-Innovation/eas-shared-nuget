// <copyright file="EmailReceivedQueueOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Configuration;

using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

/// <summary>
/// The new email received queue options.
/// </summary>
public class EmailReceivedQueueOptions : QueuePublisherOptions
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = "Queues:EmailReceived";
}
