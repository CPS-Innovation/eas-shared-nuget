// <copyright file="FakeS3ClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Emulation;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Fake S3 client factory for unit tests.
/// Returns test mailbox configuration data from embedded resource for mailbox config,
/// and uses FakeAmazonS3ApiClient for email storage.
/// </summary>
public class FakeS3ClientFactory : IAmazonS3ClientFactory
{
    private readonly IStorageClient mockMailboxConfigClient;
    private readonly IStorageClient fakeEmailStorageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeS3ClientFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="apiOptions">The S3 API options.</param>
    public FakeS3ClientFactory(
        ILoggerFactory loggerFactory,
        IOptions<AmazonS3ApiOptions> apiOptions)
    {
        this.mockMailboxConfigClient = new FakeMailboxConfigurationStorageClient();
        this.fakeEmailStorageClient = new FakeAmazonS3ApiClient(loggerFactory, apiOptions);
    }

    /// <inheritdoc/>
    public IStorageClient CreateClient(string containerName)
    {
        // Default to email storage for generic requests
        return this.fakeEmailStorageClient;
    }

    /// <inheritdoc/>
    public IStorageClient GetEmailStorageClient()
    {
        return this.fakeEmailStorageClient;
    }

    /// <inheritdoc/>
    public IStorageClient GetMailboxConfigurationClient()
    {
        return this.mockMailboxConfigClient;
    }
}
