// <copyright file="MailboxOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Text.Json;

/// <summary>
/// The options used to identify shared mailboxes.
/// </summary>
public class MailboxOptions
{
    private readonly ImmutableDictionary<string, IdentifiableMailboxSettings> mailboxIdentifiers;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxOptions"/> class using S3 storage.
    /// </summary>
    /// <param name="clientFactory">The S3 client factory.</param>
    public MailboxOptions(IAmazonS3ClientFactory clientFactory)
    {
        Requires.NotNull(clientFactory);

        this.MailboxList = CreateMailboxListEmbedded();
        this.mailboxIdentifiers = CreateMailboxIdentifiers(this.MailboxList);
    }

    /// <summary>
    /// Gets the list of mailboxes.
    /// </summary>
    public IList<IdentifiableMailboxSettings> MailboxList { get; init; }

    /// <summary>
    /// Derives the metric report name from the specified mailbox settings.
    /// </summary>
    /// <param name="item">The mailbox settings.</param>
    /// <returns>The metric report name from the specified mailbox settings.</returns>
    public static string DeriveMetricReportName(IdentifiableMailboxSettings item)
    {
        Requires.NotNull(item);
        Requires.NotNullOrWhiteSpace(item.Name);

        string result = item.Name;
        string token = "CPS - ";
        if (result.StartsWith(token, StringComparison.OrdinalIgnoreCase))
        {
            result = result[token.Length..];
        }

        result = result.Replace("/", " and ");
        result = result.Replace("  ", " ");
        return result;
    }

