// <copyright file="S3IntegrationFactAttribute.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.IntegrationTests;

using Xunit;

/// <summary>
/// Custom fact attribute for integration tests that require S3/AWS credentials.
/// Automatically skips when AWS credentials are not available.
/// </summary>
public sealed class S3IntegrationFactAttribute : S3IntegrationAttributeBase
{
}
