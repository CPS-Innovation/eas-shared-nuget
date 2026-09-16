// <copyright file="JsonToChangeNotificationCollectionResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Translator;

using System.Text.Json;
using Cps.Fct.EasShared.Core;
using Cps.Fct.EasShared.Core.Interfaces;
using Microsoft.Graph.Models;

/// <summary>
/// Deserialises a JSON string to a <see cref="ChangeNotificationCollectionResponse"/>.
/// </summary>
public class JsonToChangeNotificationCollectionResponse :
    IEntityTranslator<string, ChangeNotificationCollectionResponse>
{
    /// <inheritdoc/>
    public ChangeNotificationCollectionResponse Translate(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            // Will result in a Bad Request response due to invalid JSON.
            return new ChangeNotificationCollectionResponse();
        }

        try
        {
            using ParseNodeFactory factory = new();
            return factory.CreateParseNode(source).GetObjectValue(ChangeNotificationCollectionResponse.CreateFromDiscriminatorValue)!;
        }
        catch (Exception e) when (e is JsonException || e is FormatException)
        {
            // Will result in a Bad Request response due to invalid JSON.
            return new ChangeNotificationCollectionResponse();
        }
    }
}
