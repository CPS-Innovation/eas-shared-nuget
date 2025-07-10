// <copyright file="HealthCheckInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core.Diagnostics;

/// <summary>
/// Information required to configure a health check that can be used by liveness, readiness and startup probes for containers.
/// </summary>
/// <param name="Pattern">The URL pattern of the health checks endpoint.</param>
/// <param name="Tag">The tag name used to categorise the health check so that they can be filtered.</param>
public record HealthCheckInfo(
    string Pattern,
    string Tag)
{
    /// <summary>
    /// The URL pattern of the liveness health checks endpoint.
    /// </summary>
    public const string LivenessPattern = "/healthz/live";

    /// <summary>
    /// Tag used to identify all liveness health checks.
    /// </summary>
    public const string LivenessTag = "liveness";

    /// <summary>
    /// Gets the <see cref="HealthCheckInfo"/> for the liveness health check.
    /// </summary>
    public static HealthCheckInfo LivenessHealthCheckInfo => new(
        Pattern: LivenessPattern,
        Tag: LivenessTag);
}
