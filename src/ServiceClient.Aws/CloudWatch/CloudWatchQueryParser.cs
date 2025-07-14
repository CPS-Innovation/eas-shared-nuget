// <copyright file="CloudWatchQueryParser.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.EasShared.ServiceClient.Aws.CloudWatch;

/// <summary>
/// Cloud watch query parser.
/// </summary>
public class CloudWatchQueryParser : ICloudWatchQueryParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudWatchQueryParser"/> class.
    /// Cloud watch query parser.
    /// </summary>
    public CloudWatchQueryParser()
    {
    }

    /// <inheritdoc/>
    public string GetCloudWatchQuery(string template, Dictionary<string, string> parameters)
    {
        Requires.NotNullOrWhiteSpace(template);
        Requires.NotNull(parameters);

        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            template = template.Replace($"{{{{{parameter.Key}}}}}", parameter.Value);
        }

        return template;
    }
}
