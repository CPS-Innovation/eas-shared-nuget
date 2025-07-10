// <copyright file="SubscriptionOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription.Configuration;

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cps.EasShared.ServiceClient.GraphApiSubscription.Interfaces;
using Microsoft;

/// <summary>
/// The settings required to manage a change notification subscription in the Graph API.
/// </summary>
public class SubscriptionOptions : IMailFolderIdentifier
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionOptions"/> class.
    /// </summary>
    public SubscriptionOptions()
        : this(
              changeType: "created",
              clientState: string.Empty,
              expirationInterval: TimeSpan.FromHours(69),
              folderName: "Inbox",
              lifecycleNotificationUrl: null,
              mailboxId: string.Empty,
              notificationUrl: string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionOptions"/> class.
    /// </summary>
    /// <param name="changeType">
    /// Indicates the type of change in the subscribed resource that will raise a change notification.
    /// </param>
    /// <param name="clientState">
    /// Specifies the value of the clientState property sent by the service in each change notification.
    /// The maximum length is 128 characters.
    /// </param>
    /// <param name="expirationInterval">The expiration interval.</param>
    /// <param name="folderName">The name of the folder to monitor for changes.</param>
    /// <param name="lifecycleNotificationUrl">The lifecycle notification URL.</param>
    /// <param name="mailboxId">The mailbox identifier. The identifier of the mailbox that will be monitored for changes. Can be either a user principal name or a user object ID.</param>
    /// <param name="notificationUrl">
    /// The URL of the endpoint that will receive the change notifications. This URL must make use of the HTTPS protocol.
    /// </param>
    [JsonConstructor]
    public SubscriptionOptions(
        string changeType,
        string clientState,
        TimeSpan expirationInterval,
        string folderName,
        string? lifecycleNotificationUrl,
        string mailboxId,
        string notificationUrl)
    {
        this.ChangeType = changeType;
        this.ClientState = clientState;
        this.ExpirationInterval = expirationInterval;
        this.FolderName = folderName;
        this.LifecycleNotificationUrl = lifecycleNotificationUrl;
        this.MailboxId = mailboxId;
        this.NotificationUrl = notificationUrl;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionOptions"/> class.
    /// </summary>
    /// <param name="options">Options to clone.</param>
    public SubscriptionOptions(
        SubscriptionOptions options)
    {
        Requires.NotNull(options);

        this.ChangeType = options.ChangeType;
        this.ClientState = options.ClientState;
        this.ExpirationInterval = options.ExpirationInterval;
        this.FolderName = options.FolderName;
        this.LifecycleNotificationUrl = options.LifecycleNotificationUrl;
        this.MailboxId = options.MailboxId;
        this.NotificationUrl = options.NotificationUrl;
    }

    /// <summary>
    /// Gets the change type. Indicates the type of change in the subscribed resource that will raise a change notification.
    /// The supported values are: created, updated, deleted. Multiple values can be combined using a comma-separated list.
    /// Note: Drive root item and list change notifications support only the updated changeType. User and group change notifications
    /// support updated and deleted changeType. Use updated to receive notifications when user or group is created, updated or soft deleted.
    /// Use deleted to receive notifications when user or group is permanently deleted.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ChangeType { get; init; }

    /// <summary>
    /// Gets the client state. Specifies the value of the clientState property sent by the service in each change notification.
    /// The maximum length is 128 characters. The client can check that the change notification came from the service by comparing the value
    /// of the clientState property sent with the subscription with the value of the clientState property received with each change notification.
    /// </summary>
    public string? ClientState { get; init; }

    /// <summary>
    /// Gets or sets the expiration interval. The maximum value depends on the resource, check the Graph API documentation. For mail subscriptions
    /// it is 3 days.
    /// </summary>
    public TimeSpan ExpirationInterval { get; set; }

    /// <summary>
    /// Gets the name of the mail folder that will be monitored for changes.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string FolderName { get; init; }

    /// <summary>
    /// Gets the lifecycle notification URL.
    /// Optional. The URL of the endpoint that receives lifecycle notifications, including subscriptionRemoved, reauthorizationRequired, and missed notifications. This URL must make use of the HTTPS protocol.
    /// </summary>
    public string? LifecycleNotificationUrl { get; init; }

    /// <summary>
    /// Gets the identifier of the mailbox that will be monitored for changes. Can be either a user principal name or a user object ID.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string MailboxId { get; init; }

    /// <summary>
    /// Gets notification URL. The URL of the endpoint that will receive the change notifications. This URL must make use of the HTTPS protocol.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string NotificationUrl { get; init; }
}
