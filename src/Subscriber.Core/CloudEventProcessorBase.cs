// <copyright file="CloudEventProcessorBase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core;

using System;
using System.Text.Json;

/// <summary>
/// The abstract cloud event processor.
/// </summary>
/// <typeparam name="TData">The <c>data</c> payload of the cloud event.</typeparam>
public abstract class CloudEventProcessorBase<TData> : ICloudEventProcessor<TData>
    where TData : class
{
    /// <inheritdoc/>
    public abstract void Process(CloudEvent cloudEvent);

    /// <inheritdoc/>
    public TData Translate(CloudEvent source)
    {
        TData? result = null;
        if (source.Data is TData data)
        {
            result = data;
        }

        if (result is null && source.Data is JsonElement jsonElement)
        {
            string json = jsonElement.GetRawText();
            result = JsonSerializer.Deserialize<TData>(json);
        }

        if (result is null)
        {
            throw new InvalidOperationException($"Expecting a cloud event with a data payload of type {typeof(TData)}");
        }

        return result;
    }
}
