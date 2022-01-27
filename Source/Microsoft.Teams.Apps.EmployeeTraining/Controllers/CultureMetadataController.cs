// <copyright file="CultureMetadataController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers;

using System;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// The controller handles the data requests related to cultures.
/// </summary>
[Route(template: "api/[controller]")]
[ApiController]
public class CultureMetadataController : BaseController
{
    /// <summary>
    /// Default culture.
    /// </summary>
    private readonly string defaultCulture;

    /// <summary>
    /// Logs errors and information.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Supported cultures.
    /// </summary>
    private readonly string[] supportedCultures;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureMetadataController" /> class.
    /// </summary>
    /// <param name="logger">The ILogger object which logs errors and information</param>
    /// <param name="telemetryClient">The Application Insights telemetry client</param>
    /// <param name="configuration">IConfiguration instance.</param>
    public CultureMetadataController(
        ILogger<EventFilesController> logger,
        TelemetryClient telemetryClient,
        IConfiguration configuration)
        : base(telemetryClient: telemetryClient)
    {
        this.logger = logger;
        if (configuration == null)
        {
            throw new ArgumentNullException(paramName: nameof(configuration));
        }

        this.defaultCulture = configuration.GetValue<string>(key: "i18n:DefaultCulture");
        if (!string.IsNullOrEmpty(value: configuration.GetValue<string>(key: "i18n:SupportedCultures")))
        {
            this.supportedCultures = configuration.GetValue<string>(key: "i18n:SupportedCultures").Split(separator: ",");
        }
    }

    /// <summary>
    /// Get default culture from configuration.
    /// </summary>
    /// <returns>Default culture</returns>
    [HttpGet]
    public string GetDefaultCulture()
    {
        this.RecordEvent(eventName: "Get event- The HTTP GET call to get default culture has been initiated");
        try
        {
            return this.defaultCulture;
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get event- The HTTP GET call to get default culture has been failed");
            this.logger.LogError(exception: ex, message: "Error occurred while fetching default culture");
            throw;
        }
    }

    /// <summary>
    /// Get supported cultures from configuration.
    /// </summary>
    /// <returns>Supported culture</returns>
    [HttpGet(template: "supportedcultures")]
    public string[] GetSupportedCultures()
    {
        this.RecordEvent(eventName: "Get event- The HTTP GET call to get supported culture has been initiated");
        try
        {
            return this.supportedCultures;
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get event- The HTTP GET call to get supported culture has been failed");
            this.logger.LogError(exception: ex, message: "Error occurred while fetching supported culture");
            throw;
        }
    }
}