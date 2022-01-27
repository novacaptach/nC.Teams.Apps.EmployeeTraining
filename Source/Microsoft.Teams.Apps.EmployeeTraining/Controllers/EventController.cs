// <copyright file="EventController.cs" company="Microsoft">
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService;

/// <summary>
/// Exposes APIs related to event operations.
/// </summary>
[Route(template: "api/[controller]")]
[ApiController]
[Authorize]
public class EventController : BaseController
{
    /// <summary>
    /// Category helper for fetching based on Ids, binding category names to events
    /// </summary>
    private readonly ICategoryHelper categoryHelper;

    /// <summary>
    /// Logs errors and information
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Search service to search and filter events.
    /// </summary>
    private readonly IUserEventSearchService userEventSearchService;

    /// <summary>
    /// The event helper for performing user operations on events created by LnD team
    /// </summary>
    private readonly IUserEventsHelper userEventsHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventController" /> class.
    /// </summary>
    /// <param name="logger">The ILogger object which logs errors and information</param>
    /// <param name="telemetryClient">The Application Insights telemetry client</param>
    /// <param name="userEventSearchService">The user event search service helper dependency injection</param>
    /// <param name="userEventsHelper">The user events helper dependency injection</param>
    /// <param name="categoryHelper">Category helper for fetching based on Ids, binding category names to events</param>
    public EventController(
        ILogger<EventController> logger,
        TelemetryClient telemetryClient,
        IUserEventSearchService userEventSearchService,
        IUserEventsHelper userEventsHelper,
        ICategoryHelper categoryHelper)
        : base(telemetryClient: telemetryClient)
    {
        this.logger = logger;
        this.userEventSearchService = userEventSearchService;
        this.userEventsHelper = userEventsHelper;
        this.categoryHelper = categoryHelper;
    }

