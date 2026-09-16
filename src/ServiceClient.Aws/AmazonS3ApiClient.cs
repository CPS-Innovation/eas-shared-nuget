// <copyright file="AmazonS3ApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Amazon.Runtime;
using Amazon.S3.Model;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Diagnostics;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// A client for the Amazon API used to access S3 buckets.
/// </summary>
public class AmazonS3ApiClient : IStorageClient
{
    private readonly ILogger<AmazonS3ApiClient> logger;
    private readonly Lazy<AmazonS3Client> amazonS3Client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ApiClient"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="apiOptions">The S3 API options.</param>
    protected internal AmazonS3ApiClient(
        ILoggerFactory loggerFactory,
        IOptions<AmazonS3ApiOptions> apiOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(apiOptions);

        this.logger = loggerFactory.CreateLogger<AmazonS3ApiClient>();
        this.ApiOptions = apiOptions.Value;
        this.amazonS3Client = new Lazy<AmazonS3Client>(this.CreateAmazonS3Client);
    }

    /// <summary>
    /// Gets the Amazon S3 API options.
    /// </summary>
    public AmazonS3ApiOptions ApiOptions { get; }

    /// <summary>
    /// Gets the Amazon S3 client.
    /// </summary>
    public virtual IAmazonS3 Client => this.amazonS3Client.Value;

