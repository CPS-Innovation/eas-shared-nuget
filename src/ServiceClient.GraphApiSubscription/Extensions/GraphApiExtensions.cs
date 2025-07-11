// <copyright file="GraphApiExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Extensions;

using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;

/// <summary>
/// Extension methods for Graph API entities.
/// </summary>
public static class GraphApiExtensions
{
    /// <summary>
    /// Creates a <see cref="MailFolderSubscription"/> from a <see cref="Subscription"/>.
    /// </summary>
    /// <param name="subscription">A Graph API subscription.</param>
    /// <returns>A <see cref="MailFolderSubscription"/> from a <see cref="Subscription"/>.</returns>
    public static MailFolderSubscription ToMailFolderSubscription(
        this Subscription subscription)
    {
        return new MailFolderSubscription(
            ChangeType: subscription.ChangeType!,
            ClientState: subscription.ClientState,
            ExpirationDateTime: subscription.ExpirationDateTime!.Value,
            LifecycleNotificationUrl: subscription.LifecycleNotificationUrl,
            NotificationUrl: subscription.NotificationUrl!,
            Resource: subscription.Resource!,
            SubscriptionId: subscription.Id!);
    }

    /// <summary>
    /// Creates a <see cref="Subscription"/> from a <see cref="MailFolderSubscription"/>.
    /// </summary>
    /// <param name="mailFolderSubscription">The mail folder subscription entity to convert.</param>
    /// <returns>
    /// Returns a <see cref="Subscription"/> from a <see cref="MailFolderSubscription"/>.
    /// </returns>
    public static Subscription ToSubscription(
        this MailFolderSubscription mailFolderSubscription)
    {
        return new Subscription()
        {
            ChangeType = mailFolderSubscription.ChangeType,
            ClientState = mailFolderSubscription.ClientState,
            ExpirationDateTime = mailFolderSubscription.ExpirationDateTime,
            LifecycleNotificationUrl = mailFolderSubscription.LifecycleNotificationUrl,
            NotificationUrl = mailFolderSubscription.NotificationUrl,
            Resource = mailFolderSubscription.Resource,
            Id = mailFolderSubscription.SubscriptionId,
        };
    }
}
