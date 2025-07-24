// <copyright file="SubscriberException.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.EasShared.Subscriber.Core;

using System;

/// <summary>
/// An exception thrown by a cloud event subscriber.
/// </summary>
/// <remarks>The exception's message is suitable to display to an end user.</remarks>
[Serializable]
public class SubscriberException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriberException"/> class.
    /// </summary>
    /// <param name="message">A message that is safe enough to display to an end user.</param>
    public SubscriberException(string message)
        : base(message)
    {
    }
}
