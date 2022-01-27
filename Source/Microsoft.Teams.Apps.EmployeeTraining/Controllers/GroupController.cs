// <copyright file="GroupController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;

/// <summary>
/// Exposes APIs related to Microsoft Graph group operations.
/// </summary>
[Route(template: "api/[controller]")]
[ApiController]
[Authorize]
public class GroupController : BaseController
{
    /// <summary>
    /// Graph API helper for fetching group related data.
    /// </summary>
    private readonly IGroupGraphHelper groupGraphHelper;

    /// <summary>
    /// Logs errors and information
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupController" /> class.
    /// </summary>
    /// <param name="logger">The ILogger object which logs errors and information.</param>
    /// <param name="telemetryClient">The Application Insights telemetry client.</param>
    /// <param name="groupGraphHelper">Graph API helper for fetching group related data.</param>
    public GroupController(
        ILogger<GroupController> logger,
        TelemetryClient telemetryClient,
        IGroupGraphHelper groupGraphHelper)
        : base(telemetryClient: telemetryClient)
    {
        this.logger = logger;
        this.groupGraphHelper = groupGraphHelper;
    }

    /// <summary>
    /// Get group members.
    /// </summary>
    /// <param name="groupId">Group object Id.</param>
    /// <returns>List of user profiles.</returns>
    [HttpGet(template: "get-group-members")]
    [ResponseCache(Duration = 86400)] // cache for 1 day
    public async Task<IActionResult> GetMembersAsync(string groupId)
    {
        this.RecordEvent(eventName: "Get group members - The HTTP call to GET group members has been initiated", payload: new Dictionary<string, string>
        {
            { "groupId", groupId },
        });

        if (string.IsNullOrEmpty(value: groupId))
        {
            this.RecordEvent(eventName: "Get group members - The HTTP call to GET group members has been failed", payload: new Dictionary<string, string>
            {
                { "groupId", groupId },
            });
            this.logger.LogError(message: "Group Id cannot be null or empty");
            return this.BadRequest(error: new ErrorResponse { Message = "Group Id cannot be null or empty" });
        }

        try
        {
            var groupMembers = await this.groupGraphHelper.GetGroupMembersAsync(groupId: groupId);
            this.RecordEvent(eventName: "Get group members - The HTTP call to GET group members has been succeeded", payload: new Dictionary<string, string>
            {
                { "groupId", groupId },
            });
            return this.Ok(value: groupMembers);
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get group members - The HTTP call to GET group members has been failed", payload: new Dictionary<string, string>
            {
                { "groupId", groupId },
            });
            this.logger.LogError(exception: ex, message: "Error occurred while fetching users profiles");
            throw;
        }
    }
}