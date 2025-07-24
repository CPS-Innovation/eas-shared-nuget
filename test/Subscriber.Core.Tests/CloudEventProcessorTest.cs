// <copyright file="CloudEventProcessorTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests;

using System;
using System.Globalization;
using System.Net.Mime;
using Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// Tests a cloud event processor.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class CloudEventProcessorTest
{
    private const string MailboxId = "338b27ea-6f8e-42f0-849d-e396fd161156";
    private const string Source = $"Users/{MailboxId}/Messages";
    private const string MessageId = "AAMkADYyNDU0NWEwLTJlMzYtNGExZC1hZTJmLTMxYzY5ZTBkNDMxZABGAAAAAABdhU36QI-kTLa-1b3sH5CzBwDlGkianql-Rr9Dp21TeC_GAAAAAAEKAADlGkianql-Rr9Dp21TeC_GAAAHlKtcAAA=";

    /// <summary>
    /// Calling <c>Process</c> succeeds.
    /// </summary>
    [Fact]
    public void CallingProcessSucceeds()
    {
        // Assemble
        CloudEventProcessor processor = new();
        var correlationId = Guid.NewGuid();

        // Act
        processor.Process(CreateTestCloudEvent(correlationId));

        // Assert
        processor.HasProcessed.Should().BeTrue();
    }

    /// <summary>
    /// Calling <c>Process</c> fails.
    /// </summary>
    [Fact]
    public void CallingProcessFails()
    {
        // Assemble
        CloudEventProcessor processor = new();
        string message = "Simulated error";
        processor.ThrowOnNextProcessCall = new SubscriberException(message);
        var correlationId = Guid.NewGuid();

        // Act
        SubscriberException exception = Assert.ThrowsAny<SubscriberException>(() => processor.Process(CreateTestCloudEvent(correlationId)));

        // Assert
        processor.HasProcessed.Should().BeFalse();
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }

    /// <summary>
    /// Translating <see cref="CloudEvent"/> to <see cref="NewEmailNotificationReceived"/> succeeds.
    /// </summary>
    [Fact]
    public void TranslatingCloudEventToNewEmailNotificationReceivedSucceeds()
    {
        // Assemble
        ICloudEventProcessor<NewEmailNotificationReceived> processor = new CloudEventProcessor();
        var correlationId = Guid.NewGuid();

        // Act
        NewEmailNotificationReceived data = processor.Translate(CreateTestCloudEvent(correlationId));

        // Assert
        data.Should().Be(CreateDataPayload(correlationId));
    }

    /// <summary>
    /// Translating a decoded <see cref="CloudEvent"/> To <see cref="NewEmailNotificationReceived"/> succeeds.
    /// </summary>
    [Fact]
    public void TranslateDecodedCloudEventToNewEmailNotificationReceivedSucceeds()
    {
        // Assemble
        var correlationId = Guid.NewGuid();
        JsonEventFormatter jsonEventFormatter = new();
        ICloudEventProcessor<NewEmailNotificationReceived> processor = new CloudEventProcessor();
        CloudEvent cloudEvent = CreateTestCloudEvent(correlationId);
        ReadOnlyMemory<byte> body = jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out ContentType _);
        cloudEvent = jsonEventFormatter.DecodeStructuredModeMessage(body, new ContentType("application/json"), null);

        // Act
        NewEmailNotificationReceived data = processor.Translate(cloudEvent);

        // Assert
        data.Should().Be(CreateDataPayload(correlationId));
    }

    /// <summary>
    /// Translating <see cref="CloudEvent"/> to <see cref="NewEmailNotificationReceived"/> fails.
    /// </summary>
    [Fact]
    public void TranslateCloudEventToNewEmailNotificationReceivedFails()
    {
        // Assemble
        ICloudEventProcessor<NewEmailNotificationReceived> processor = new CloudEventProcessor();
        CloudEvent cloudEvent = new()
        {
            Type = "#Microsoft.Graph.Message",
            Source = new Uri(Source, UriKind.Relative),
            Subject = MailboxId,
            Id = MessageId,
            Time = DateTimeOffset.UtcNow,
            DataContentType = SubscriberConstants.NewEmailNotificationReceivedContentType,
            Data = "Not a NewEmailNotificationReceived entity",
        };

        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => processor.Translate(cloudEvent));

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Expecting a cloud event with a data payload of type {typeof(NewEmailNotificationReceived)}");
    }

    private static NewEmailNotificationReceived CreateDataPayload(Guid correlationId)
    {
        return new(
            CorrespondenceId: correlationId,
            ETag: "W/\"CQAAABYAAADlGkianql/Rr9Dp21TeC+GAAAHk7N4\"",
            EventTime: DateTimeOffset.Parse("2023-03-01T14:11:48.3343433Z", CultureInfo.InvariantCulture),
            MailboxId: MailboxId,
            MessageId: MessageId);
    }

    private static CloudEvent CreateTestCloudEvent(Guid correlationId)
    {
        NewEmailNotificationReceived payload = CreateDataPayload(correlationId);

        CloudEvent result = new()
        {
            Type = "#Microsoft.Graph.Message",
            Source = new Uri(Source, UriKind.Relative),
            Subject = MailboxId,
            Id = $"{MessageId} {payload.ETag}",
            Time = payload.EventTime,
            DataContentType = SubscriberConstants.NewEmailNotificationReceivedContentType,
            Data = payload,
        };

        result[SubscriberConstants.ChangeTypeAttributeName] = "Created";
        return result;
    }
}
