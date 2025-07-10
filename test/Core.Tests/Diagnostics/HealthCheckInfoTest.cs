// <copyright file="HealthCheckInfoTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Diagnostics;

using Cps.Fct.EasShared.Core.Diagnostics;

/// <summary>
/// Tests <see cref="HealthCheckInfo"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class HealthCheckInfoTest
{
    /// <summary>
    /// <c>LivenessHealthCheckInfo</c> test.
    /// </summary>
    [Fact]
    public void ValidateLivenessHealthCheckInfo()
    {
        // Arrange
        HealthCheckInfo expected = new(
            Pattern: "/healthz/live",
            Tag: "liveness");

        // Act
        HealthCheckInfo actual = HealthCheckInfo.LivenessHealthCheckInfo;

        // Act
        actual.Should().BeEquivalentTo(expected);
    }
}
