// <copyright file="EmbeddedResourceUtilityTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests;

using System;

/// <summary>
/// Tests <see cref="EmbeddedResourceUtility"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EmbeddedResourceUtilityTests
{
    /// <summary>
    /// Load embedded resource succeeds.
    /// </summary>
    [Fact]
    public void LoadEmbeddedResourceSucceeds()
    {
        // Assemble
        Type type = typeof(EmbeddedResourceUtilityTests);

        // Act
        string content = EmbeddedResourceUtility.LoadEmbeddedResource(" Cps.Fct.EasShared.Core.Tests.Embedded.Sample.embedded.resource.txt", type);

        // Assert
        content.Should().Contain("A sample embedded resource.");
    }

    /// <summary>
    /// Load embedded resource fails.
    /// </summary>
    [Fact]
    public void LoadEmbeddedResourceFails()
    {
        // Assemble
        Type type = typeof(EmbeddedResourceUtilityTests);
        string notFoundResource = "Does.not.exist";

        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => EmbeddedResourceUtility.LoadEmbeddedResource(notFoundResource, type));

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Contains(notFoundResource);
    }

    /// <summary>
    /// Load embedded binary resource succeeds.
    /// </summary>
    [Fact]
    public void LoadEmbeddedBinaryResourceSucceeds()
    {
        // Assemble
        Type type = typeof(EmbeddedResourceUtilityTests);

        // Act
        byte[] content = EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary(" Cps.Fct.EasShared.Core.Tests.Embedded.Sample.embedded.resource.txt", type);

        // Assert
        content.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Load embedded binary resource fails.
    /// </summary>
    [Fact]
    public void LoadEmbeddedBinaryResourceFails()
    {
        // Assemble
        Type type = typeof(EmbeddedResourceUtilityTests);
        string notFoundResource = "Does.not.exist";

        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => EmbeddedResourceUtility.LoadEmbeddedResourceAsBinary(notFoundResource, type));

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Contains(notFoundResource);
    }
}
