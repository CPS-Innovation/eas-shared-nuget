// <copyright file="AmazonS3ClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws;

using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Factory for creating Amazon S3 API clients for different containers.
/// </summary>
public class AmazonS3ClientFactory : IAmazonS3ClientFactory
{
    private readonly ILoggerFactory loggerFactory;
    private readonly AmazonS3ApiOptions options;
    private readonly AmazonS3ClientFactoryOptions s3ClientFactoryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ClientFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="apiOptions">The S3 API options.</param>
    /// <param name="clientFactoryOptions">The S3 Client Factory options.</param>
    public AmazonS3ClientFactory(
        ILoggerFactory loggerFactory,
        IOptions<AmazonS3ApiOptions> apiOptions,
        IOptions<AmazonS3ClientFactoryOptions> clientFactoryOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(apiOptions);
        Requires.NotNull(clientFactoryOptions);

        this.loggerFactory = loggerFactory;
        this.options = apiOptions.Value;
        this.s3ClientFactoryOptions = clientFactoryOptions.Value;
    }

    /// <inheritdoc/>
    public IStorageClient CreateClient(string containerName)
    {
        Requires.NotNullOrWhiteSpace(containerName);

        var clientOptions = new AmazonS3ApiOptions(containerName, this.options.RegionEndpoint)
        {
            AccessKey = this.options.AccessKey,
            SecretKey = this.options.SecretKey,
            ServiceEndpoint = this.options.ServiceEndpoint,
        };

        return new AmazonS3ApiClient(this.loggerFactory, Options.Create(clientOptions));
    }

    /// <inheritdoc/>
    public IStorageClient GetEmailStorageClient()
    {
        return this.CreateClient(this.s3ClientFactoryOptions.EmailStorageContainerName
            ?? throw new InvalidOperationException("EmailStorageContainerName is not configured"));
    }

    /// <inheritdoc/>
    public IStorageClient GetMailboxConfigurationClient()
    {
        return this.CreateClient(this.s3ClientFactoryOptions.MailboxConfigurationContainerName
            ?? throw new InvalidOperationException("MailboxConfigurationContainerName is not configured"));
    }
}