// <copyright file="EntityValidatorTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Interfaces;

using Cps.Fct.EasShared.Core.Interfaces;

/// <summary>
/// Entity validator tests.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class EntityValidatorTest
{
    /// <summary>
    /// Validates succeeds.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="valid"><c>true</c> if the source is valid; otherwise <c>false</c>.</param>
    [Theory]
    [InlineData(null, false)]
    [InlineData("string", true)]
    public void ValidateSucceeds(object? source, bool valid)
    {
        // Assemble
        IEntityValidator<object> validator = new Validator();

        // Act
        ValidationResult result = validator.Validate(source!);

        // Assert
        result.Should().NotBeNull();
        result.Valid.Should().Be(valid);
        if (valid)
        {
            result.InvalidReason.Should().BeNull();
        }
        else
        {
            result.InvalidReason.Should().NotBeNullOrWhiteSpace();
        }
    }
}
