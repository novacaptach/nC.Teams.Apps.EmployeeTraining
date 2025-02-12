﻿// <copyright file="BotSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// A class which helps to provide Bot settings for application.
/// </summary>
public class BotSettings
{
    /// <summary>
    /// Gets or sets application base Uri which helps in generating customer token.
    /// </summary>
    public string AppBaseUri { get; set; }

    /// <summary>
    /// Gets or sets the median delay to target before the first retry, call it f (= f * 2^0).
    /// </summary>
    public double MedianFirstRetryDelay { get; set; }

    /// <summary>
    /// Gets or sets retry count that represents the maximum number of retries to use, in addition to the original call.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets application manifest id.
    /// </summary>
    public string ManifestId { get; set; }

    /// <summary>
    /// Gets or sets application id.
    /// </summary>
    public string MicrosoftAppId { get; set; }

    /// <summary>
    /// Gets or sets application password.
    /// </summary>
    public string MicrosoftAppPassword { get; set; }

    /// <summary>
    /// Gets or sets cache interval.
    /// </summary>
    public double CacheDurationInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of events need to render per page.
    /// </summary>
    public int EventsPageSize { get; set; }
}