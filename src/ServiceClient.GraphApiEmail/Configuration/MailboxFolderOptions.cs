// <copyright file="MailboxFolderOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail.Configuration;

using System.Globalization;
using System.Text.Json.Serialization;

/// <summary>
/// The mailbox folder options.
/// </summary>
public class MailboxFolderOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "MailboxFolders";

    /// <summary>
    /// The default exceptions folder name.
    /// </summary>
    public const string DefaultExceptionsFolderName = "CMS.Exceptions";

    /// <summary>
    /// The default excluded folder name.
    /// </summary>
    public const string DefaultExcludedFolderName = "CMS.Excluded";

    /// <summary>
    /// The default inbox folder name.
    /// </summary>
    public const string DefaultInboxFolderName = "CMS.Inbox";

    /// <summary>
    /// The default processed folder name.
    /// </summary>
    public const string DefaultProcessedFolderName = "CMS.Processed";

    /// <summary>
    /// The default invalid URN folder name.
    /// </summary>
    public const string DefaultInvalidUrnFolderName = "CMS.InvalidURN";

    /// <summary>
    /// The default processing folder name.
    /// </summary>
    public const string DefaultProcessingFolderName = "CMS.Processing";

    /// <summary>
    /// The default urgent folder name.
    /// </summary>
    public const string DefaultUrgentFolderName = "CMS.Urgent";

    private static readonly TimeSpan DefaultFolderExpiry = TimeSpan.Parse("30.00:00:00", CultureInfo.InvariantCulture);

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxFolderOptions"/> class.
    /// </summary>
    public MailboxFolderOptions()
        : this(
            exceptionsFolderName: DefaultExceptionsFolderName,
            excludedFolderName: DefaultExcludedFolderName,
            inboxFolderName: DefaultInboxFolderName,
            processedFolderName: DefaultProcessedFolderName,
            invalidUrnFolderName: DefaultInvalidUrnFolderName,
            processingFolderName: DefaultProcessingFolderName,
            urgentFolderName: DefaultUrgentFolderName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxFolderOptions"/> class.
    /// </summary>
    /// <param name="exceptionsFolderName">The exceptions folder name.</param>
    /// <param name="excludedFolderName">The excluded folder name.</param>
    /// <param name="inboxFolderName">The inbox folder name.</param>
    /// <param name="processedFolderName">The processed folder name.</param>
    /// <param name="invalidUrnFolderName">The invalid URN folder name.</param>
    /// <param name="processingFolderName">The processing folder name.</param>
    /// <param name="urgentFolderName">The urgent folder name.</param>
    [JsonConstructor]
    public MailboxFolderOptions(
        string exceptionsFolderName,
        string excludedFolderName,
        string inboxFolderName,
        string processedFolderName,
        string invalidUrnFolderName,
        string processingFolderName,
        string urgentFolderName)
    {
        this.ExceptionsFolderName = exceptionsFolderName;
        this.ExcludedFolderName = excludedFolderName;
        this.InboxFolderName = inboxFolderName;
        this.ProcessedFolderName = processedFolderName;
        this.InvalidUrnFolderName = invalidUrnFolderName;
        this.ProcessingFolderName = processingFolderName;
        this.UrgentFolderName = urgentFolderName;

        this.ExcludedFolderExpiry = DefaultFolderExpiry;
        this.ProcessedFolderExpiry = DefaultFolderExpiry;
        this.InvalidUrnFolderExpiry = DefaultFolderExpiry;
        this.UrgentFolderExpiry = DefaultFolderExpiry;
    }

    /// <summary>
    /// Gets the exceptions folder name.
    /// </summary>
    public string ExceptionsFolderName { get; init; }

    /// <summary>
    /// Gets the excluded folder name.
    /// </summary>
    public string ExcludedFolderName { get; init; }

    /// <summary>
    /// Gets the expiry interval for email in the excluded folder.
    /// Emails in the folder that are there for longer than this period will be deleted.
    /// </summary>
    public TimeSpan? ExcludedFolderExpiry { get; init; }

    /// <summary>
    /// Gets the inbox folder name.
    /// </summary>
    public string InboxFolderName { get; init; }

    /// <summary>
    /// Gets the processed folder name.
    /// </summary>
    public string ProcessedFolderName { get; init; }

    /// <summary>
    /// Gets the expiry interval for email in the processed folder.
    /// Emails in the folder that are there for longer than this period will be deleted.
    /// </summary>
    public TimeSpan? ProcessedFolderExpiry { get; init; }

    /// <summary>
    /// Gets the invalid URN folder name.
    /// </summary>
    public string InvalidUrnFolderName { get; init; }

    /// <summary>
    /// Gets the expiry interval for email in the invalid URN folder.
    /// Emails in the folder that are there for longer than this period will be deleted.
    /// </summary>
    public TimeSpan? InvalidUrnFolderExpiry { get; init; }

    /// <summary>
    /// Gets the processing folder name.
    /// </summary>
    public string ProcessingFolderName { get; init; }

    /// <summary>
    /// Gets the urgent folder name.
    /// </summary>
    public string UrgentFolderName { get; init; }

    /// <summary>
    /// Gets the expiry interval for email in the urgent folder.
    /// Emails in the folder that are there for longer than this period will be deleted.
    /// </summary>
    public TimeSpan? UrgentFolderExpiry { get; init; }

    /// <summary>
    /// Gets all the folder names.
    /// </summary>
    /// <returns>All the folder names.</returns>
    public IEnumerable<string> GetAllFolderNames()
    {
        yield return this.ExceptionsFolderName;
        yield return this.ExcludedFolderName;
        yield return this.InboxFolderName;
        yield return this.ProcessedFolderName;
        yield return this.InvalidUrnFolderName;
        yield return this.ProcessingFolderName;
        yield return this.UrgentFolderName;
    }
}
