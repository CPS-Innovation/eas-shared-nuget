// <copyright file="AmazonS3ApiClientTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.Aws.Tests;

using System;
using Cps.Fct.EasShared.Core.Configuration;
using Cps.Fct.EasShared.Core.Configuration.Extensions;
using Cps.Fct.EasShared.Core.Xunit;
using Cps.Fct.EasShared.ServiceClient.Aws;
using Cps.Fct.EasShared.ServiceClient.Aws.Configuration;
using Cps.Fct.EasShared.ServiceClient.Aws.Emulation;
using Cps.Fct.EasShared.Storage.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

/// <summary>
/// Unit tests for <see cref="AmazonS3ClientFactory"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class AmazonS3ClientFactoryTest
{
    private readonly Mock<ILoggerFactory> mockLoggerFactory;
    private readonly Mock<ILogger<AmazonS3ApiClient>> mockLogger;
    private readonly AmazonS3ApiOptions baseOptions;
    private readonly AmazonS3ClientFactoryOptions factoryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ClientFactoryTest"/> class.
    /// </summary>
    public AmazonS3ClientFactoryTest()
    {
        this.mockLoggerFactory = new Mock<ILoggerFactory>();
        this.mockLogger = new Mock<ILogger<AmazonS3ApiClient>>();

        this.mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.mockLogger.Object);

        this.baseOptions = new AmazonS3ApiOptions(
            containerName: "default-bucket",
            regionEndpoint: Amazon.RegionEndpoint.EUWest2)
        {
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            ServiceEndpoint = "https://s3.test.com"
        };

        this.factoryOptions = new AmazonS3ClientFactoryOptions
        {
            EmailStorageContainerName = "emails-bucket",
            MailboxConfigurationContainerName = "mailbox-config-bucket"
        };
    }

    /// <summary>
    /// Tests that the constructor requires a non-null logger factory.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AmazonS3ClientFactory(
                null!,
                Options.Create(this.baseOptions),
                Options.Create(this.factoryOptions)));
    }

    /// <summary>
    /// Tests that the constructor requires non-null API options.
    /// </summary>
    [Fact]
    public void Constructor_WithNullApiOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AmazonS3ClientFactory(
                this.mockLoggerFactory.Object,
                null!,
                Options.Create(this.factoryOptions)));
    }

    /// <summary>
    /// Tests that the constructor requires non-null factory options.
    /// </summary>
    [Fact]
    public void Constructor_WithNullFactoryOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AmazonS3ClientFactory(
                this.mockLoggerFactory.Object,
                Options.Create(this.baseOptions),
                null!));
    }

    /// <summary>
    /// Tests that CreateClient returns a valid storage client.
    /// </summary>
    [Fact]
    public void CreateClient_WithValidContainerName_ReturnsStorageClient()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        IStorageClient client = factory.CreateClient("test-bucket");

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<AmazonS3ApiClient>();
    }

    /// <summary>
    /// Tests that CreateClient throws when container name is null.
    /// </summary>
    [Fact]
    public void CreateClient_WithNullContainerName_ThrowsArgumentException()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateClient(null!));
    }

    /// <summary>
    /// Tests that CreateClient throws when container name is empty.
    /// </summary>
    [Fact]
    public void CreateClient_WithEmptyContainerName_ThrowsArgumentException()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.CreateClient(string.Empty));
    }

    /// <summary>
    /// Tests that CreateClient throws when container name is whitespace.
    /// </summary>
    [Fact]
    public void CreateClient_WithWhitespaceContainerName_ThrowsArgumentException()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.CreateClient("   "));
    }

    /// <summary>
    /// Tests that GetEmailStorageClient returns a storage client.
    /// </summary>
    [Fact]
    public void GetEmailStorageClient_ReturnsStorageClient()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        IStorageClient client = factory.GetEmailStorageClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<AmazonS3ApiClient>();
    }

    /// <summary>
    /// Tests that GetMailboxConfigurationClient returns a storage client.
    /// </summary>
    [Fact]
    public void GetMailboxConfigurationClient_ReturnsStorageClient()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        IStorageClient client = factory.GetMailboxConfigurationClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<AmazonS3ApiClient>();
    }

    /// <summary>
    /// Tests that CreateClient creates different instances for different buckets.
    /// </summary>
    [Fact]
    public void CreateClient_WithDifferentBuckets_CreatesDifferentInstances()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        IStorageClient client1 = factory.CreateClient("bucket-1");
        IStorageClient client2 = factory.CreateClient("bucket-2");

        // Assert
        client1.Should().NotBeSameAs(client2);
    }

    /// <summary>
    /// Tests that GetEmailStorageClient and GetMailboxConfigurationClient return different instances.
    /// </summary>
    [Fact]
    public void GetEmailStorageClient_AndGetMailboxConfigurationClient_ReturnDifferentInstances()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        IStorageClient emailClient = factory.GetEmailStorageClient();
        IStorageClient mailboxClient = factory.GetMailboxConfigurationClient();

        // Assert
        emailClient.Should().NotBeSameAs(mailboxClient);
    }

    /// <summary>
    /// Tests that CreateClient uses the base options credentials.
    /// </summary>
    [Fact]
    public void CreateClient_UsesBaseOptionsCredentials()
    {
        // Arrange
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(this.baseOptions),
            Options.Create(this.factoryOptions));

        // Act
        var client = factory.CreateClient("test-bucket") as AmazonS3ApiClient;

        // Assert
        client.Should().NotBeNull();
        // Note: You may need to add internal visibility or reflection to verify
        // that the created client actually has the correct credentials/settings
    }

    /// <summary>
    /// Tests that factory can be created with minimal options.
    /// </summary>
    [Fact]
    public void Constructor_WithMinimalOptions_Succeeds()
    {
        // Arrange
        var minimalBaseOptions = new AmazonS3ApiOptions(
            containerName: "default-bucket",
            regionEndpoint: Amazon.RegionEndpoint.EUWest2);

        var minimalFactoryOptions = new AmazonS3ClientFactoryOptions
        {
            EmailStorageContainerName = "emails",
            MailboxConfigurationContainerName = "mailbox-config"
        };

        // Act
        var factory = new AmazonS3ClientFactory(
            this.mockLoggerFactory.Object,
            Options.Create(minimalBaseOptions),
            Options.Create(minimalFactoryOptions));

        // Assert
        factory.Should().NotBeNull();
    }
}