// <copyright file="FakeOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Fake options used for testing.
/// </summary>
public class FakeOptions
{
    /// <summary>
    /// The default parent configuration section name.
    /// </summary>
    public const string ParentSectionName = "Fakes";

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeOptions"/> class.
    /// </summary>
    public FakeOptions()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeOptions"/> class.
    /// </summary>
    /// <param name="name">The name of the application.</param>
    [JsonConstructor]
    public FakeOptions(
        string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Name { get; init; }
}
