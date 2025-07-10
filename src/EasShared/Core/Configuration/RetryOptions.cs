// <copyright file="RetryOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Retry options.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryOptions"/> class.
    /// </summary>
    public RetryOptions()
        : this(3)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryOptions"/> class.
    /// </summary>
    /// <param name="retryCount">The retry count.</param>
    [JsonConstructor]
    public RetryOptions(int retryCount)
    {
        this.RetryCount = retryCount;
    }

    /// <summary>
    /// Gets the retry count.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int RetryCount { get; init; }
}
