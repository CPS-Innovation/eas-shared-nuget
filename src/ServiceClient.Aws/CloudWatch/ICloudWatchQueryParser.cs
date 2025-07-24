// <copyright file="ICloudWatchQueryParser.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.EasShared.ServiceClient.Aws.CloudWatch;

/// <summary>
/// Interface for cloud query watcher.
/// </summary>
public interface ICloudWatchQueryParser
{
    /// <summary>
    /// Gets a CloudWatch query template, and populates the elements.
    /// </summary>
    /// <param name="template">The query template.</param>
    /// <param name="parameters">The parameters to populate the template.</param>
    /// <returns>The parsed template.</returns>
    public string GetCloudWatchQuery(string template, Dictionary<string, string> parameters);
}
