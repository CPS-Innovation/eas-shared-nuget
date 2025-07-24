// <copyright file="ParseNodeFactoryTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests;

using Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Tests for <see cref="ParseNodeFactory"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ParseNodeFactoryTest
{
    /// <summary>
    /// The method <c>CreateParseNode</c> succeeds.
    /// </summary>
    [Fact]
    public void CreateParseNodeSucceeds()
    {
        // Assemble
        string json = "{}";
        using ParseNodeFactory factory = new();

        // Act
        IParseNode node = factory.CreateParseNode(json);

        // Assert
        node.Should().NotBeNull();
    }
}
