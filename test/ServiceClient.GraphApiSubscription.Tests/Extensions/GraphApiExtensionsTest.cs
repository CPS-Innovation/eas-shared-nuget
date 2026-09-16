// <copyright file="GraphApiExtensionsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests.Extensions;

using System;
using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Extensions;

/// <summary>
/// Tests <see cref="GraphApiExtensions"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class GraphApiExtensionsTest
{
    /// <summary>
    /// Tests extension method <c>ToChangeNotificationSubscription</c>.
    /// </summary>
    [Fact]
    public void SubscriptionToChangeNotificationSubscription()
    {
        // Arrange
        Subscription from = new()
        {
            ChangeType = "created",
            ClientState = "xyz",
            ExpirationDateTime = DateTimeOffset.UtcNow,
            NotificationUrl = "http://localhost",
            Resource = "x",
            Id = Guid.NewGuid().ToString(),
        };

        // Act
        var destination = from.ToMailFolderSubscription();

        // Assert
        destination.ChangeType.Should().Be(from.ChangeType);
        destination.ClientState.Should().Be(from.ClientState);
        destination.ExpirationDateTime.Should().Be(from.ExpirationDateTime);
        destination.NotificationUrl.Should().Be(from.NotificationUrl);
        destination.Resource.Should().Be(from.Resource);
        destination.SubscriptionId.Should().Be(from.Id);
    }
}
