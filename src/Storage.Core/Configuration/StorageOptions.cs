// <copyright file="StorageOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Storage.Core.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Storage options.
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageOptions"/> class.
    /// </summary>
    public StorageOptions()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageOptions"/> class.
    /// </summary>
    /// <param name="containerName">The storage container name.</param>
    [JsonConstructor]
    public StorageOptions(string containerName)
    {
        this.ContainerName = containerName;
    }

    /// <summary>
    /// Gets the storage container name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ContainerName { get; init; }
}
