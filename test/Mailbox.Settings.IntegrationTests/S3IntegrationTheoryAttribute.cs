// <copyright file="S3IntegrationTheoryAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;

using Xunit;

/// <summary>
/// Custom theory attribute for parameterized integration tests that require S3/AWS credentials.
/// Automatically skips when AWS credentials are not available.
/// </summary>
public sealed class S3IntegrationTheoryAttribute : TheoryAttribute
{
    /// <inheritdoc />
    public override string? Skip
    {
        get => S3IntegrationAttributeBase.GetSkipReasonIfNotConfigured() ?? base.Skip;
        set => base.Skip = value;
    }
}
