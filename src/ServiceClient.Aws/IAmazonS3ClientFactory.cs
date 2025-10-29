// <copyright file="IAmazonS3ClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws;

using Cps.Fct.EasShared.Storage.Core;

/// <summary>
/// Factory for creating Amazon S3 API clients for different containers.
/// </summary>
public interface IAmazonS3ClientFactory
{
    /// <summary>
    /// Creates an S3 storage client for the specified container.
    /// </summary>
    /// <param name="containerName">The S3 container (bucket) name.</param>
    /// <returns>A storage client configured for the specified container.</returns>
    IStorageClient CreateClient(string containerName);

    /// <summary>
    /// Gets the S3 client for email storage.
    /// </summary>
    /// <returns>A storage client configured for email storage.</returns>
    IStorageClient GetEmailStorageClient();

    /// <summary>
    /// Gets the S3 client for mailbox configuration.
    /// </summary>
    /// <returns>A storage client configured for mailbox configuration.</returns>
    IStorageClient GetMailboxConfigurationClient();
}