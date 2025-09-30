// <copyright file="AttachmentSizeSetting.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Attachment Size Setting.
/// </summary>
public class AttachmentSettingOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentSettingOptions"/> class.
    /// </summary>
    public AttachmentSettingOptions()
        : this(20971520)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentSettingOptions"/> class.
    /// </summary>
    /// <param name="AttachmentSize">The attachment size allowed.</param>
    [JsonConstructor]
    public AttachmentSettingOptions(int attachmentSize)
    {
        this.AttachmentSize = attachmentSize;
    }

    /// <summary>
    /// Gets the retry count.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int AttachmentSize { get; init; }
}
