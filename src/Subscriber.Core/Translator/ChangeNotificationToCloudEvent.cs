// <copyright file="ChangeNotificationToCloudEvent.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Core.Interfaces;
using Microsoft.Graph.Models;

/// <summary>
/// Translates a <see cref="ChangeNotification"/> to a <see cref="CloudEvent"/>
/// with a payload that is a <see cref="NewEmailNotificationReceived"/>.
/// </summary>
public class ChangeNotificationToCloudEvent :
    IEntityTranslator<ChangeNotification, CloudEvent>
{
    private readonly MessageReceivedToCloudEvent translator = new();

    /// <inheritdoc/>
    public CloudEvent Translate(ChangeNotification source)
    {
        return this.translator.Translate(new MessageReceived(source));
    }
}
