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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Tests the Amazon S3 API client.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class AmazonS3ApiClientTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmazonS3ApiClientTest"/> class.
    /// </summary>
    /// <param name="output">Provides test output.</param>
    public AmazonS3ApiClientTest(ITestOutputHelper output)
    {
        string[] args = Array.Empty<string>();

        IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder.ProvisionDefaultConfigurationProviders(args, "Development", false);
            })
            .ConfigureLogging(builder =>
            {
                builder
                    .AddXunitLogger();
            })
            .ConfigureServices(services =>
            {
                services
                    .AddServiceOptions<AmazonS3ApiOptions>(AmazonS3ApiOptions.DefaultSectionName)
                    .AddServiceOptions<AmazonS3ClientFactoryOptions>(AmazonS3ClientFactoryOptions.DefaultSectionName)
                    .AddServiceOptions<TestOptions>(TestOptions.DefaultSectionName)
                    .AddSingleton<IAmazonS3ClientFactory, FakeS3ClientFactory>();

                services
                    .AddSingleton(output)
                    .AddSingleton<AmazonS3ApiClient>(sp =>
                    {
                        var factory = sp.GetRequiredService<IAmazonS3ClientFactory>();
                        var apiOptions = sp.GetRequiredService<IOptions<AmazonS3ApiOptions>>();
                        return (AmazonS3ApiClient)factory.CreateClient(apiOptions.Value.ContainerName);
                    });
                //.AddSingleton<AmazonS3ApiClient>(sp =>
                //{
                //    var testOptions = sp.GetRequiredService<IOptions<TestOptions>>().Value;
                //
                //    if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
                //    {
                //        // For unit tests: return the fake directly
                //        return sp.GetRequiredService<FakeAmazonS3ApiClient>();
                //    }
                //    else
                //    {
                //        // For real usage: use the factory
                //        var factory = sp.GetRequiredService<IAmazonS3ClientFactory>();
                //        var apiOptions = sp.GetRequiredService<IOptions<AmazonS3ApiOptions>>();
                //        return (AmazonS3ApiClient)factory.CreateClient(apiOptions.Value.ContainerName);
                //    }
                //})
                //.AddSingleton<FakeAmazonS3ApiClient>()
                ;
            })
            .Build();

        IServiceScope serviceScope = host.Services.CreateScope();
        this.ServiceProvider = serviceScope.ServiceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Instantiates a <see cref="AmazonS3ApiClient"/> and validates that <c>Client</c> property is not <c>null</c>.
    /// </summary>
    [Fact]
    public void AmazonS3ClientIsNotNull()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();

        // Act
        IAmazonS3 serviceClient = client.Client;

        // Assert
        serviceClient.Should().NotBeNull();
    }

    /// <summary>
    /// The method <c>ObjectExistsAsync</c> returns <c>false</c> given a key that does not exist.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task ObjectExistsReturnsFalseGivenAKeyThatDoesNotExist()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();

        // Act
        bool exists = await client.ObjectExistsAsync(Guid.NewGuid().ToString()).ConfigureAwait(true);

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Putting an object into an S3 bucket succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task PutObjectSucceeds()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        TestObject obj = new()
        {
            Key = $"Test/{Guid.NewGuid().ToString()}",
        };

        // Act
        await client.PutObjectAsync(obj.Key, obj).ConfigureAwait(true);
        bool exists = await client.ObjectExistsAsync(obj.Key).ConfigureAwait(true);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Putting a stream into an S3 bucket succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task PutStreamSucceeds()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        string key = $"Test/{Guid.NewGuid().ToString()}";
        byte[] content = new byte[] { 1, 2, 3, 4 };
        using MemoryStream stream = new(content);

        // Act
        await client.PutStreamAsync(key, stream).ConfigureAwait(true);
        bool exists = await client.ObjectExistsAsync(key).ConfigureAwait(true);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Specifying an access key and secret key as an option succeeds.
    /// </summary>
    [Fact]
    public void SpecifyingAccessKeySucceeds()
    {
        // Assemble
        ILoggerFactory loggerFactory = this.ServiceProvider.GetRequiredService<ILoggerFactory>();
        AmazonS3ApiOptions options = new()
        {
            AccessKey = "xyz",
            SecretKey = "abc",
        };

        OptionsWrapper<AmazonS3ApiOptions> optionsWrapper = new(options);

        // Act
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();

        // Assert
        client.Client.Should().NotBeNull();
    }

    /// <summary>
    /// Specifying a service endpoint as an option succeeds.
    /// </summary>
    [Fact]
    public void SpecifyingServiceEndpointSucceeds()
    {
        // Arrange
        ILoggerFactory loggerFactory = this.ServiceProvider.GetRequiredService<ILoggerFactory>();
        AmazonS3ApiOptions options = new(
            containerName: "test-bucket",
            regionEndpoint: Amazon.RegionEndpoint.EUWest2)
        {
            ServiceEndpoint = "http://localhost",
        };
        OptionsWrapper<AmazonS3ApiOptions> optionsWrapper = new(options);

        // Act
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();

        // Assert
        client.Client.Should().NotBeNull();
    }

    /// <summary>
    /// Getting an access URL succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnAccessUrlSucceeds()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        TestObject obj = new()
        {
            Key = "Test/Object",
        };

        bool exists = await client.ObjectExistsAsync(obj.Key).ConfigureAwait(true);
        if (!exists)
        {
            await client.PutObjectAsync(obj.Key, obj).ConfigureAwait(true);
        }

        // Act
        string url = client.GetAccessUrl(obj.Key, TimeSpan.FromDays(2));

        // Assert
        url.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Getting a delete URL succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingADeleteUrlSucceeds()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        TestObject obj = new()
        {
            Key = "Test/Object",
        };

        bool exists = await client.ObjectExistsAsync(obj.Key).ConfigureAwait(true);
        if (!exists)
        {
            await client.PutObjectAsync(obj.Key, obj).ConfigureAwait(true);
        }

        // Act
        string url = client.GetDeleteUrl(obj.Key, TimeSpan.FromDays(2));

        // Assert
        url.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Getting an object succeeds.
    /// </summary>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Fact]
    public async Task GettingAnObjectSucceeds()
    {
        // Assemble
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        TestObject obj = new()
        {
            Key = "Test/Object",
        };

        bool exists = await client.ObjectExistsAsync(obj.Key).ConfigureAwait(true);
        if (!exists)
        {
            await client.PutObjectAsync(obj.Key, obj).ConfigureAwait(true);
        }

        // Act
        TestObject result = await client.GetObjectAsync<TestObject>(obj.Key).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(obj);
    }

    /// <summary>
    /// Creating objects and then deleting them succeeds.
    /// </summary>
    /// <param name="folder">The folder to use for the test.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    [Theory]
    [InlineData("Test/Folder")]
    [InlineData("Test/[OS] Test 29/08/2023")]
    public async Task CreatingObjectsAndThenDeletingThemSucceeds(string folder)
    {
        // Assemble
        int numberOfObjects = 3;
        List<string> keys = new(numberOfObjects);
        AmazonS3ApiClient client = this.ServiceProvider.GetRequiredService<AmazonS3ApiClient>();
        for (int index = 0; index < numberOfObjects; index++)
        {
            string key = $"{folder}/{Guid.NewGuid()}";
            TestObject obj = new()
            {
                Key = key,
            };

            await client.PutObjectAsync(obj.Key, obj).ConfigureAwait(true);
            keys.Add(key);
        }

        foreach (string key in keys)
        {
            bool exists = await client.ObjectExistsAsync(key).ConfigureAwait(true);
            exists.Should().BeTrue();
        }

        // Act
        foreach (string key in keys)
        {
            await client.DeleteObjectAsync(key).ConfigureAwait(true);
        }

        // Assert
        foreach (string key in keys)
        {
            bool exists = await client.ObjectExistsAsync(key).ConfigureAwait(true);
            exists.Should().BeFalse();
        }
    }

    //private static AmazonS3ApiClient CreateAmazonS3ApiClient(IServiceProvider serviceProvider)
    //{
    //    ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    //    IOptions<AmazonS3ApiOptions> apiOptions = serviceProvider.GetRequiredService<IOptions<AmazonS3ApiOptions>>();
    //    var s3ClientFactory = serviceProvider.GetRequiredService<IAmazonS3ClientFactory>();

    //    AmazonS3ApiClient serviceClient = (AmazonS3ApiClient)s3ClientFactory.CreateClient(apiOptions.Value.ContainerName);

    //    TestOptions testOptions = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;
    //    if (testOptions.Category.Equals(TestOptions.UnitTest, StringComparison.OrdinalIgnoreCase))
    //    {
    //        serviceClient = new FakeAmazonS3ApiClient(
    //            loggerFactory: loggerFactory,
    //            apiOptions: apiOptions);
    //    }

    //    return serviceClient;
    //}

    private sealed class TestObject
    {
        /// <summary>
        /// Gets or sets the key value of this object.
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
