// <copyright file="AmazonS3ApiOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

using System.Text.Json.Serialization;
using Amazon;
using Cps.Fct.EasShared.Storage.Core.Configuration;

/// <summary>
/// The AWS API options for an S3 bucket.
/// </summary>
public class AmazonS3ApiOptions : StorageOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "AmazonS3";

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ApiOptions"/> class.
    /// </summary>
    public AmazonS3ApiOptions()
        : this(string.Empty, RegionEndpoint.EUWest2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ApiOptions"/> class.
    /// </summary>
    /// <param name="containerName">The name of the Amazon S3 bucket.</param>
    /// <param name="regionEndpoint">The AWS region endpoint.</param>
    [JsonConstructor]
    public AmazonS3ApiOptions(
        string containerName,
        RegionEndpoint regionEndpoint)
            : base(containerName)
    {
        this.RegionEndpoint = regionEndpoint;
    }

    /// <summary>
    /// Gets the optional AWS credential access key. Not required when using an access point.
    /// </summary>
    public string? AccessKey { get; init; }

    /// <summary>
    /// Gets the optional AWS credential secret key. Not required when using an access point.
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    /// Gets the region endpoint.
    /// </summary>
    public RegionEndpoint RegionEndpoint { get; init; }

    /// <summary>
    /// Gets the service endpoint. Applies when using access points in a private network.
    /// See https://aws.amazon.com/blogs/developer/overriding-endpoints-in-the-aws-sdk-for-net/ and
    /// https://docs.aws.amazon.com/AmazonS3/latest/userguide/privatelink-interface-endpoints.html#accessing-s3-interface-endpoints.
    /// </summary>
    public string? ServiceEndpoint { get; init; }
}
