// <copyright file="SubscriberConstants.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core;

/// <summary>
/// Subscriber constants.
/// </summary>
public static class SubscriberConstants
{
    /// <summary>
    /// The media type for a <c>NewEmailNotificationReceived</c>.
    /// </summary>
    public const string NewEmailNotificationReceivedContentType = "application/vnd.Cps.Fct.EasShared.NewEmailNotificationReceived+json";

    /// <summary>
    /// The media type for a <c>NewEmailReceived</c>.
    /// </summary>
    public const string NewEmailReceivedContentType = "application/vnd.Cps.Fct.EasShared.NewEmailReceived+json";

    /// <summary>
    /// The name of the change type attribute. Needs to conform to <c>CloudEvent</c> attribute naming convention:
    /// must consist of lower-case letters ('a' to 'z') or digits ('0' to '9') from the ASCII character set.
    /// Attribute names should be descriptive and terse and should not exceed 20 characters in length.
    /// </summary>
    /// <remarks>See https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#naming-conventions.</remarks>
    public const string ChangeTypeAttributeName = "changetype";

    /// <summary>
    /// The default content type.
    /// </summary>
    public const string DefaultContentType = "application/octet-stream";

    /// <summary>
    /// The content type for an Outlook email message.
    /// </summary>
    public const string OutlookMessageContentType = "application/vnd.ms-outlook";

    /// <summary>
    /// The content type for the email message body.
    /// </summary>
    public const string EmailBodyContentType = "text/html";

    /// <summary>
    /// The name of the email message body in HTML format as it is stored in AWS S3 and CMS.
    /// </summary>
    public const string EmailBodyHtmlFilename = "email-body.hte";

    /// <summary>
    /// The name of the email message body in Outlook (MSG) format as it is stored in AWS S3 and CMS.
    /// </summary>
    public const string EmailBodyOutlookFilename = "email-body.msg";

    /// <summary>
    /// The content type for a MIME (eml) email message.
    /// </summary>
    public const string MimeMessageContentType = "message/rfc822";
}