    /// <summary>
    /// Get event details.
    /// </summary>
    /// <param name="eventId">Event Id for which details needs to be fetched.</param>
    /// <param name="teamId">Team Id with which event is associated.</param>
    /// <returns>Event details.</returns>
    [HttpGet]
    public async Task<IActionResult> GetEventAsync(
        string eventId,
        string teamId)
    {
        this.RecordEvent(eventName: "Get event- The HTTP POST call to get event details has been initiated", payload: new Dictionary<string, string>
        {
            { "eventId", eventId },
            { "teamId", teamId },
        });

        if (string.IsNullOrEmpty(value: eventId))
        {
            this.logger.LogError(message: "Event Id is either null or empty");
            this.RecordEvent(eventName: "Get event- The HTTP POST call to get event details has been failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            return this.BadRequest(error: new { message = "Event Id is null or empty" });
        }

        if (string.IsNullOrEmpty(value: teamId))
        {
            this.logger.LogError(message: "Team Id is either null or empty");
            this.RecordEvent(eventName: "Get event- The HTTP POST call to get event details has been failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            return this.BadRequest(error: new { message = "Team Id is null or empty" });
        }

        try
        {
            var eventDetails = await this.userEventsHelper.GetEventAsync(eventId: eventId, teamId: teamId, userObjectId: this.UserAadId);
            await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });

            this.RecordEvent(eventName: "Get event- The HTTP POST call to get event details has been succeeded", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            return this.Ok(value: eventDetails);
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get event- The HTTP POST call to get event details has been failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            this.logger.LogError(exception: ex, message: $"Error occurred while fetching event details for event Id {eventId} team Id {teamId}");
            throw;
        }
    }

    /// <summary>
    /// Get user events as per user search text and filters
    /// </summary>
    /// <param name="searchString">Search string entered by user.</param>
    /// <param name="pageCount">>Page count for which post needs to be fetched.</param>
    /// <param name="eventSearchType">Event search operation type. Refer <see cref="EventSearchType" /> for values.</param>
    /// <param name="createdByFilter">Semicolon separated user AAD object identifier who created events.</param>
    /// <param name="categoryFilter">Semicolon separated category Ids.</param>
    /// <param name="sortBy">0 for recent and 1 for popular events. Refer <see cref="SortBy" /> for values.</param>
    /// <returns>List of user events</returns>
    [HttpGet(template: "UserEvents")]
    public async Task<IActionResult> GetEventsAsync(
        string searchString,
        int pageCount,
        int eventSearchType,
        string createdByFilter,
        string categoryFilter,
        int sortBy)
    {
        this.RecordEvent(eventName: "Get user events- The HTTP GET call to get user events has initiated");

        if (!Enum.IsDefined(enumType: typeof(EventSearchType), value: eventSearchType))
        {
            this.logger.LogError(message: "Invalid event search type");
            this.RecordEvent(eventName: "Get user events- The HTTP GET call to get user events has failed");
            return this.BadRequest(error: new ErrorResponse { Message = "The event search type was invalid" });
        }

        if (!Enum.IsDefined(enumType: typeof(SortBy), value: sortBy))
        {
            this.logger.LogError(message: "Invalid sort by value");
            this.RecordEvent(eventName: "Get user events- The HTTP GET call to get user events has failed");
            return this.BadRequest(error: new ErrorResponse { Message = "Provided sort by value was invalid" });
        }

        try
        {
            var userEvents = await this.userEventsHelper.GetEventsAsync(
                searchString: searchString, pageCount: pageCount, eventSearchType: eventSearchType, userObjectId: this.UserAadId, createdByFilter: createdByFilter, categoryFilter: categoryFilter, sortBy: sortBy);

            this.RecordEvent(eventName: "Get user events- The HTTP GET call to get user events has succeeded");

            if (userEvents.IsNullOrEmpty())
            {
                return this.Ok(value: new List<EventEntity>());
            }

            await this.categoryHelper.BindCategoryNameAsync(events: userEvents);
            return this.Ok(value: userEvents);
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Get user events- The HTTP GET call to get user events has failed");
            this.logger.LogError(exception: ex, message: "Error occurred while fetching user events");
            throw;
        }
    }

    /// <summary>
    /// Search event as per user search input.
    /// </summary>
    /// <param name="search">Search string entered by user.</param>
    /// <returns>Event details.</returns>
    [HttpGet(template: "search-by-title")]
    public async Task<IActionResult> SearchEventAsync(string search)
    {
        this.RecordEvent(eventName: "Search event- The HTTP POST call to search event details has been initiated");

        if (string.IsNullOrEmpty(value: search))
        {
            this.logger.LogError(message: "Search query is either null or empty");
            return this.BadRequest(error: new ErrorResponse { Message = "Search query is either null or empty" });
        }

        try
        {
            var searchParametersDto = new SearchParametersDto
            {
                SearchString = search,
                SearchScope = EventSearchType.SearchByName,
                UserObjectId = this.UserAadId,
            };
            var searchedEvents = await this.userEventSearchService.GetEventsAsync(searchParametersDto: searchParametersDto);

            this.RecordEvent(eventName: "Search event- The HTTP POST call to search event details has been succeeded");
            return this.Ok(value: searchedEvents);
        }
        catch (Exception ex)
        {
            this.RecordEvent(eventName: "Search event- The HTTP POST call to search event details has been failed");
            this.logger.LogError(exception: ex, message: "Error occurred while searching event");
            throw;
        }
    }

    /// <summary>
    /// Registers the user for an event
    /// </summary>
    /// <param name="teamId">The LnD team Id who created the event</param>
    /// <param name="eventId">The event Id</param>
    /// <returns>Returns true if registration done successfully. Else returns false.</returns>
    [HttpPost(template: "RegisterToEvent")]
    public async Task<IActionResult> RegisterToEventAsync(
        string teamId,
        string eventId)
    {
        this.RecordEvent(eventName: "Register to event- The HTTP POST call to register user for an event has initiated", payload: new Dictionary<string, string>
        {
            { "eventId", eventId },
            { "teamId", teamId },
        });

        if (string.IsNullOrEmpty(value: teamId))
        {
            this.logger.LogError(message: "Invalid team Id was provided");
            this.RecordEvent(eventName: "Register to event- The HTTP POST call to register user for an event has failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            return this.BadRequest(error: new ErrorResponse { Message = "Invalid team Id was provided" });
        }

        if (string.IsNullOrEmpty(value: eventId))
        {
            this.logger.LogError(message: "Invalid event Id was provided");
            this.RecordEvent(eventName: "Register to event- The HTTP POST call to register user for an event has failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            return this.BadRequest(error: new ErrorResponse { Message = "Invalid event Id was provided" });
        }

        try
        {
            var isRegistrationSuccessful = await this.userEventsHelper.RegisterToEventAsync(teamId: teamId, eventId: eventId, userAADObjectId: this.UserAadId);

            this.RecordEvent(eventName: "Register to event- The HTTP POST call to register user for an event has succeeded", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            return this.Ok(value: isRegistrationSuccessful);
        }
        catch (Exception ex)
        {
            this.logger.LogError(exception: ex, message: $"Error occurred while registering user {this.UserAadId} for event {eventId}");
            this.RecordEvent(eventName: "Register to event- The HTTP POST call to register user for an event has failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            throw;
        }
    }

    /// <summary>
    /// Unregisters the user for an event
    /// </summary>
    /// <param name="teamId">The LnD team Id who created the event</param>
    /// <param name="eventId">The event Id</param>
    /// <returns>Returns true if the user successfully unregistered for an event. Else returns false.</returns>
    [HttpPost(template: "UnregisterToEvent")]
    public async Task<IActionResult> UnregisterToEventAsync(
        string teamId,
        string eventId)
    {
        this.RecordEvent(eventName: "Unregister to event- The HTTP POST call to unregister user to an event has initiated", payload: new Dictionary<string, string>
        {
            { "eventId", eventId },
            { "teamId", teamId },
        });

        if (string.IsNullOrEmpty(value: teamId))
        {
            this.logger.LogError(message: "Invalid team Id was provided");
            this.RecordEvent(eventName: "Invalid team Id was provided");
            return this.BadRequest(error: new ErrorResponse { Message = "Invalid team Id was provided" });
        }

        if (string.IsNullOrEmpty(value: eventId))
        {
            this.logger.LogError(message: "Invalid event Id was provided");
            this.RecordEvent(eventName: "Invalid event Id was provided");
            return this.BadRequest(error: new ErrorResponse { Message = "Invalid event Id was provided" });
        }

        try
        {
            var isUserRemovedFromEvent = await this.userEventsHelper.UnregisterFromEventAsync(teamId: teamId, eventId: eventId, userAADObjectId: this.UserAadId);

            this.RecordEvent(eventName: "Unregister to event- The HTTP POST call to unregister user to an event has succeeded", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            return this.Ok(value: isUserRemovedFromEvent);
        }
        catch (Exception ex)
        {
            this.logger.LogError(exception: ex, message: $"Error occurred while unregistering user {this.UserAadId} for event {eventId}");
            this.RecordEvent(eventName: "Unregister to event- The HTTP POST call to unregister user to an event has failed", payload: new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });
            throw;
        }
    }
}