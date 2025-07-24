// <copyright file="ExceptionExtensionsTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Tests.Diagnostics;

using System;
using Cps.Fct.EasShared.Core.Diagnostics;

/// <summary>
/// Tests <see cref="ExceptionExtensions"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ExceptionExtensionsTest
{
    /// <summary>
    /// Tests <see cref="ExceptionExtensions.ToAggregatedMessage"/>.
    /// </summary>
    [Fact]
    public void ToAggregatedMessageSucceeds()
    {
        // Arrange
        string innerInnerMessage = "Message 1";
        string innerMessage = "Message 2";
        string message = "Message 3";
        Exception innerInnerException = new(innerInnerMessage);
        Exception innerException = new(innerMessage, innerInnerException);
        Exception exception = new(message, innerException);

        // Act
        string actual = exception.ToAggregatedMessage();

        // Assert
        actual.Should().Be($"{innerInnerMessage}{Environment.NewLine}{innerMessage}{Environment.NewLine}{message}");
    }
}
