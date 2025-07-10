// <copyright file="TestOptionsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Configuration;

using Cps.Fct.EasShared.Core.Configuration;

/// <summary>
/// Tests <see cref="TestOptions"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class TestOptionsTest
{
    /// <summary>
    /// Constructor test.
    /// </summary>
    [Fact]
    public void CreateTestOptions()
    {
        // Arrange
        const string UnitTest = nameof(UnitTest);

        TestOptions option1 = new()
        {
            Category = UnitTest,
        };

        // Act
        TestOptions option2 = new();

        // Assert
        option2.Should().BeEquivalentTo(option1);
    }
}
