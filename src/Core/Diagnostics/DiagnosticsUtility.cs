// <copyright file="DiagnosticsUtility.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Diagnostics;

using Cps.Fct.EasShared.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Diagnostics utility class.
/// </summary>
public static class DiagnosticsUtility
{
    /// <summary>
    /// The warning prefix.
    /// </summary>
    public const string Warning = $"{nameof(Warning)}: ";

    /// <summary>
    /// The error prefix.
    /// </summary>
    public const string Error = $"{nameof(Error)}: ";

    /// <summary>
    /// Logs the application information.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="environmentName">The environment name.</param>
    public static void LogApplicationInfo(
        IServiceProvider serviceProvider,
        ILogger logger,
        string environmentName)
    {
        IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();
        logger.LogInformation(
            "Application: {Application} - Version: {AssemblyFileVersion} - Environment: {EnvironmentName} - Log level: {LogLevel}",
            ApplicationOptions.GetApplicationName(serviceProvider),
            ApplicationOptions.GetAssemblyFileVersion(typeof(DiagnosticsUtility)),
            environmentName,
            config["Logging:LogLevel:Default"]);
    }

    /// <summary>
    /// Derives a log message for an email attachment.
    /// </summary>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="size">The size of the attachment.</param>
    /// <param name="contentType">The content type of the attachment.</param>
    /// <returns>Returns a log message for an email attachment.</returns>
    public static string ToAttachmentText(string name, long size, string contentType)
    {
        return $"'{name}' ({size:N0}) Content type - '{contentType}'";
    }

    /// <summary>
    /// Derives a log message for an email attachment.
    /// </summary>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="size">The size of the attachment.</param>
    /// <param name="contentType">The content type of the attachment.</param>
    /// <param name="oDataType">The OData type of the attachment.</param>
    /// <returns>Returns a log message for an email attachment.</returns>
    public static string ToAttachmentText(string name, long size, string contentType, string oDataType)
    {
        return $"'{name}' ({size:N0}) OData type - '{oDataType}' Content type - '{contentType}'";
    }
}
