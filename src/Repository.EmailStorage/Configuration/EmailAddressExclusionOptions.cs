// <copyright file="EmailAddressExclusionOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Configuration;

using System.Collections.Generic;
using Cps.Fct.EasShared.Core.Text;

/// <summary>
/// Email address exclusions.
/// </summary>
public class EmailAddressExclusionOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressExclusionOptions"/> class.
    /// </summary>
    public EmailAddressExclusionOptions()
        : this(new List<TextMatchInfo>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressExclusionOptions"/> class.
    /// </summary>
    /// <param name="emailAddressExclusionList">The email address exclusion list.</param>
    public EmailAddressExclusionOptions(IList<TextMatchInfo> emailAddressExclusionList)
    {
        this.EmailAddressExclusionList = emailAddressExclusionList;
    }

    /// <summary>
    /// Gets the email address exclusion list.
    /// </summary>
    public IList<TextMatchInfo> EmailAddressExclusionList { get; init; }
}
