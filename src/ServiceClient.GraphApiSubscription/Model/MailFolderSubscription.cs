// <copyright file="MailFolderSubscription.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;

using System;

/// <summary>
/// A change notification subscription to the Graph API.
/// </summary>
/// <param name="ChangeType">
/// Indicates the type of change in the subscribed resource that will raise a change notification.
/// </param>
/// <param name="ClientState">
/// Specifies the value of the clientState property sent by the service in each change notification.
/// The maximum length is 128 characters.
/// </param>
/// <param name="ExpirationDateTime">The expiration time of the subscription.</param>
/// <param name="LifecycleNotificationUrl">
/// The lifecycle notification URL.
/// Optional. The URL of the endpoint that receives lifecycle notifications, including subscriptionRemoved, reauthorizationRequired, and missed notifications.
/// This URL must make use of the HTTPS protocol.
/// </param>
/// <param name="NotificationUrl">
/// The URL of the endpoint that will receive the change notifications. This URL must make use of the HTTPS protocol.
/// </param>
/// <param name="Resource">
/// Specifies the resource that will be monitored for changes. Do not include the base URL (https://graph.microsoft.com/v1.0/).
/// </param>
/// <param name="SubscriptionId">The subscription ID.</param>
public record MailFolderSubscription(
    string ChangeType,
    string? ClientState,
    DateTimeOffset ExpirationDateTime,
    string? LifecycleNotificationUrl,
    string NotificationUrl,
    string Resource,
    string SubscriptionId);
