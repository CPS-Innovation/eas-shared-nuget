// <copyright file="CloudWatchQueryInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Cloud Watch query information.
/// </summary>
public class CloudWatchQueryInfo
{
    /// <summary>
    /// Gets the Query Name.
    /// </summary>
    public string QueryName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the dictionary of parameters.
    /// </summary>
    public Dictionary<string, string> Parameters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
