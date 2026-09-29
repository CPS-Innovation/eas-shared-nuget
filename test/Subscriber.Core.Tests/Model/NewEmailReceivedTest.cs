// <copyright file="NewEmailReceivedTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core.Tests.Model;

using System;
using System.Collections.Generic;
using Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// Tests for <see cref="NewEmailReceived"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class NewEmailReceivedTest
{
    /// <summary>
    /// Instantiation succeeds.
    /// </summary>
    [Fact]
    public void InstantiationSucceeds()
    {
        // Assemble
        DocumentDetail detail = new(
            ContentType: "text/plain",
            Name: "test.txt",
            Size: 1024,
            ReadUrl: "#read/test.txt",
            DeleteUrl: "#delete/test.txt");
        List<DocumentDetail> attachments = new() { detail };
        DateTimeOffset eventTime = DateTimeOffset.UtcNow;
        string urn = "12AB1234523";

        // Act
        NewEmailReceived item = new(
            CorrespondenceId: Guid.NewGuid(),
            CorrespondenceStartTime: eventTime.AddMinutes(-1),
            MailboxId: Guid.NewGuid().ToString(),
            MessageId: "xyz",
            Attachments: attachments,
            EventTime: DateTimeOffset.UtcNow,
            From: "AdeleV@zpkw1.onmicrosoft.com",
            FromName: "Adele Vance",
            MessageBodyDeleteUrl: "#delete/email.body.msg",
            MessageBodyName: "email.body.msg",
            MessageBodyReadUrl: "#read/email.body.msg",
            MessageReceivedTime: eventTime.AddSeconds(-5),
            MessageSentTime: eventTime.AddSeconds(-10),
            MessageSubject: $"{urn} subject",
            Urn: urn,
            MessageBodyContentType: "application/vnd.ms-outlook");

        // Assert
        item.Should().NotBeNull();
    }
}
