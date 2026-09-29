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
    private bool _skipUntilCanAccessBucketLocally;

    public bool SkipUntilCanAccessBucketLocally
    {
        get => _skipUntilCanAccessBucketLocally;
        set
        {
            _skipUntilCanAccessBucketLocally = value;
            if (value)
                Skip = "Skipped until bucket is ready: copy target environment config files to staging bucket and configure staging credentials before running.";
        }
    }

    /// <inheritdoc />
    public override string? Skip
    {
        get => S3IntegrationAttributeBase.GetSkipReasonIfNotConfigured() ?? base.Skip;
        set => base.Skip = value;
    }
}
