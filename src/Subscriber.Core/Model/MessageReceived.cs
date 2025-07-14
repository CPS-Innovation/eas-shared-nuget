// <copyright file="MessageReceived.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Model;

using Microsoft.Graph.Models;

/// <summary>
/// A message received from the Graph API to indicate a change to a resource.
/// </summary>
/// <param name="ChangeType">The change type; for example, created.</param>
/// <param name="CloudEventType">The type of resource that was changed.</param>
/// <param name="ETag">The ETag for the resource.</param>
/// <param name="Id">The id of the resource.</param>
/// <param name="Resource">The resource string; for example, Users/a%40b.com/Messages.</param>
public record MessageReceived(
    string ChangeType,
    string CloudEventType,
    string ETag,
    string Id,
    string Resource)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageReceived"/> class.
    /// </summary>
    /// <param name="source">A <see cref="ChangeNotification"/> received from the Graph API.</param>
    public MessageReceived(ChangeNotification source)
        : this(
            ChangeType: source.ChangeType.ToString()!,
            CloudEventType: source.ResourceData!.OdataType!,
            ETag: source.ResourceData!.AdditionalData["@odata.etag"].ToString()!,
            Id: source.ResourceData!.AdditionalData["id"].ToString()!,
            Resource: source.Resource!)
    {
    }
}
