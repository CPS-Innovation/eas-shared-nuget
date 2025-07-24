// <copyright file="IGraphApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApi;

using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;

/// <summary>
/// An interface for a Graph API client.
/// </summary>
public interface IGraphApiClient
{
    /// <summary>
    /// Gets the configuration for the Graph API client.
    /// </summary>
    GraphApiConnectionOptions Configuration { get; }

    /// <summary>
    /// Gets the <see cref="GraphServiceClient"/> used to interact with the Graph API.
    /// </summary>
    GraphServiceClient GraphServiceClient { get; }

    /// <summary>
    /// Logs an OData error.
    /// </summary>
    /// <param name="error">The error to log.</param>
    void LogError(ODataError error);

    /// <summary>
    /// Forces a reauthorisation on the next call of the Graph API client.
    /// </summary>
    void ResetAuthorisation();
}
