// <copyright file="MockS3ClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Emulation;

using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.Storage.Core;

/// <summary>
/// Mock S3 client factory for unit tests.
/// Returns test mailbox configuration data from embedded resource.
/// </summary>
public class MockS3ClientFactory : IAmazonS3ClientFactory
{
    private const string ResourceNamespace = "Cps.Fct.EasShared.ServiceClient.Aws.Emulation.Embedded";
    private const string ConfigFileName = "appsettings.UnitTest.json";
    private readonly IStorageClient mockStorageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockS3ClientFactory"/> class.
    /// </summary>
    public MockS3ClientFactory()
    {
        this.mockStorageClient = new MockStorageClient();
    }

    /// <inheritdoc/>
    public IStorageClient CreateClient(string containerName)
    {
        return this.mockStorageClient;
    }

    /// <inheritdoc/>
    public IStorageClient GetEmailStorageClient()
    {
        return this.mockStorageClient;
    }

    /// <inheritdoc/>
    public IStorageClient GetMailboxConfigurationClient()
    {
        return this.mockStorageClient;
    }

    /// <summary>
    /// Mock storage client that returns test configuration.
    /// </summary>
    private sealed class MockStorageClient : IStorageClient
    {
        public Task DeleteObjectAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public string GetAccessUrl(string key, TimeSpan expiryInterval)
        {
            return string.Empty;
        }

        public string GetDeleteUrl(string key, TimeSpan expiryInterval)
        {
            return string.Empty;
        }

        public Task<TValue> GetObjectAsync<TValue>(string key, CancellationToken cancellationToken = default)
        {
            return this.GetObjectAsync<TValue>(key, ApplicationOptions.ApplicationSerializerOptions, cancellationToken);
        }

        public Task<TValue> GetObjectAsync<TValue>(string key, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken = default)
        {
            string json = EmbeddedResourceUtility.LoadEmbeddedResource(
                $"{ResourceNamespace}.{ConfigFileName}",
                typeof(MockS3ClientFactory));

            TValue result = JsonSerializer.Deserialize<TValue>(json, serializerOptions)!;
            return Task.FromResult(result);
        }

        public Task<IEnumerable<string>> ListObjectKeysAsync(string? prefix = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<string> keys = new List<string> { ConfigFileName };
            return Task.FromResult(keys);
        }

        public Task<bool> ObjectExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task PutObjectAsync<TValue>(string key, TValue value, CancellationToken cancellationToken = default) where TValue : notnull
        {
            return Task.CompletedTask;
        }

        public Task PutObjectAsync<TValue>(string key, TValue value, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken = default) where TValue : notnull
        {
            return Task.CompletedTask;
        }

        public Task PutStreamAsync(string key, Stream stream, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
































