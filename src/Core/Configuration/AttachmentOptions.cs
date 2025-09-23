// <copyright file="AttachmentSizeSetting.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Attachment Size Setting.
/// </summary>
public class AttachmentOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentOptions"/> class.
    /// </summary>
    public AttachmentOptions()
        : this(20480)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentOptions"/> class.
    /// </summary>
    /// <param name="AttachmentSize">The attachment size allowed.</param>
    [JsonConstructor]
    public AttachmentOptions(int attachmentSize)
    {
        this.AttachmentSize = attachmentSize;
    }

    /// <summary>
    /// Gets the retry count.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int AttachmentSize { get; init; }
}
