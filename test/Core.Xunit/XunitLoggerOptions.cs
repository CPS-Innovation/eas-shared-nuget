// <copyright file="XunitLoggerOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Xunit;

using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>
/// The xUnit logger options.
/// </summary>
public class XunitLoggerOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    internal const string DefaultSectionName = "XunitLogger";

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLoggerOptions"/> class.
    /// </summary>
    public XunitLoggerOptions()
        : this(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XunitLoggerOptions"/> class.
    /// </summary>
    /// <param name="logLevel">The default log level.</param>
    [JsonConstructor]
    public XunitLoggerOptions(LogLevel logLevel)
    {
        this.Default = logLevel;
    }

    /// <summary>
    /// Gets the default log level.
    /// </summary>
    public LogLevel Default { get; init; }
}