    /// <summary>
    /// Gets the mailbox settings for the specified mailbox.
    /// </summary>
    /// <param name="id">The mailbox ID; either an email address or an AAD object GUID.</param>
    /// <returns>the mailbox settings for the specified mailbox.</returns>
    public IMailboxSettings GetMailboxSettings(string id)
    {
        IMailboxSettings result = new MailboxSettings();
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (this.mailboxIdentifiers.TryGetValue(id, out IdentifiableMailboxSettings? mailboxInfo))
            {
                result = mailboxInfo;
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified mailbox is a supported shared mailbox.
    /// </summary>
    /// <param name="id">The mailbox ID; either an email address or an AAD object GUID.</param>
    /// <returns>A value indicating whether the specified mailbox is a supported shared mailbox.</returns>
    public bool IsSharedMailbox(string id)
    {
        bool result = false;
        if (!string.IsNullOrWhiteSpace(id))
        {
            result = this.mailboxIdentifiers.ContainsKey(id);
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified mailbox requires Proceeds of Crime URN format support.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires Proceeds of Crime URN format support; otherwise, <c>false</c>.</returns>
    public bool RequiresProceedsOfCrimeUrnSupport(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        bool result = setting.ProceedsOfCrimeUrnSupport != null &&
            setting.ProceedsOfCrimeUrnSupport.Supported;
        return result;
    }

    /// <summary>
    /// Determines whether to accept other area URN(s). Only applicable to mailboxes that support Proceeds of Crime URN formats.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> to accept other area URN(s); otherwise <c>false</c>.</returns>
    public bool AcceptNonProceedsOfCrimeUrn(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        bool result = setting.ProceedsOfCrimeUrnSupport != null &&
            setting.ProceedsOfCrimeUrnSupport.Supported &&
            setting.ProceedsOfCrimeUrnSupport.AcceptOtherAreaUrns.HasValue &&
            setting.ProceedsOfCrimeUrnSupport.AcceptOtherAreaUrns.Value;
        return result;
    }

    /// <summary>
    /// Determines whether the mailbox requires emails to be in MSG file format.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires emails to be in MSG file format; otherwise, <c>false</c>.</returns>
    public virtual bool RequiresEmailAsMsgFile(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        bool result = setting.EmailAsMsgFileSupport != null &&
            setting.EmailAsMsgFileSupport.Supported;
        return result;
    }

    /// <summary>
    /// Determines whether the mailbox requires emails to be in MSG file format with attachments.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires emails to be in MSG file format with attachments; otherwise, <c>false</c>.</returns>
    public bool RequiresEmailAsMsgFileIncludeAttachments(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        bool result = setting.EmailAsMsgFileSupport != null &&
            setting.EmailAsMsgFileSupport.Supported &&
            setting.EmailAsMsgFileSupport.IncludeAttachments.HasValue &&
            setting.EmailAsMsgFileSupport.IncludeAttachments.Value;
        return result;
    }

    /// <summary>
    /// Determines whether the mailbox requires attachments to be ignored.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox requires emails to be in MSG file format with attachments; otherwise, <c>false</c>.</returns>
    public bool RequiresIgnoreAttachments(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        return setting.IgnoreAttachments;
    }

    /// <summary>
    /// Determines whether the mailbox should use the secure queue for processing emails.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to lookup.</param>
    /// <returns>Returns <c>true</c> if the mailbox is secure; otherwise, <c>false</c>.</returns>
    public bool RequiresSecureMailbox(string mailboxId)
    {
        IMailboxSettings setting = this.GetMailboxSettings(mailboxId);
        return setting.IsSecureMailbox;
    }

    /// <summary>
    /// Creates the mailbox list by dynamically loading all configuration files from S3 bucket.
    /// </summary>
    /// <param name="clientFactory">The S3 client factory.</param>
    /// <returns>The list of mailbox settings loaded from S3.</returns>
    private static IList<IdentifiableMailboxSettings> CreateMailboxList(IAmazonS3ClientFactory clientFactory)
    {
        Requires.NotNull(clientFactory);

        List<IdentifiableMailboxSettings> result = new();
        IStorageClient configClient = clientFactory.GetMailboxConfigurationClient();

        try
        {
            // Dynamically list ALL configuration files in the S3 bucket
            IEnumerable<string> configFiles = configClient.ListObjectKeysAsync(prefix: "appsettings.")
                .GetAwaiter()
                .GetResult();

            // Filter to only .json files
            configFiles = configFiles.Where(key => key.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            foreach (string key in configFiles)
            {
                try
                {
                    MailboxListSettings? settings = configClient.GetObjectAsync<MailboxListSettings>(key)
                        .GetAwaiter()
                        .GetResult();

                    if (settings is not null && settings.MailboxList is not null && settings.MailboxList.Any())
                    {
                        result.AddRange(settings.MailboxList);
                        System.Diagnostics.Debug.WriteLine($"Loaded {settings.MailboxList.Count} mailbox(es) from {key}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Could not load {key} from S3: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error listing objects from S3: {ex.Message}. Falling back to legacy embedded resources.");
            throw;
        }

        LoadPrivateSettings(result);
        return result;
    }

    /// <summary>
    /// Creates the mailbox list by loading configuration files from embedded resources.
    /// This method is kept for rollback purposes if S3 loading fails.
    /// </summary>
    /// <returns>The list of mailbox settings loaded from embedded resources.</returns>
    private static IList<IdentifiableMailboxSettings> CreateMailboxListEmbedded()
    {
        const string ResourceNamespace = "Cps.Fct.EasShared.Mailbox.Settings.Configuration";
        string[] environments =
        {
            "Default",
            "Production.London",
            "Production.North West",
            "Production.West Midlands",
            "Production.Proceeds of Crime",
            "Production.East Midlands",
            "Production.Cymru-Wales",
            "Production.Wessex",
            "Production.Extradition",
            "Production.Yorkshire and Humberside",
            "Production.Mersey Cheshire",
            "Production.CPS Direct",
            "Production.South West",
            "Production.South East",
            "Production.East of England",
            "Production.London North",
            "Production.London South",
            "Production.North East",
            "Production.Thames and Chiltern",
            "Production.ARU",
            "Production.VTP",
            "Development",
            "Staging",
            "UnitTest",
        };

        List<IdentifiableMailboxSettings> result = new();
        foreach (string environment in environments)
        {
            string json = EmbeddedResourceUtility.LoadEmbeddedResource($"{ResourceNamespace}.appsettings.{environment}.json", typeof(MailboxOptions));
            MailboxListSettings? settings = JsonSerializer.Deserialize<MailboxListSettings>(json: json, options: ApplicationOptions.ApplicationSerializerOptions);
            if (settings is not null && settings.MailboxList is not null && settings.MailboxList.Any())
            {
                result.AddRange(settings.MailboxList);
            }
        }

        LoadPrivateSettings(result);
        return result;
    }

    /// <summary>
    /// Loads private settings from the local file system.
    /// </summary>
    /// <param name="result">The list to add private settings to.</param>
    private static void LoadPrivateSettings(List<IdentifiableMailboxSettings> result)
    {
        DirectoryInfo dirInfo = new(Path.Combine(Assembly.GetExecutingAssembly().Location, "../"));
        string filename = Path.Combine(dirInfo.FullName, "privatesettings.json");
        if (File.Exists(filename))
        {
            string json = File.ReadAllText(filename, Encoding.UTF8);
            MailboxListSettings? settings = JsonSerializer.Deserialize<MailboxListSettings>(json: json, options: ApplicationOptions.ApplicationSerializerOptions);
            if (settings is not null && settings.MailboxList is not null && settings.MailboxList.Any())
            {
                result.AddRange(settings.MailboxList);
            }
        }
    }

    /// <summary>
    /// Creates a dictionary of mailbox identifiers for fast lookup.
    /// </summary>
    /// <param name="mailboxList">The list of mailbox settings.</param>
    /// <returns>An immutable dictionary mapping IDs and email addresses to mailbox settings.</returns>
    private static ImmutableDictionary<string, IdentifiableMailboxSettings> CreateMailboxIdentifiers(IList<IdentifiableMailboxSettings> mailboxList)
    {
        Dictionary<string, IdentifiableMailboxSettings> identifiers = new(StringComparer.OrdinalIgnoreCase);
        foreach (IdentifiableMailboxSettings mailbox in mailboxList)
        {
            if (!identifiers.TryAdd(mailbox.MailboxId, mailbox))
            {
                throw new InvalidOperationException($"Duplicate mailbox identifier '{mailbox.MailboxId}' found.");
            }

            if (!identifiers.TryAdd(mailbox.EmailAddress, mailbox))
            {
                throw new InvalidOperationException($"Duplicate mailbox identifier '{mailbox.EmailAddress}' found.");
            }
        }

        return identifiers.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private sealed record MailboxListSettings(List<IdentifiableMailboxSettings> MailboxList);
}