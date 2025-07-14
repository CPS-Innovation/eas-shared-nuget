// <copyright file="CloudWatchClientOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

using Amazon;

/// <summary>
/// The AWS client options to access CloudWatch.
/// </summary>
public class CloudWatchClientOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "AmazonCloudWatch";

    /// <summary>
    /// Gets the optional AWS credential access key.
    /// </summary>
    public string? AccessKey { get; init; }

    /// <summary>
    /// Gets the region endpoint.
    /// </summary>
    public RegionEndpoint RegionEndpoint { get; init; } = RegionEndpoint.EUWest2;

    /// <summary>
    /// Gets the optional AWS credential secret key.
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    /// Gets the service endpoint. See https://docs.aws.amazon.com/general/latest/gr/cw_region.html.
    /// </summary>
    public string? ServiceEndpoint { get; init; }
}
