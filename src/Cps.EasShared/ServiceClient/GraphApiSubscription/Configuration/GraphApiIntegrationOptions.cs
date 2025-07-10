// <copyright file="GraphApiIntegrationOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApiSubscription.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// The options required to integrate with the Graph API change notification system using a webhook.
/// </summary>
public class GraphApiIntegrationOptions
{
    /// <summary>
    /// The minimum length required for the webhook's client state.
    /// </summary>
    public const int WebhookClientStateMinLength = 12;

    /// <summary>
    /// The maximum length required for the webhook's client state.
    /// </summary>
    public const int WebhookClientStateMaxLength = 128;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiIntegrationOptions"/> class.
    /// </summary>
    public GraphApiIntegrationOptions()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiIntegrationOptions"/> class.
    /// </summary>
    /// <param name="webhookClientState">The value of the <c>webhookClientState</c> property sent by the service in each change notification.</param>
    [JsonConstructor]
    public GraphApiIntegrationOptions(
        string webhookClientState)
    {
        this.WebhookClientState = webhookClientState;
    }

    /// <summary>
    /// Gets the client state. Specifies the value of the clientState property sent by the service in each change notification.
    /// The maximum length is 128 characters. The client can check that the change notification came from the service by comparing the value
    /// of the clientState property sent with the subscription with the value of the clientState property received with each change notification.
    /// </summary>
    [MaxLength(WebhookClientStateMaxLength)]
    [MinLength(WebhookClientStateMinLength)]
    public string WebhookClientState { get; init; }
}
