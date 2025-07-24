// <copyright file="CloudWatchQueryClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.EasShared.ServiceClient.Aws.CloudWatch;

using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Cps.Fct.EasShared.Core.Diagnostics;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Cloud watch query service for connecting to aws.
/// </summary>
public class CloudWatchQueryClient : ICloudWatchQueryClient
{
    private readonly CloudWatchClientOptions clientOptions;
    private readonly ILogger<CloudWatchQueryClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchQueryClient"/> class.
    /// Cloud watch query service for connecting to aws.
    /// </summary>
    /// <param name="clientOptions">The S3 API options.</param>
    /// <param name="logger">Logger.</param>
    public CloudWatchQueryClient(
        IOptions<CloudWatchClientOptions> clientOptions,
        ILogger<CloudWatchQueryClient> logger)
    {
        Requires.NotNull(clientOptions);
        Requires.NotNull(clientOptions.Value);
        Requires.NotNull(logger);

        this.clientOptions = clientOptions.Value;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<List<ResultField>>> QueryCloudWatch(CloudWatchQuery query)
    {
        Requires.NotNull(query);

        using AmazonCloudWatchLogsClient client = this.CreateCloudWatchClient();

        StartQueryResponse queryRequest = await client.StartQueryAsync(
            new StartQueryRequest
            {
                LogGroupName = query.LogGroup,
                StartTime = query.StartTime.ToUnixTimeSeconds(),
                EndTime = query.EndTime.ToUnixTimeSeconds(),
                QueryString = query.Query,
            }).ConfigureAwait(false);

        var request = new GetQueryResultsRequest
        {
            QueryId = queryRequest.QueryId,
        };

        GetQueryResultsResponse? results = null;
        do
        {
            if (results is not null)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            results = await client.GetQueryResultsAsync(request).ConfigureAwait(false);
        }
        while (results.Status == QueryStatus.Running ||
                 results.Status == QueryStatus.Scheduled);

        if (results.Status != QueryStatus.Complete)
        {
            string errorMsg = $"Query {query.QueryName} against log group {query.LogGroup} failed with status {results.Status}. The failed query is: {query.Query}";
            throw new InvalidOperationException(errorMsg);
        }

        return results.Results;
    }

    private AmazonCloudWatchLogsClient CreateCloudWatchClient()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(this.clientOptions.AccessKey) && !string.IsNullOrWhiteSpace(this.clientOptions.SecretKey))
            {
                BasicAWSCredentials credentials = new(
                    accessKey: this.clientOptions.AccessKey,
                    secretKey: this.clientOptions.SecretKey);
                return new AmazonCloudWatchLogsClient(credentials: credentials, this.clientOptions.RegionEndpoint);
            }

            AmazonCloudWatchLogsConfig config = new()
            {
                RegionEndpoint = this.clientOptions.RegionEndpoint,
            };

            if (!string.IsNullOrWhiteSpace(this.clientOptions.ServiceEndpoint))
            {
                config.ServiceURL = this.clientOptions.ServiceEndpoint;
            }

            return new AmazonCloudWatchLogsClient(config: config);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, DiagnosticsUtility.Error + "Amazon Cloud Watch client creation failed with access key '{AccessKey}' and service endpoint '{ServiceEndpoint}'", this.clientOptions.AccessKey, this.clientOptions.ServiceEndpoint);
            throw;
        }
    }
}
