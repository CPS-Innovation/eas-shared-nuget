// <copyright file="TimeUtility.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.EasShared.Core;

using System.Globalization;

/// <summary>
/// Time utility constants and methods.
/// </summary>
public static class TimeUtility
{
    /// <summary>
    /// The string format for time without Z at the end.
    /// </summary>
    public const string TimeFormatWithoutZ = "yyyy-MM-ddTHH:mm:ss.fff";

    /// <summary>
    /// The string format for time.
    /// </summary>
    public const string TimeFormat = $"{TimeFormatWithoutZ}Z";

    /// <summary>
    /// The string format for tracing time.
    /// </summary>
    public const string TraceTimeFormat = TimeFormat;

    /// <summary>
    /// Gets the UK local time zone.
    /// </summary>
    public static TimeZoneInfo UKLocal => TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    /// <summary>
    /// Gets the UK local time text.
    /// </summary>
    /// <param name="dateTime">The time to convert to UK local time.</param>
    /// <returns>Returns <paramref name="dateTime"/> as UK local time text.</returns>
    public static string GetUKLocalTime(DateTimeOffset dateTime)
    {
        dateTime = TimeZoneInfo.ConvertTime(dateTime, UKLocal);
        return $"{dateTime.ToString(TimeFormatWithoutZ, CultureInfo.InvariantCulture)} (UK)";
    }

    /// <summary>
    /// Converts the given date time offset to UK local time.
    /// </summary>
    /// <param name="dateTime">The date time offset to convert to UK local time.</param>
    /// <returns>Returns the date time offset in UK local time.</returns>
    public static DateTimeOffset GetUKDateTimeOffset(DateTimeOffset dateTime)
    {
        return TimeZoneInfo.ConvertTime(dateTime, UKLocal);
    }

    /// <summary>
    /// Converts the given date time offset to UTC.
    /// </summary>
    /// <param name="dateTime">The date time offset to convert to UTC.</param>
    /// <returns>Returns the date time offset in UTC.</returns>
    public static DateTimeOffset GetUtcDateTimeOffset(DateTimeOffset dateTime)
    {
        return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc);
    }
}
