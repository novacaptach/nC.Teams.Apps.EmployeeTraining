﻿// <copyright file="BaseController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The base controller which handles application's API operations.
/// </summary>
[Route(template: "api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Instance of application insights telemetry client.
    /// </summary>
    private readonly TelemetryClient telemetryClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController" /> class.
    /// </summary>
    /// <param name="telemetryClient">The Application Insights telemetry client.</param>
    public BaseController(TelemetryClient telemetryClient)
    {
        this.telemetryClient = telemetryClient;
    }

    /// <summary>
    /// Gets the user tenant id from the HttpContext.
    /// </summary>
    protected string UserTenantId
    {
        get
        {
            var tenantClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
            var claim = this.User.Claims.FirstOrDefault(p => tenantClaimType.Equals(value: p.Type, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }
    }

    /// <summary>
    /// Gets the user Azure Active Directory id from the HttpContext.
    /// </summary>
    protected string UserAadId
    {
        get
        {
            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            var claim = this.User.Claims.FirstOrDefault(p => oidClaimType.Equals(value: p.Type, comparisonType: StringComparison.OrdinalIgnoreCase));
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }
    }

    /// <summary>
    /// Gets the user name from the HttpContext.
    /// </summary>
    protected string UserName
    {
        get
        {
            var claim = this.User.Claims.FirstOrDefault(p => "name".Equals(value: p.Type, comparisonType: StringComparison.OrdinalIgnoreCase));
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }
    }

    /// <summary>
    /// Gets the user principal name from the HttpContext.
    /// </summary>
    protected string UserPrincipalName => this.User.FindFirst(type: ClaimTypes.Upn)?.Value;

    /// <summary>
    /// Records event data to Application Insights telemetry client.
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="payload">Payload which needs to be logged against event.</param>
    public void RecordEvent(
        string eventName,
        IDictionary<string, string> payload = null)
    {
        var payloadDictionary = new Dictionary<string, string>
        {
            { "userId", this.UserAadId },
        };

        if (payload != null)
        {
            foreach (var item in payload)
            {
                payloadDictionary.Add(key: item.Key, value: item.Value);
            }
        }

        this.telemetryClient.TrackEvent(eventName: eventName, properties: payloadDictionary);
    }
}