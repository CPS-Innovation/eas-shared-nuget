// <copyright file="EmailMessageBodyTranslator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Translator;

using System;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Cps.Fct.EasShared.Core.Interfaces;
using Microsoft.Graph.Models;

/// <summary>
/// Translates the email message body content.
/// </summary>
public partial class EmailMessageBodyTranslator
    : IEntityTranslator<Message, string>
{
    private static readonly Regex BodyPattern = CompiledBodyPattern();

    /// <inheritdoc/>
    public string Translate(Message source)
    {
        Requires.NotNull(source);

        if (source?.Body?.Content is not null)
        {
            string content = source.Body.Content;
            EmailHeaderText emailHeader = new(
                From: DeriveEmail(name: source.From!.EmailAddress!.Name!, emailAddress: source.From!.EmailAddress!.Address!),
                Date: source.SentDateTime!.Value,
                To: DeriveToEmailAddresses(source),
                ReplyTo: DeriveReplyToEmailAddresses(source),
                CcRecipients: DeriveCcRecipientsEmailAddresses(source),
                BccRecipients: DeriveBccRecipientsEmailAddresses(source),
                Subject: source.Subject!);
            MatchCollection matches = BodyPattern.Matches(content);
            if (matches.Count == 1)
            {
                Match match = matches[0];
                string front = content[..(match.Index + match.Length + 1)];
                string back = content[(match.Index + match.Length + 1)..];
                content = $"{front}{GenerateFormattedEmailHeader(emailHeader)}{back}";
            }
            else
            {
                content = $"{GenerateUnformattedEmailHeader(emailHeader)}{content}".Replace("\r", string.Empty);
                string[] parts = content.Split('\n');
                StringBuilder sb = new();
                foreach (string part in parts)
                {
                    sb.Append(HtmlEncoder.Default.Encode(part));
                    sb.Append("<br>");
                }

                content = sb.ToString();
            }

            return content;
        }

        throw new NotSupportedException("Translating an email message without any content is not supported.");
    }

    private static string GenerateFormattedEmailHeader(EmailHeaderText emailHeader)
    {
        StringBuilder sb = new();
        sb.Append("<p>");
        sb.Append(CultureInfo.InvariantCulture, $"<b>From:</b> {HtmlEncoder.Default.Encode(emailHeader.From)}");
        sb.Append("<br>");
        sb.Append(CultureInfo.InvariantCulture, $"<b>Date:</b> {emailHeader.Date:dd/MM/yyyy HH:mm:ss}");
        sb.Append("<br>");
        sb.Append(CultureInfo.InvariantCulture, $"<b>To:</b> {HtmlEncoder.Default.Encode(emailHeader.To)}");
        sb.Append("<br>");
        if (emailHeader.ReplyTo is not null)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<b>Reply to:</b> {HtmlEncoder.Default.Encode(emailHeader.ReplyTo)}");
            sb.Append("<br>");
        }

        if (emailHeader.CcRecipients is not null)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<b>CC:</b> {HtmlEncoder.Default.Encode(emailHeader.CcRecipients)}");
            sb.Append("<br>");
        }

        if (emailHeader.BccRecipients is not null)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<b>BCC:</b> {HtmlEncoder.Default.Encode(emailHeader.BccRecipients)}");
            sb.Append("<br>");
        }

        sb.Append(CultureInfo.InvariantCulture, $"<b>Subject:</b> {HtmlEncoder.Default.Encode(emailHeader.Subject)}");
        sb.Append("<br>");
        sb.Append("</p>");
        return sb.ToString();
    }

    private static string GenerateUnformattedEmailHeader(EmailHeaderText emailHeader)
    {
        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"From: {emailHeader.From}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Date: {emailHeader.Date:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"To: {emailHeader.To}");
        if (emailHeader.ReplyTo is not null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Reply to: {emailHeader.ReplyTo}");
        }

        if (emailHeader.CcRecipients is not null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CC: {emailHeader.CcRecipients}");
        }

        if (emailHeader.BccRecipients is not null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"BCC: {emailHeader.BccRecipients}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"Subject: {emailHeader.Subject}");
        return sb.ToString();
    }

    private static string DeriveEmail(string? name, string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return emailAddress;
        }

        return $"{name} <{emailAddress}>";
    }

    private static string DeriveToEmailAddresses(Message message)
    {
        return string.Join(';', DeriveEmailAddressList(message.ToRecipients));
    }

    private static string? DeriveReplyToEmailAddresses(Message message)
    {
        return DeriveEmailAddressListText(DeriveEmailAddressList(message.ReplyTo));
    }

    private static string? DeriveCcRecipientsEmailAddresses(Message message)
    {
        return DeriveEmailAddressListText(DeriveEmailAddressList(message.CcRecipients));
    }

    private static string? DeriveBccRecipientsEmailAddresses(Message message)
    {
        return DeriveEmailAddressListText(DeriveEmailAddressList(message.BccRecipients));
    }

    private static IReadOnlyCollection<string> DeriveEmailAddressList(List<Recipient>? addresses)
    {
        List<string>? result = null;
        if (addresses != null && addresses.Any())
        {
            result = new(addresses.Select(r => $"{DeriveEmail(r.EmailAddress?.Name, r.EmailAddress!.Address!)}"));
        }

        result ??= new List<string>();
        return result;
    }

    private static string? DeriveEmailAddressListText(IReadOnlyCollection<string> addresses)
    {
        string? result = null;
        if (addresses.Any())
        {
            result = string.Join(';', addresses);
        }

        return result;
    }

    [GeneratedRegex("<body[^>]*", RegexOptions.Compiled)]
    private static partial Regex CompiledBodyPattern();

    private sealed record EmailHeaderText(string From, DateTimeOffset Date, string To, string? ReplyTo, string? CcRecipients, string? BccRecipients, string Subject);
}
