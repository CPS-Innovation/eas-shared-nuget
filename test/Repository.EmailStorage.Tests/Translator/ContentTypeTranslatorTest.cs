// <copyright file="ContentTypeTranslatorTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage.Tests.Translator;

using Cps.Fct.EasShared.Repository.EmailStorage.Translator;

/// <summary>
/// Tests the <see cref="ContentTypeTranslator"/> class.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class ContentTypeTranslatorTest
{
    /// <summary>
    /// Tests that the <see cref="ContentTypeTranslator"/> translates the content type to the correct content type.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="expectedContentType">The expected content type.</param>
    [Theory]
    [InlineData("a.bin", "application/octet-stream", "application/octet-stream")]
    [InlineData("a.DOC", "application/octet-stream", "application/msword")]
    [InlineData("a.docx", "application/octet-stream", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("a.dot", "application/octet-stream", "application/msword")]
    [InlineData("a.jpeg", "application/octet-stream", "image/jpeg")]
    [InlineData("a.pdf", "application/octet-stream", "application/pdf")]
    [InlineData("a.rtf", "application/octet-stream", "application/rtf")]
    [InlineData("a.tif", "application/octet-stream", "image/tiff")]
    [InlineData("a.txt", "application/octet-stream", "text/plain")]
    [InlineData("a.XLS", "application/octet-stream", "application/vnd.ms-excel")]
    [InlineData("a.xlsx", "application/octet-stream", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("x", "message/delivery-status", "text/plain")]
    [InlineData("x", "message/rfc822", "message/rfc822")]
    [InlineData("x", "text/rfc822-headers", "text/plain")]
    [InlineData("x.eml", "application/octet-stream", "message/rfc822")]
    [InlineData("x.eml", "text/html", "message/rfc822")]
    [InlineData("x.EML", "text/plain", "message/rfc822")]
    [InlineData("x.msg", "application/vnd.ms-outlook", "application/vnd.ms-outlook")]
    [InlineData("x.MSG", "application/octet-stream", "application/vnd.ms-outlook")]
    [InlineData("x.msg", "text/html", "application/vnd.ms-outlook")]
    [InlineData("x.msg", "text/plain", "application/vnd.ms-outlook")]
    public void ContentTypeTranslationSucceeds(string filename, string contentType, string expectedContentType)
    {
        // Arrange
        ContentTypeTranslator contentTypeTranslator = new();
        ContentTypeInfo contentTypeInfo = new(ContentType: contentType, Filename: filename);

        // Act
        string result = contentTypeTranslator.Translate(contentTypeInfo);

        // Assert
        result.Should().Be(expectedContentType);
    }
}
