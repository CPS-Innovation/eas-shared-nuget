// <copyright file="CloudEventProcessor.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests;

using CloudNative.CloudEvents;
using Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// A cloud event processor.
/// </summary>
public class CloudEventProcessor : CloudEventProcessorBase<NewEmailNotificationReceived>
{
    /// <summary>
    /// Gets a value indicating whether this object has been processed.
    /// </summary>
    public bool HasProcessed { get; private set; }

    /// <summary>
    /// Gets or sets the exception to throw when <c>Process</c> is called.
    /// </summary>
    public SubscriberException? ThrowOnNextProcessCall { get; set; }

    /// <inheritdoc/>
    public override void Process(CloudEvent cloudEvent)
    {
        Requires.NotNull(cloudEvent);
        if (this.ThrowOnNextProcessCall is not null)
        {
            SubscriberException saved = this.ThrowOnNextProcessCall;
            this.ThrowOnNextProcessCall = null;
            throw saved;
        }

        this.HasProcessed = true;
    }
}
