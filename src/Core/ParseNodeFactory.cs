// <copyright file="ParseNodeFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core;

using System;
using System.Text;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

/// <summary>
/// A factory for creating <see cref="IParseNode"/> instances.
/// </summary>
public class ParseNodeFactory : IDisposable
{
    private static readonly JsonParseNodeFactory JsonParseNodeFactory = new();

    private bool disposedValue;
    private MemoryStream? memoryStream;

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates an <see cref="IParseNode"/> from the specified JSON.
    /// </summary>
    /// <param name="json">The JSON used to create an an <see cref="IParseNode"/>.</param>
    /// <returns>The <see cref="IParseNode"/> created from the specified JSON.</returns>
    public IParseNode CreateParseNode(string json)
    {
        this.memoryStream = new(Encoding.UTF8.GetBytes(json));
        return JsonParseNodeFactory.GetRootParseNodeAsync("application/json", this.memoryStream).ConfigureAwait(true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Disposes the current instance.
    /// </summary>
    /// <param name="disposing">Indicates whether this was called by the <c>Dispose()</c> method.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            this.disposedValue = true;
            if (disposing)
            {
                this.memoryStream?.Dispose();
            }

            this.memoryStream = null;
        }
    }
}
