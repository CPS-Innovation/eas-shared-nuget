// <copyright file="TimeUtilityTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests;

using System;

/// <summary>
/// Tests for <see cref="TimeUtility"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class TimeUtilityTest
{
    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeUtilityTest"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public TimeUtilityTest(ITestOutputHelper output)
    {
        Requires.NotNull(output);

        this.output = output;
    }

    /// <summary>
    /// Test that <see cref="TimeUtility.GetUKLocalTime(DateTimeOffset)"/> returns a string containing "UK".
    /// </summary>
    [Fact]
    public void GetUKLocalTimeSucceeds()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Act
        string ukLocalTime = TimeUtility.GetUKLocalTime(now);

        // Assert
        ukLocalTime.Should().NotBeNullOrWhiteSpace();
        ukLocalTime.Should().Contain("UK");
    }

    /// <summary>
    /// Test that <see cref="TimeUtility.GetUKDateTimeOffset(DateTimeOffset)"/> returns a <see cref="DateTimeOffset"/> with the correct offset.
    /// </summary>
    [Fact]
    public void GetUKDateTimeOffsetSucceeds()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.Now;

        // Act
        DateTimeOffset result = TimeUtility.GetUKDateTimeOffset(now);

        // Assert
        this.output.WriteLine($"Now in local time = {now}; UK local time = {result}");
        result.Offset.Should().Be(TimeUtility.UKLocal.GetUtcOffset(now));
    }

    /// <summary>
    /// Test that <see cref="TimeUtility.GetUtcDateTimeOffset(DateTimeOffset)"/> returns a <see cref="DateTimeOffset"/> with a zero offset.
    /// </summary>
    [Fact]
    public void GetUtcDateTimeOffsetSucceeds()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.Now;

        // Act
        DateTimeOffset result = TimeUtility.GetUtcDateTimeOffset(now);

        // Assert
        this.output.WriteLine($"Now in local time = {now}; UK local time = {result}");
        result.Offset.Should().Be(TimeSpan.Zero);
    }
}
