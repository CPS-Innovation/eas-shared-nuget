// <copyright file="ChangeNotificationCollectionToCloudEventsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Translator;

using System.Collections.Generic;
using System.Linq;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Subscriber.Core.Model;
using Cps.Fct.EasShared.Subscriber.Core.Translator;
using Microsoft.Graph.Models;

/// <summary>
/// Tests <see cref="ChangeNotificationCollectionToCloudEvents"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ChangeNotificationCollectionToCloudEventsTest
{
    /// <summary>
    /// Change notification collection to CloudEvents translation succeeds.
    /// </summary>
    [Fact]
    public void TranslateSucceeds()
    {
        // Assemble
        string json = EmbeddedResourceUtility.LoadEmbeddedResource("Cps.Fct.EasShared.Subscriber.Core.Tests.Embedded.ChangeNotificationCollection.json", typeof(ChangeNotificationCollectionToCloudEventsTest));
        JsonToChangeNotificationCollectionResponse jsonToChangeNotificationCollectionResponse = new();
        ChangeNotificationCollectionResponse notifications = jsonToChangeNotificationCollectionResponse.Translate(json);
        ChangeNotificationCollectionToCloudEvents translator = new();

        // Act
        IReadOnlyDictionary<string, CloudEvent> result = translator.Translate(notifications!);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        IEnumerable<NewEmailNotificationReceived> notificationsReceived = result.Values
            .Where(e => e.Data is not null)
            .Select(e => e.Data as NewEmailNotificationReceived)
            .OfType<NewEmailNotificationReceived>();
        notificationsReceived.Should().HaveCount(1);
        NewEmailNotificationReceived notificationReceived = notificationsReceived.First();
        result.Keys.Should().Contain(notificationReceived.MessageId);
    }
}
