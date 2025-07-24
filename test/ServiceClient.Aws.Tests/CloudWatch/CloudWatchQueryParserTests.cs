// <copyright file="CloudWatchQueryParserTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.Eas.ServiceClient.Aws.Tests.CloudWatch;

using System.Text.Json;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws.CloudWatch;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

/// <summary>
/// Cloud watch query tests.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class CloudWatchQueryParserTests
{
    /// <summary>
    /// Generate query string expect success.
    /// </summary>
    [Fact]
    public void GenerateQueryStringExpectSuccess()
    {
        string finalQuery = "| filter @logStream like /.(?i)(eas-)/ and @message like /.(?i)(a@b.com)/";
        var cloudWatchQueryParser = new CloudWatchQueryParser();
        string cloudWatchQuery = EmbeddedResourceUtility.LoadEmbeddedResource("Cps.Fct.EasShared.ServiceClient.Aws.Tests.Embedded.CloudWatchQueryConfig.json", typeof(CloudWatchQueryParserTests));
        CloudWatchQueryInfo query = JsonSerializer.Deserialize<CloudWatchQueryInfo>(json: cloudWatchQuery, options: ApplicationOptions.ApplicationSerializerOptions)!;
        query.Should().NotBeNull();
        query.QueryName.Should().NotBeNullOrWhiteSpace();

        string getCloudWatchQuery = cloudWatchQueryParser.GetCloudWatchQuery("| filter @logStream like /.(?i)(eas-)/ {{mailboxFilter}}", query.Parameters);
        finalQuery.Should().Be(getCloudWatchQuery);
    }
}
