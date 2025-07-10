// <copyright file="GraphApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApi;

using Azure.Identity;
using Cps.Fct.Eas.Core.Diagnostics;
using Cps.EasShared.ServiceClient.GraphApi.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// A client that can connect to the Graph API.
/// </summary>
public class GraphApiClient : IGraphApiClient
{
    private static readonly string[] DefaultScopes = new string[] { "https://graph.microsoft.com/.default" };
    private readonly ILogger<GraphApiClient> logger;
    private readonly object sync = new();
    private GraphServiceClient? graphServiceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiClient"/> class.
    /// </summary>
    /// <param name="graphApiConnectionOption">
    /// The settings required to connect to the Graph API.
    /// </param>
    /// <param name="loggerFactory">
    /// The factory used to create instances of <see cref="ILogger"/> from the registered logger Providers.
    /// </param>
    public GraphApiClient(
        IOptions<GraphApiConnectionOptions> graphApiConnectionOption,
        ILoggerFactory loggerFactory)
    {
        Requires.NotNull(graphApiConnectionOption);
        Requires.NotNull(loggerFactory);

        this.Configuration = graphApiConnectionOption.Value;
        this.logger = loggerFactory.CreateLogger<GraphApiClient>();
    }

    /// <inheritdoc/>
    public virtual GraphServiceClient GraphServiceClient
    {
        get
        {
            if (this.graphServiceClient is null)
            {
                lock (this.sync)
                {
                    this.graphServiceClient ??= this.GetGraphClient();
                }
            }

            return this.graphServiceClient;
        }
    }

    /// <inheritdoc/>
    public GraphApiConnectionOptions Configuration { get; }

    /// <inheritdoc/>
    public void ResetAuthorisation()
    {
        lock (this.sync)
        {
            this.graphServiceClient = null;
        }

        this.logger.LogInformation("Graph API client reauthorise requested.");
    }

    /// <inheritdoc/>
    public void LogError(ODataError error)
    {
        this.logger.LogError(
            DiagnosticsUtility.Error + "An error occurred calling the Graph API.\n - Error code: {CodeText} ({Code})\n - Error reason: {Reason}",
            error.Error?.Code,
            error.ResponseStatusCode,
            error.Error?.Message);
    }

    private GraphServiceClient GetGraphClient()
    {
        ClientSecretCredential clientSecretCredential = new(
            tenantId: this.Configuration.TenantId,
            clientId: this.Configuration.ClientId,
            clientSecret: this.Configuration.ClientSecret);
        return new(clientSecretCredential, DefaultScopes);
    }
}
