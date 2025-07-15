// <copyright file="EmailNotificationReceivedQueueOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Configuration;

using Cps.Fct.EasShared.ServiceClient.RabbitMQ.Configuration;

/// <summary>
/// The new email notification received queue options.
/// </summary>
public class EmailNotificationReceivedQueueOptions : QueuePublisherOptions
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = $"{ParentSectionName}:EmailNotificationReceived";
}
