// <copyright file="IStorageClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Storage.Core;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface for a storage client.
/// </summary>
public interface IStorageClient
{
    /// <summary>
    /// Deletes an object from storage.
    /// </summary>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task DeleteObjectAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a pre-signed URL for accessing the storage object.
    /// </summary>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="expiryInterval">The expiry interval.</param>
    /// <returns>The pre-signed URL for accessing the storage object.</returns>
    string GetAccessUrl(
        string key,
        TimeSpan expiryInterval);

    /// <summary>
    /// Get a pre-signed URL for deleting a storage object.
    /// </summary>
    /// <param name="key">The key used to identify an object in storage.</param>
    /// <param name="expiryInterval">The expiry interval.</param>
    /// <returns>The pre-signed URL for deleting a storage object.</returns>
    string GetDeleteUrl(
        string key,
        TimeSpan expiryInterval);

    /// <summary>
    /// Gets an object from storage.
    /// </summary>
    /// <typeparam name="TValue">The object type.</typeparam>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The object if found, otherwise throws a not found exception.</returns>
    Task<TValue> GetObjectAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an object from storage.
    /// </summary>
    /// <typeparam name="TValue">The object type.</typeparam>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The object if found, otherwise throws a not found exception.</returns>
    Task<TValue> GetObjectAsync<TValue>(
        string key,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an object exists in storage.
    /// </summary>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns <c>true</c> if the object exists; otherwise returns <c>false</c>.</returns>
    Task<bool> ObjectExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts an object into storage.
    /// </summary>
    /// <typeparam name="TValue">The type of object to be serialized to storage.</typeparam>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="value">The object that needs to be serialized.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PutObjectAsync<TValue>(
        string key,
        TValue value,
        CancellationToken cancellationToken = default)
            where TValue : notnull;

    /// <summary>
    /// Puts an object into storage.
    /// </summary>
    /// <typeparam name="TValue">The type of object to be serialized to storage.</typeparam>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="value">The object that needs to be serialized.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PutObjectAsync<TValue>(
        string key,
        TValue value,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
            where TValue : notnull;

    /// <summary>
    /// Puts a stream of data into storage.
    /// </summary>
    /// <param name="key">The key used to identify the object in storage.</param>
    /// <param name="stream">The stream of raw data to save to storage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PutStreamAsync(string key, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all object keys in the storage with optional prefix filtering.
    /// </summary>
    /// <param name="prefix">Optional prefix to filter objects.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of object keys.</returns>
    Task<IEnumerable<string>> ListObjectKeysAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default);
}