    /// <inheritdoc/>
    public virtual async Task DeleteObjectAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.DeleteAsync(this.GetDeleteUrl(key, TimeSpan.FromHours(2)), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public virtual string GetAccessUrl(
        string key,
        TimeSpan expiryInterval)
    {
        GetPreSignedUrlRequest request = new()
        {
            BucketName = this.ApiOptions.ContainerName,
            Key = key,
            Expires = DateTime.UtcNow + expiryInterval,
            Verb = HttpVerb.GET,
        };

        return this.Client.GetPreSignedURL(request);
    }

    /// <inheritdoc/>
    public virtual string GetDeleteUrl(
        string key,
        TimeSpan expiryInterval)
    {
        GetPreSignedUrlRequest request = new()
        {
            BucketName = this.ApiOptions.ContainerName,
            Key = key,
            Expires = DateTime.UtcNow + expiryInterval,
            Verb = HttpVerb.DELETE,
        };

        return this.Client.GetPreSignedURL(request);
    }

    /// <inheritdoc/>
    public virtual async Task<TValue> GetObjectAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await this.GetObjectAsync<TValue>(key, ApplicationOptions.ApplicationSerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<TValue> GetObjectAsync<TValue>(
        string key,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(key);

        var stopwatch = Stopwatch.StartNew();
        string outcome = "Failed";
        try
        {
            GetObjectRequest request = new()
            {
                BucketName = this.ApiOptions.ContainerName,
                Key = key,
            };

            using GetObjectResponse response = await this.Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
            TValue result = JsonSerializer.Deserialize<TValue>(response.ResponseStream, serializerOptions)!;
            outcome = "Succeeded";
            return result;
        }
        finally
        {
            this.logger.LogTrace(
                "Amazon S3 client attempted GET for an object with key {Key}\nOutcome: {Outcome}\nDuration : {Duration}",
                key,
                outcome,
                stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Determines whether an object exists in an S3 bucket.
    /// </summary>
    /// <param name="key">The key used to identify the object in S3.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns <c>true</c> if the object exists; otherwise returns <c>false</c>.</returns>
    public async Task<bool> ObjectExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await this.ThrowIfObjectDoesNotExistAsync(key, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            throw;
        }
    }

    /// <summary>
    /// Puts an object into an S3 bucket.
    /// </summary>
    /// <typeparam name="TValue">The type of object to be serialized to an S3 bucket.</typeparam>
    /// <param name="key">The key used to identify the object in S3.</param>
    /// <param name="value">The object that needs to be serialized.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    public Task PutObjectAsync<TValue>(
        string key,
        TValue value,
        CancellationToken cancellationToken = default)
            where TValue : notnull
    {
        return this.PutObjectAsync(key, value, ApplicationOptions.ApplicationSerializerOptions, cancellationToken);
    }

    /// <summary>
    /// Puts an object into an S3 bucket.
    /// </summary>
    /// <typeparam name="TValue">The type of object to be serialized to an S3 bucket.</typeparam>
    /// <param name="key">This key is used to identify the object in S3.</param>
    /// <param name="value">The object that needs to be serialized.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    public virtual async Task PutObjectAsync<TValue>(
        string key,
        TValue value,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
            where TValue : notnull
    {
        Requires.NotNullOrWhiteSpace(key);

        var stopwatch = Stopwatch.StartNew();
        string outcome = "Failed";
        try
        {
            PutObjectRequest request = new()
            {
                BucketName = this.ApiOptions.ContainerName,
                InputStream = new MemoryStream(),
                Key = key,
            };

            JsonSerializer.Serialize(request.InputStream, value, serializerOptions);
            await this.Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
            request.InputStream.Dispose();
            outcome = "Succeeded";
        }
        finally
        {
            this.logger.LogTrace(
                "Amazon S3 client attempted PUT for an object with key {Key}\nOutcome: {Outcome}\nDuration : {Duration}",
                key,
                outcome,
                stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Puts a raw stream into an S3 bucket.
    /// </summary>
    /// <param name="key">This key is used to identify the object in S3.</param>
    /// <param name="stream">The stream of raw data to save to the S3 bucket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    public virtual async Task PutStreamAsync(string key, Stream stream, CancellationToken cancellationToken = default)
    {
        Requires.NotNullOrWhiteSpace(key);

        var stopwatch = Stopwatch.StartNew();
        string outcome = "Failed";
        try
        {
            stream.Position = 0;
            PutObjectRequest request = new()
            {
                AutoCloseStream = false,
                BucketName = this.ApiOptions.ContainerName,
                InputStream = stream,
                Key = key,
            };

            await this.Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
            outcome = "Succeeded";
        }
        finally
        {
            this.logger.LogTrace(
                "Amazon S3 client attempted PUT for a stream with key {Key}\nOutcome: {Outcome}\nDuration : {Duration}",
                key,
                outcome,
                stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Throws an exception if the object does not exist when getting its metadata.
    /// </summary>
    /// <param name="key">The key used to identify the object in S3.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    public virtual async Task ThrowIfObjectDoesNotExistAsync(string key, CancellationToken cancellationToken)
    {
        await this.Client.GetObjectAsync(
            bucketName: this.ApiOptions.ContainerName,
            key: key,
            cancellationToken: cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>
    /// Logs a error.
    /// </summary>
    /// <param name="error">The error to log.</param>
    protected void LogError(Exception error)
    {
        Requires.NotNull(error);

        this.logger.LogError(
            error,
            DiagnosticsUtility.Error + "Amazon S3 raised an error.\n - Failure: {Reason}",
            error.ToAggregatedMessage());
    }

    private AmazonS3Client CreateAmazonS3Client()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(this.ApiOptions.AccessKey) && !string.IsNullOrWhiteSpace(this.ApiOptions.SecretKey))
            {
                BasicAWSCredentials credentials = new(
                    accessKey: this.ApiOptions.AccessKey,
                    secretKey: this.ApiOptions.SecretKey);
                return new AmazonS3Client(credentials: credentials, this.ApiOptions.RegionEndpoint);
            }

            AmazonS3Config config = new();
            if (this.ApiOptions.ServiceEndpoint is not null)
            {
                config.ServiceURL = this.ApiOptions.ServiceEndpoint;
            }

            return new AmazonS3Client(config: config);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, DiagnosticsUtility.Error + "Amazon S3 client creation failed with access key '{AccessKey}' and service endpoint '{ServiceEndpoint}'", this.ApiOptions.AccessKey, this.ApiOptions.ServiceEndpoint);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<string>> ListObjectKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        string outcome = "Failed";

        List<string> keys = new();

        try
        {
            string? continuationToken = null;

            do
            {
                ListObjectsV2Request request = new()
                {
                    BucketName = this.ApiOptions.ContainerName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                };

                ListObjectsV2Response response = await this.Client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

                keys.AddRange(response.S3Objects.Select(obj => obj.Key));

                continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
            }
            while (continuationToken != null);

            outcome = "Succeeded";
            return keys;
        }
        finally
        {
            this.logger.LogTrace(
                "Amazon S3 client attempted LIST for objects with prefix {Prefix}\nOutcome: {Outcome}\nDuration: {Duration}\nCount: {Count}",
                prefix ?? "(none)",
                outcome,
                stopwatch.Elapsed,
                outcome == "Succeeded" ? ((List<string>)keys).Count : 0);
        }
    }
}
