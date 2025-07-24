// <copyright file="IEmailStorageRepository.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Repository.EmailStorage;

using System.Threading.Tasks;
using Cps.Fct.EasShared.Subscriber.Core.Model;

/// <summary>
/// An interface for an email storage repository.
/// </summary>
public interface IEmailStorageRepository
{
    /// <summary>
    /// Persists the email message into BLOB storage.
    /// </summary>
    /// <param name="notificationReceived">The new email notification received event.</param>
    /// <returns>A <see cref="Task" /> that will complete when the operation has completed.</returns>
    Task PersistEmailMessageAsync(NewEmailNotificationReceived notificationReceived);
}
