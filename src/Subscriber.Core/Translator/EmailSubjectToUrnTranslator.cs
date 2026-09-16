// <copyright file="EmailSubjectToUrnTranslator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>


using Cps.Fct.EasShared.Core.Interfaces;
using Cps.Fct.EasShared.Mailbox.Settings.Configuration;
using Cps.Fct.EasShared.Subscriber.Core.Translator;

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;
/// <summary>
/// Translates an email subject to a URN.
/// </summary>
public partial class EmailSubjectToUrnTranslator
    : IEntityTranslator<EmailSubjectInfo, string?>
{
    private readonly EmailSubjectToPocUrnTranslator pocUrnTranslator;
    private readonly EmailSubjectToStandardUrnTranslator standardUrnTranslator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectToUrnTranslator"/> class.
    /// </summary>
    /// <param name="urnSupportLookup">The URN support lookup.</param>
    public EmailSubjectToUrnTranslator(
        IUrnSupportLookup urnSupportLookup)
    {
        Requires.NotNull(urnSupportLookup);

        this.UrnSupportLookup = urnSupportLookup;
        this.pocUrnTranslator = new EmailSubjectToPocUrnTranslator(urnSupportLookup);
        this.standardUrnTranslator = new EmailSubjectToStandardUrnTranslator(urnSupportLookup);
    }

    /// <summary>
    /// Gets the URN support lookup.
    /// </summary>
    public IUrnSupportLookup UrnSupportLookup { get; }

    /// <inheritdoc/>
    public string? Translate(EmailSubjectInfo source)
    {
        Requires.NotNull(source);
        Requires.NotNullOrWhiteSpace(source.MailboxId);

        string? result = null;
        if (source.Subject is not null)
        {
            bool requiresPocUrnSupport = this.UrnSupportLookup.RequiresProceedsOfCrimeUrnSupport(source.MailboxId);
            if (requiresPocUrnSupport)
            {
                result = this.pocUrnTranslator.Translate(source);
            }
            else
            {
                result = this.standardUrnTranslator.Translate(source);
            }
        }

        return result;
    }
}
