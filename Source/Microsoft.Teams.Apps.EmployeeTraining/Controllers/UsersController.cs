// <copyright file="UsersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using User = Microsoft.Graph.User;

/// <summary>
/// Exposes APIs related to event operations.
/// </summary>
[Route(template: "api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : BaseController
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
    /// Graph helper for users API.
    /// </summary>
    private readonly IUserGraphHelper userGraphHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController" /> class.
    /// </summary>
    /// <param name="logger">The ILogger object which logs errors and information</param>
    /// <param name="telemetryClient">The Application Insights telemetry client</param>
    /// <param name="userGraphHelper">Graph helper for users API.</param>
    /// <param name="groupGraphHelper">Graph API helper for fetching group related data.</param>
    public UsersController(
        ILogger<UsersController> logger,
        TelemetryClient telemetryClient,
        IUserGraphHelper userGraphHelper,
        IGroupGraphHelper groupGraphHelper)
        : base(telemetryClient: telemetryClient)
    {
        this.logger = logger;
        this.userGraphHelper = userGraphHelper;
        this.groupGraphHelper = groupGraphHelper;
    }

    /// <summary>
    /// The HTTP GET call to get all event categories
    /// </summary>
    /// <param name="searchText">Search text entered by user.</param>
    /// <returns>
    /// Returns the list of categories sorted by category name if request processed successfully. Else, it throws an
    /// exception.
    /// </returns>
    [HttpGet]
    [ResponseCache(Duration = 86400)] // cache for 1 day
    public async Task<IActionResult> SearchUsersrAndGroups(string searchText)
    {
        searchText ??= string.Empty;
        this.RecordEvent(eventName: "Search users and group - The HTTP call to GET users/groups has been initiated");
        try
        {
            var searchResults = new List<UserGroupSearchResult>();
            var users = new List<User>();
            var groups = new List<Group>();

            var getUsersTask = this.userGraphHelper.SearchUsersAsync(searchText: searchText);
            var getGroupsTask = this.groupGraphHelper.SearchGroupsAsync(searchText: searchText);
            await Task.WhenAll(getUsersTask, getGroupsTask);

            users = getUsersTask.Result;
            groups = getGroupsTask.Result;

            searchResults.AddRange(collection: users.Select(user => new UserGroupSearchResult
            {
                DisplayName = user.DisplayName,
                Id = user.Id,
                IsGroup = false,
                Email = user.Mail,
            }));
            searchResults.AddRange(collection: groups?.Select(group => new UserGroupSearchResult
            {
                DisplayName = group.DisplayName,
                Id = group.Id,
                IsGroup = true,
                Email = group.Mail,
            }));

            this.RecordEvent(eventName: "Search users and group - The HTTP call to GET users/groups succeeded");

            return this.Ok(value: searchResults.OrderBy(userAndGroup => userAndGroup.DisplayName));
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Search users and group - The HTTP call to GET users/groups failed");
            this.logger.LogError(exception: ex, message: "Error occurred while fetching users/groups");
            throw;
        }
    }

    /// <summary>
    /// Get user profiles by user object Ids.
    /// </summary>
    /// <param name="userIds">List of user object Ids.</param>
    /// <returns>List of user profiles.</returns>
    [HttpPost]
    [ResponseCache(Duration = 1209600)] // Cache data for 14 days.
    public async Task<IActionResult> GetUsersProfiles([FromBody] List<string> userIds)
    {
        this.RecordEvent(eventName: "Get users profiles - The HTTP call to GET users profiles has been initiated");

        if ((userIds == null) || !userIds.Any())
        {
            this.RecordEvent(eventName: "Get users profiles - The HTTP call to GET users profiles has been failed");
            this.logger.LogError(message: "User Id list cannot be null or empty");
            return this.BadRequest(error: new { message = "User Id list cannot be null or empty" });
        }

        try
        {
            var userProfiles = await this.userGraphHelper.GetUsersAsync(userObjectIds: userIds);
            this.RecordEvent(eventName: "Get users profiles - The HTTP call to GET users profiles has been succeeded");

            if (userProfiles != null)
            {
                return this.Ok(value: userProfiles.Select(user => new { user.DisplayName, user.Id }).ToList());
            }

            return this.Ok(value: new List<Models.User>());
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get users profiles - The HTTP call to GET users profiles has been failed");
            this.logger.LogError(exception: ex, message: "Error occurred while fetching users profiles");
            throw;
        }
    }
}