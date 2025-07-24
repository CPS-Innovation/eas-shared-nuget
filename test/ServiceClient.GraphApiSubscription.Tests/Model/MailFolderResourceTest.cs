// <copyright file="MailFolderResourceTest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Tests.Model;

using Cps.Fct.EasShared.ServiceClient.GraphApiSubscription.Model;

/// <summary>
/// Tests for <see cref="MailFolderResource"/>.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Status", "Ready")]
public class MailFolderResourceTest
{
    /// <summary>
    /// Resource is as expected.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID.</param>
    /// <param name="folderId">The folder ID.</param>
    /// <param name="expectedResource">The expected resource.</param>
    [Theory]
    [InlineData("dec13ff3-70b1-43df-8e90-c172daa2dae3", "xyz", "/users/dec13ff3-70b1-43df-8e90-c172daa2dae3/mailFolders/xyz/messages")]
    [InlineData("x@z.com", "x z", "/users/x%40z.com/mailFolders/x%20z/messages")]
    public void ResourceIsAsExpected(string mailboxId, string folderId, string expectedResource)
    {
        // Assemble
        MailFolderResource mailFolderResource = new(mailboxId, folderId);

        // Act
        string resource = mailFolderResource.Resource;

        // Assert
        resource.Should().Be(expectedResource);
    }

    /// <summary>
    /// Try parsing a resource returns true.
    /// </summary>
    /// <param name="resource">The resource to parse.</param>
    /// <param name="mailboxId">The expected mailbox ID.</param>
    /// <param name="folderId">The expected folder ID.</param>
    [Theory]
    [InlineData("/users/dec13ff3-70b1-43df-8e90-c172daa2dae3/mailFolders/xyz/messages", "dec13ff3-70b1-43df-8e90-c172daa2dae3", "xyz")]
    [InlineData("/users/x%40z.com/mailFolders/x%20z/messages", "x@z.com", "x z")]
    public void TryParsingAResourceReturnsTrue(string resource, string mailboxId, string folderId)
    {
        // Act
        bool result = MailFolderResource.TryParse(resource, out MailFolderResource? mailFolderResource);

        // Assert
        result.Should().BeTrue();
        mailFolderResource.Should().NotBeNull();
        mailFolderResource!.MailboxId.Should().Be(mailboxId);
        mailFolderResource.FolderId.Should().Be(folderId);
    }

    /// <summary>
    /// Try parsing a resource returns false.
    /// </summary>
    /// <param name="resource">The resource to parse.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/users")]
    [InlineData("/users/x%40z.com")]
    [InlineData("/users/x%40z.com/mailFolders")]
    [InlineData("/users/x%40z.com/mailFolders/x%20z/")]
    [InlineData("/users/x%40z.com/mailFolders/x%20z/messages/too-long")]
    [InlineData("/xusers/x%40z.com")]
    [InlineData("/users/x%40z.com/xmailFolders")]
    [InlineData("/users/x%40z.com/mailFolders/x%20z/xmessages")]
    public void TryParsingAResourceReturnsFalse(string? resource)
    {
        // Act
        bool result = MailFolderResource.TryParse(resource!, out MailFolderResource? mailFolderResource);

        // Assert
        result.Should().BeFalse();
        mailFolderResource.Should().BeNull();
    }
}
