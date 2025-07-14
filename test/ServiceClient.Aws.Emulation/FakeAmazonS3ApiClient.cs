// <copyright file="FakeAmazonS3ApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Emulation;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The fake client for the Amazon API used access S3 buckets.
/// </summary>
public class FakeAmazonS3ApiClient : AmazonS3ApiClient
{
    private static readonly ConcurrentDictionary<string, object> ObjectCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeAmazonS3ApiClient"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="apiOptions">The S3 API options.</param>
    public FakeAmazonS3ApiClient(
        ILoggerFactory loggerFactory,
        IOptions<AmazonS3ApiOptions> apiOptions)
            : base(loggerFactory, apiOptions)
    {
    }

    /// <inheritdoc/>
    public override IAmazonS3 Client
    {
        get
        {
            return Mock.Of<IAmazonS3>();
        }
    }

    /// <inheritdoc/>
    public override async Task DeleteObjectAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.DeleteObjectAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            this.LogError(error);
        }

        ObjectCache.TryRemove(key, out _);
    }

    /// <inheritdoc/>
    public override string GetAccessUrl(string key, TimeSpan expiryInterval)
    {
        string? result = null;
        try
        {
            result = base.GetAccessUrl(key, expiryInterval);
        }
        catch (Exception error)
        {
            this.LogError(error);
        }

        this.ThrowIfObjectDoesNotExistAsync(key).Wait();
        result ??= $"#{key}";
        return result;
    }

    /// <inheritdoc/>
    public override string GetDeleteUrl(string key, TimeSpan expiryInterval)
    {
        string? result = null;
        try
        {
            result = base.GetDeleteUrl(key, expiryInterval);
        }
        catch (Exception error)
        {
            this.LogError(error);
        }

        this.ThrowIfObjectDoesNotExistAsync(key).Wait();
        result ??= $"#{key}";
        return result;
    }

    /// <inheritdoc/>
    public override async Task<TValue> GetObjectAsync<TValue>(
        string key,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await base.GetObjectAsync<TValue>(key, serializerOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            this.LogError(error);
        }

        await this.ThrowIfObjectDoesNotExistAsync(key, cancellationToken).ConfigureAwait(false);
        ObjectCache.TryGetValue(key, out object? result);
        return result is not null && result is TValue value ? value : throw new InvalidOperationException($"Expecting a value of type {typeof(TValue)}");
    }

    /// <inheritdoc/>
    public override async Task PutObjectAsync<TValue>(
        string key,
        TValue value,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
    {
        await base.PutObjectAsync(key, value, serializerOptions, cancellationToken).ConfigureAwait(false);
        ObjectCache.TryAdd(key, value);
    }

    /// <inheritdoc/>
    public override async Task PutStreamAsync(string key, Stream stream, CancellationToken cancellationToken = default)
    {
        await base.PutStreamAsync(key, stream, cancellationToken).ConfigureAwait(false);
        byte[] content = new byte[stream.Length];
        stream.Position = 0;
        stream.Read(content, 0, content.Length);
        ObjectCache.TryAdd(key, content);
    }

    /// <inheritdoc/>
    public override async Task ThrowIfObjectDoesNotExistAsync(string key, CancellationToken cancellationToken = default)
    {
        await base.ThrowIfObjectDoesNotExistAsync(key, cancellationToken).ConfigureAwait(false);
        if (!ObjectCache.ContainsKey(key))
        {
            throw new AmazonS3Exception($"Key '{key}' not found")
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
            };
        }
    }
}
