// <copyright file="CorrespondenceIdExtendedProperty.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiEmail;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Cps.Fct.EasShared.Core;

/// <summary>
/// The extended property for a correspondence ID.
/// </summary>
/// <param name="CorrespondenceId">The correspondence ID.</param>
/// <param name="EventTime">The time that the correspondence was received.</param>
public record CorrespondenceIdExtendedProperty(
    Guid CorrespondenceId,
    DateTimeOffset EventTime)
{
    /// <summary>
    /// The extended property ID for a correspondence ID.
    /// </summary>
    public const string ExtendedPropertyId = "String {80f98438-6260-4f48-baf2-d37336ca4dd7} Name CorrespondenceId_v1";

    private const char Separator = '|';

    /// <summary>
    /// Gets the value of the extended property.
    /// </summary>
    public string ExtendedPropertyValue => $"{this.EventTime.ToUniversalTime().ToString(TimeUtility.TimeFormat, CultureInfo.InvariantCulture)}{Separator}{this.CorrespondenceId}";

    /// <summary>
    /// Tries to parse the value of the extended property.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="result">A <see cref="CorrespondenceIdExtendedProperty"/> if parsed successfully; otherwise <c>null</c>.</param>
    /// <returns>Returns <c>true</c> if the value could be parsed; otherwise returns <c>false</c>.</returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out CorrespondenceIdExtendedProperty? result)
    {
        result = null;
        if (!string.IsNullOrWhiteSpace(value))
        {
            DateTimeOffset? eventTime = null;
            Guid? correspondenceId = null;
            string[] parts = value.Split(Separator);
            if (parts.Length == 2)
            {
                if (DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedEventTime))
                {
                    eventTime = parsedEventTime;
                }

                if (eventTime != null && Guid.TryParse(parts[1], out Guid parsedCorrespondenceId))
                {
                    correspondenceId = parsedCorrespondenceId;
                }
            }

            if (eventTime.HasValue && correspondenceId.HasValue)
            {
                result = new CorrespondenceIdExtendedProperty(correspondenceId.Value, eventTime.Value);
            }
        }

        return result != null;
    }
}
