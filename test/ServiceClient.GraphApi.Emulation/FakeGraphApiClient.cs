// <copyright file="FakeGraphApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApi.Emulation;

using Cps.Fct.EasShared.ServiceClient.GraphApi;
using Cps.Fct.EasShared.ServiceClient.GraphApi.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;

/// <summary>
/// The fake client that can connect to the Graph API.
/// </summary>
public class FakeGraphApiClient : GraphApiClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeGraphApiClient"/> class.
    /// </summary>
    /// <param name="graphApiConnectionOption">The settings required to connect to the Graph API.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public FakeGraphApiClient(
        IOptions<GraphApiConnectionOptions> graphApiConnectionOption,
        ILoggerFactory loggerFactory)
            : base(graphApiConnectionOption, loggerFactory)
    {
    }

    /// <inheritdoc/>
    public override GraphServiceClient GraphServiceClient
    {
        get
        {
            var authenticationProvider = new Mock<IRequestAdapter>();
            return new GraphServiceClient(authenticationProvider.Object);
        }
    }
}
