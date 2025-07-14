// <copyright file="EmailAsMsgFileSupport.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Mailbox.Settings.Configuration;

/// <summary>
/// Determines whether email as MSG file formatting is supported.
/// </summary>
/// <param name="Supported">Indicates whether email as MSG file formatting is supported.</param>
/// <param name="IncludeAttachments">Indicates whether to include attachments. Only applicable if <c>Supported == true</c>.</param>
public record EmailAsMsgFileSupport(
    bool Supported,
    bool? IncludeAttachments)
{
}
