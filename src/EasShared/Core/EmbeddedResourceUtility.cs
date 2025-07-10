// <copyright file="EmbeddedResourceUtility.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core;

using System;
using System.Globalization;
using System.Reflection;

/// <summary>
/// The embedded resource utility.
/// </summary>
public static class EmbeddedResourceUtility
{
    /// <summary>
    /// Loads an embedded resource as a string from an assembly.
    /// </summary>
    /// <param name="resourceName">
    /// The name of the resource.
    /// </param>
    /// <param name="typeInResourceAssembly">
    /// A type that can be used to identify what assembly the resource can be found in.
    /// </param>
    /// <returns>
    /// The embedded resource as a string.
    /// </returns>
    public static string LoadEmbeddedResource(string resourceName, Type typeInResourceAssembly)
    {
        Requires.NotNullOrWhiteSpace(resourceName);
        Requires.NotNull(typeInResourceAssembly);

        var resourceAssembly = Assembly.GetAssembly(typeInResourceAssembly);
        return LoadEmbeddedResource(resourceName, resourceAssembly!);
    }

    /// <summary>
    /// Loads an embedded resource as a string from an assembly.
    /// </summary>
    /// <param name="resourceName">
    /// The name of the resource.
    /// </param>
    /// <param name="resourceAssembly">
    /// The Assembly where the resource is contained.
    /// </param>
    /// <returns>
    /// The embedded resource as a string.
    /// </returns>
    public static string LoadEmbeddedResource(string resourceName, Assembly resourceAssembly)
    {
        Requires.NotNullOrWhiteSpace(resourceName);
        Requires.NotNull(resourceAssembly);

        Stream? stream = null;
        try
        {
            stream = resourceAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new ArgumentException($"Embedded resource '{resourceName}' was not found in assembly '{resourceAssembly.FullName}'.");
            }

            using StreamReader reader = new(stream);
            stream = null;
            return reader.ReadToEnd().ToString(CultureInfo.CurrentCulture);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    /// <summary>
    /// Loads an embedded resource as a byte[] from an assembly.
    /// </summary>
    /// <param name="resourceName">
    /// The name of the resource.
    /// </param>
    /// <param name="typeInResourceAssembly">
    /// A Type that can be used to identify what assembly the resource can be found in.
    /// </param>
    /// <returns>
    /// The embedded resource as a byte[].
    /// </returns>
    public static byte[] LoadEmbeddedResourceAsBinary(string resourceName, Type typeInResourceAssembly)
    {
        Requires.NotNullOrWhiteSpace(resourceName);
        Requires.NotNull(typeInResourceAssembly);

        var resourceAssembly = Assembly.GetAssembly(typeInResourceAssembly);
        return LoadEmbeddedResourceAsBinary(resourceName, resourceAssembly!);
    }

    /// <summary>
    /// Loads an embedded resource as a byte[] from an Assembly.
    /// </summary>
    /// <param name="resourceName">
    /// The name of the resource.
    /// </param>
    /// <param name="resourceAssembly">
    /// The Assembly where the resource is contained.
    /// </param>
    /// <returns>
    /// The embedded resource as a byte[].
    /// </returns>
    public static byte[] LoadEmbeddedResourceAsBinary(string resourceName, Assembly resourceAssembly)
    {
        Requires.NotNullOrWhiteSpace(resourceName);
        Requires.NotNull(resourceAssembly);

        Stream? stream = null;
        try
        {
            stream = resourceAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new ArgumentException($"Embedded resource '{resourceName}' was not found in assembly '{resourceAssembly.FullName}'.");
            }

            byte[] result = new byte[stream.Length];
            stream.Read(result, 0, result.Length);
            return result;
        }
        finally
        {
            stream?.Dispose();
        }
    }
}
