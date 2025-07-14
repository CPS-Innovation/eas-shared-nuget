// <copyright file="ICloudEventProcessor.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core;

using Cps.Fct.EasShared.Core.Interfaces;

/// <summary>
/// An interface for cloud event processors.
/// </summary>
/// <typeparam name="TData">The <c>data</c> payload of the cloud event.</typeparam>
public interface ICloudEventProcessor<TData> : IEntityTranslator<CloudEvent, TData>
    where TData : class
{
    /// <summary>
    /// Processes a cloud event.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to process.</param>
    void Process(CloudEvent cloudEvent);
}
