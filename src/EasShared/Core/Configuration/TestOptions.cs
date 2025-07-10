// <copyright file="TestOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Test options.
/// </summary>
public class TestOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "Test";

    /// <summary>
    /// The unit test category.
    /// </summary>
    public const string UnitTest = nameof(UnitTest);

    /// <summary>
    /// Initializes a new instance of the <see cref="TestOptions"/> class.
    /// </summary>
    public TestOptions()
        : this(UnitTest)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestOptions"/> class.
    /// </summary>
    /// <param name="category">The test category - "UnitTest", "IntegrationTest", , "FeatureTest".</param>
    [JsonConstructor]
    public TestOptions(string category)
    {
        this.Category = category;
    }

    /// <summary>
    /// Gets the test category - "UnitTest", "IntegrationTest", , "FeatureTest".
    /// </summary>
    public string Category { get; init; }
}
