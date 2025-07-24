// <copyright file="ChangeNotificationCollectionToCloudEvents.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using System.Collections.Generic;
using Cps.Fct.EasShared.Core.Interfaces;
using Microsoft.Graph.Models;

/// <summary>
/// Translates a <see cref="ChangeNotificationCollection"/> to a dictionary of <see cref="CloudEvent"/>s keyed on message ID.
/// </summary>
public class ChangeNotificationCollectionToCloudEvents :
    IEntityTranslator<ChangeNotificationCollectionResponse, IReadOnlyDictionary<string, CloudEvent>>
{
    private readonly ChangeNotificationToCloudEvent translator = new();

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, CloudEvent> Translate(ChangeNotificationCollectionResponse source)
    {
        Requires.NotNull(source);

        Dictionary<string, CloudEvent> result = new();
        foreach (ChangeNotification notification in source.Value!)
        {
            if (notification.ChangeType == ChangeType.Created)
            {
                result.Add(notification.ResourceData!.AdditionalData["id"].ToString()!, this.translator.Translate(notification));
            }
        }

        return result;
    }
}
