// <copyright file="EmailSubjectExclusionOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Configuration;

using System.Collections.Generic;
using Cps.Fct.EasShared.Core.Text;

/// <summary>
/// Email subject exclusions.
/// </summary>
public class EmailSubjectExclusionOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectExclusionOptions"/> class.
    /// </summary>
    public EmailSubjectExclusionOptions()
        : this(new List<TextMatchInfo>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSubjectExclusionOptions"/> class.
    /// </summary>
    /// <param name="emailSubjectExclusionList">The email subject exclusion list.</param>
    public EmailSubjectExclusionOptions(IList<TextMatchInfo> emailSubjectExclusionList)
    {
        this.EmailSubjectExclusionList = emailSubjectExclusionList;
    }

    /// <summary>
    /// Gets the email subject exclusion list.
    /// </summary>
    public IList<TextMatchInfo> EmailSubjectExclusionList { get; init; }
}
