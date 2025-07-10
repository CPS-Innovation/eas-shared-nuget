// <copyright file="GraphApiConnectionOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.ServiceClient.GraphApi.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// The settings required to connect to the Graph API.
/// </summary>
public class GraphApiConnectionOptions
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = "GraphApi";

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiConnectionOptions"/> class.
    /// </summary>
    public GraphApiConnectionOptions()
        : this(string.Empty, string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphApiConnectionOptions"/> class.
    /// </summary>
    /// <param name="clientId">
    /// The client ID (also known as app ID) of the application as registered in the Graph API tenant.
    /// </param>
    /// <param name="clientSecret">
    /// The secret string previously shared with AAD at application registration to prove the identity of the application (the client) requesting the tokens.
    /// </param>
    /// <param name="tenantId">The tenant ID of the Graph API AAD.</param>
    [JsonConstructor]
    public GraphApiConnectionOptions(
        string clientId,
        string clientSecret,
        string tenantId)
    {
        this.ClientId = clientId;
        this.ClientSecret = clientSecret;
        this.TenantId = tenantId;
    }

    /// <summary>
    /// Gets the client ID (also known as app ID) of the application as registered in the Graph API AAD.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; init; }

    /// <summary>
    /// Gets the secret string previously shared with AAD at application registration to prove the identity
    /// of the application (the client) requesting the tokens.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientSecret { get; init; }

    /// <summary>
    /// Gets the tenant ID of the Graph API AAD.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string TenantId { get; init; }
}
