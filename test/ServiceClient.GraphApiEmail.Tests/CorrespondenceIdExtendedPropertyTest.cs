// <copyright file="CorrespondenceIdExtendedPropertyTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Tests;

using System;
using System.Globalization;

/// <summary>
/// Tests <see cref="CorrespondenceIdExtendedProperty"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class CorrespondenceIdExtendedPropertyTest
{
    /// <summary>
    /// Calling <see cref="CorrespondenceIdExtendedProperty.TryParse(string, out CorrespondenceIdExtendedProperty?)"/> with a valid value succeeds.
    /// </summary>
    /// <param name="eventTime">The event time.</param>
    /// <param name="correspondenceId">The correspondence ID.</param>
    [Theory]
    [InlineData("2023-03-01T14:11:48.000Z", "8b00de66-15c7-419a-b1cf-071fa725203c")]
    [InlineData("2023-03-01T14:11:48.334Z", "8b00de66-15c7-419a-b1cf-071fa725203c")]
    public void TryParseReturnsTrue(string eventTime, string correspondenceId)
    {
        // Act
        bool result = CorrespondenceIdExtendedProperty.TryParse($"{eventTime}|{correspondenceId}", out CorrespondenceIdExtendedProperty? correspondenceIdExtendedProperty);

        // Assert
        result.Should().BeTrue();
        correspondenceIdExtendedProperty.Should().NotBeNull();
        correspondenceIdExtendedProperty!.EventTime.Should().Be(DateTimeOffset.Parse(eventTime, CultureInfo.InvariantCulture));
        correspondenceIdExtendedProperty.CorrespondenceId.Should().Be(Guid.Parse(correspondenceId));
        correspondenceIdExtendedProperty.ExtendedPropertyValue.Should().Be($"{eventTime}|{correspondenceId}");
    }

    /// <summary>
    /// Calling <see cref="CorrespondenceIdExtendedProperty.TryParse(string, out CorrespondenceIdExtendedProperty?)"/> with an invalid value fails.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid property")]
    [InlineData("|")]
    [InlineData("invalid.date|invalid.guid")]
    [InlineData("2023-03-01T14:11:48.334Z|")]
    [InlineData("2023-03-01T14:11:48.334Z|invalid.guid")]
    public void TryParseReturnsFalse(string? value)
    {
        // Act
        bool result = CorrespondenceIdExtendedProperty.TryParse(value!, out CorrespondenceIdExtendedProperty? correspondenceIdExtendedProperty);

        // Assert
        result.Should().BeFalse();
        correspondenceIdExtendedProperty.Should().BeNull();
    }
}
