// <copyright file="ApplicationOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Core.Configuration;

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Application options.
/// </summary>
public class ApplicationOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "Application";

    /// <summary>
    /// The default <see cref="JsonSerializerOptions"/> for the application.
    /// </summary>
    public static readonly JsonSerializerOptions ApplicationSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
            {
                new JsonStringEnumConverter(),
            },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationOptions"/> class.
    /// </summary>
    public ApplicationOptions()
        : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationOptions"/> class.
    /// </summary>
    /// <param name="displayName">The display name of the application.</param>
    /// <param name="name">The name of the application.</param>
    [JsonConstructor]
    public ApplicationOptions(
        string displayName,
        string name)
    {
        this.Name = name;
        this.DisplayName = displayName;
    }

    /// <summary>
    /// Gets the display name of the application.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Name { get; init; }

    /// <summary>
    /// Gets the assembly version.
    /// </summary>
    /// <param name="type">A type in the assembly used to get the assembly.</param>
    /// <returns>The assembly version.</returns>
    public static string GetAssemblyFileVersion(Type type)
    {
        Requires.NotNull(type);

        string? version = null;
        var assembly = Assembly.GetAssembly(type);
        if (assembly != null)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (versionInfo.FileVersion != null)
            {
                version = versionInfo.FileVersion[0..^2];
            }
        }

        return version ?? "Unknown";
    }

    /// <summary>
    /// Gets the application name using the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The application name.</returns>
    public static string GetApplicationName(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider);

        IOptions<ApplicationOptions> options = serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>();
        return options.Value.Name;
    }
}
