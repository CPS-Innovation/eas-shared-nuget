// <copyright file="ICloudWatchQueryClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.EasShared.ServiceClient.Aws.CloudWatch;

using System.Threading.Tasks;
using Amazon.CloudWatchLogs.Model;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

/// <summary>
/// Cloud watch query service interface.
/// </summary>
public interface ICloudWatchQueryClient
{
    /// <summary>
    /// Cloud watch query.
    /// </summary>
    /// <param name="query">The query DTO.</param>
    /// <returns>The query output.</returns>
    Task<List<List<ResultField>>> QueryCloudWatch(CloudWatchQuery query);
}
