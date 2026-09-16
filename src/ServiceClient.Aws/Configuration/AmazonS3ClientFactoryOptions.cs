// <copyright file="AmazonS3ClientFactoryOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>


using System.ComponentModel.DataAnnotations;

namespace Cps.Fct.EasShared.ServiceClient.Aws.Configuration;

/// <summary>
/// The AWS API options for an S3 client factory.
/// </summary>
public class AmazonS3ClientFactoryOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "AmazonS3ClientFactory";

    /// <summary>
    /// Gets the email storage container name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string EmailStorageContainerName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the mailbox configuration container name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string MailboxConfigurationContainerName { get; init; } = string.Empty;
}
