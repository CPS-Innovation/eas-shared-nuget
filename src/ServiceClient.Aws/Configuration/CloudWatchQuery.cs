// <copyright file="CloudWatchQuery.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Cloud Watch query.
/// </summary>
public class CloudWatchQuery
    : CloudWatchQueryInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchQuery"/> class.
    /// </summary>
    public CloudWatchQuery()
        : this(
            logGroup: string.Empty,
            queryName: string.Empty,
            parameters: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            parsedQuery: string.Empty,
            startTime: new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)),
            endTime: DateTimeOffset.UtcNow)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchQuery"/> class.
    /// </summary>
    /// <param name="logGroup">The log group to run against.</param>
    /// <param name="queryName">The query name.</param>
    /// <param name="parameters">The dictionary of parameters.</param>
    /// <param name="parsedQuery">The parsed query.</param>
    /// <param name="startTime">The start time of the query.</param>
    /// <param name="endTime">The end time of the query.</param>
    public CloudWatchQuery(
        string logGroup,
        string queryName,
        Dictionary<string, string> parameters,
        string parsedQuery,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
            : base()
    {
        Requires.NotNull(logGroup);
        Requires.NotNull(queryName);
        Requires.NotNull(parameters);
        Requires.NotNull(parsedQuery);

        this.QueryName = queryName;
        this.Parameters = parameters;
        this.LogGroup = logGroup;
        this.Query = parsedQuery;
        this.StartTime = startTime;
        this.EndTime = endTime;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchQuery"/> class.
    /// </summary>
    /// <param name="queryInfo">The query base information.</param>
    /// <param name="logGroup">The log group to run against.</param>
    /// <param name="query">The parsed query.</param>
    /// <param name="startTime">The start time of the query.</param>
    /// <param name="endTime">The end time of the query.</param>
    public CloudWatchQuery(
        CloudWatchQueryInfo queryInfo,
        string logGroup,
        string query,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
            : this(
                logGroup: logGroup,
                queryName: queryInfo.QueryName,
                parameters: queryInfo.Parameters,
                parsedQuery: query,
                startTime: startTime,
                endTime: endTime)
    {
    }

    /// <summary>
    /// Gets the log group to run against.
    /// </summary>
    public string LogGroup { get; init; }

    /// <summary>
    /// Gets the parsed query.
    /// </summary>
    public string Query { get; init; }

    /// <summary>
    /// Gets the start time of the query.
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Gets the end time of the query.
    /// </summary>
    public DateTimeOffset EndTime { get; init; }
}
