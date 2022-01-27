// <copyright file="EventGraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Graph;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers.Graph;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using EventType = Microsoft.Teams.Apps.EmployeeTraining.Models.Enums.EventType;
using GraphServiceClient = BetaLib::Microsoft.Graph.GraphServiceClient;
#pragma warning disable SA1135 // Referring BETA package of MS Graph SDK.
using Beta = Microsoft.Graph;

#pragma warning restore SA1135 // Referring BETA package of MS Graph SDK.

/// <summary>
/// Implements the methods that are defined in <see cref="IEventGraphHelper" />.
/// </summary>
public class EventGraphHelper : IEventGraphHelper
{
    /// <summary>
    /// Instance of BETA graph service client for application level requests.
    /// </summary>
    private readonly GraphServiceClient applicationBetaGraphClient;

    /// <summary>
    /// Instance of graph service client for application level requests.
    /// </summary>
    private readonly Beta.GraphServiceClient applicationGraphClient;

    /// <summary>
    /// Instance of graph service client for delegated requests.
    /// </summary>
    private readonly Beta.GraphServiceClient delegatedGraphClient;

    /// <summary>
    /// The current culture's string localizer.
    /// </summary>
    private readonly IStringLocalizer<Strings> localizer;

    /// <summary>
    /// Graph helper for operations related user.
    /// </summary>
    private readonly IUserGraphHelper userGraphHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventGraphHelper" /> class.
    /// </summary>
    /// <param name="tokenAcquisitionHelper">Helper to get user access token for specified Graph scopes.</param>
    /// <param name="httpContextAccessor">HTTP context accessor for getting user claims.</param>
    /// <param name="localizer">The current culture's string localizer.</param>
    /// <param name="userGraphHelper">Graph helper for operations related user.</param>
    public EventGraphHelper(
        ITokenAcquisitionHelper tokenAcquisitionHelper,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<Strings> localizer,
        IUserGraphHelper userGraphHelper)
    {
        this.localizer = localizer;
        this.userGraphHelper = userGraphHelper;
        httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(paramName: nameof(httpContextAccessor));

        var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        var userObjectId = httpContextAccessor.HttpContext.User.Claims?
            .FirstOrDefault(claim => oidClaimType.Equals(value: claim.Type, comparisonType: StringComparison.OrdinalIgnoreCase))?.Value;

        if (!string.IsNullOrEmpty(value: userObjectId))
        {
            var jwtToken = AuthenticationHeaderValue.Parse(input: httpContextAccessor.HttpContext.Request.Headers[key: "Authorization"].ToString()).Parameter;

            this.delegatedGraphClient = GraphServiceClientFactory.GetAuthenticatedGraphClient(async () => { return await tokenAcquisitionHelper.GetUserAccessTokenAsync(userAadId: userObjectId, jwtToken: jwtToken); });

            this.applicationBetaGraphClient = GraphServiceClientFactory.GetAuthenticatedBetaGraphClient(async () => { return await tokenAcquisitionHelper.GetApplicationAccessTokenAsync(); });

            this.applicationGraphClient = GraphServiceClientFactory.GetAuthenticatedGraphClient(async () => { return await tokenAcquisitionHelper.GetApplicationAccessTokenAsync(); });
        }
    }

    /// <summary>
    /// Cancel calendar event.
    /// </summary>
    /// <param name="eventGraphId">Event Id received from Graph.</param>
    /// <param name="createdByUserId">User Id who created event.</param>
    /// <param name="comment">Cancellation comment.</param>
    /// <returns>True if event cancellation is successful.</returns>
    public async Task<bool> CancelEventAsync(
        string eventGraphId,
        string createdByUserId,
        string comment)
    {
        await this.applicationBetaGraphClient.Users[id: createdByUserId].Events[id: eventGraphId]
            .Cancel(Comment: comment).Request().PostAsync();

        return true;
    }

    /// <summary>
    /// Create teams event.
    /// </summary>
    /// <param name="eventEntity">Event details from user for which event needs to be created.</param>
    /// <returns>Created event details.</returns>
    public async Task<Event> CreateEventAsync(EventEntity eventEntity)
    {
        eventEntity = eventEntity ??
                      throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");

        var teamsEvent = new Event
        {
            Subject = eventEntity.Name,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = this.GetEventBodyContent(eventEntity: eventEntity),
            },
            Attendees = eventEntity.IsAutoRegister && (eventEntity.Audience == (int)EventAudience.Private) ? await this.GetEventAttendeesTemplateAsync(eventEntity: eventEntity) : new List<Attendee>(),
            OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
            IsReminderOn = true,
            Location = eventEntity.Type == (int)EventType.InPerson
                ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                : null,
            AllowNewTimeProposals = false,
            IsOnlineMeeting = eventEntity.Type == (int)EventType.Teams,
            OnlineMeetingProvider = eventEntity.Type == (int)EventType.Teams ? OnlineMeetingProviderType.TeamsForBusiness : OnlineMeetingProviderType.Unknown,
        };

        teamsEvent.Start = new DateTimeTimeZone
        {
            DateTime = eventEntity.StartDate?.ToString(format: "s", provider: CultureInfo.InvariantCulture),
            TimeZone = TimeZoneInfo.Utc.Id,
        };
        teamsEvent.End = new DateTimeTimeZone
        {
            DateTime = eventEntity.StartDate.Value.Date.Add(
                value: new TimeSpan(hours: eventEntity.EndTime.Hour, minutes: eventEntity.EndTime.Minute, seconds: eventEntity.EndTime.Second)).ToString(format: "s", provider: CultureInfo.InvariantCulture),
            TimeZone = TimeZoneInfo.Utc.Id,
        };

        if (eventEntity.NumberOfOccurrences > 1)
        {
            // Create recurring event.
            teamsEvent = this.GetRecurringEventTemplate(teamsEvent: teamsEvent, eventEntity: eventEntity);
        }

        return await this.delegatedGraphClient.Me.Events.Request()
            .Header(name: "Prefer", value: $"outlook.timezone=\"{TimeZoneInfo.Utc.Id}\"").AddAsync(eventsEvent: teamsEvent);
    }

    /// <summary>
    /// Update teams event.
    /// </summary>
    /// <param name="eventEntity">Event details from user for which event needs to be updated.</param>
    /// <returns>Updated event details.</returns>
    public async Task<Event> UpdateEventAsync(EventEntity eventEntity)
    {
        eventEntity = eventEntity ??
                      throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");

        var teamsEvent = new Event
        {
            Subject = eventEntity.Name,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = this.GetEventBodyContent(eventEntity: eventEntity),
            },
            Attendees = await this.GetEventAttendeesTemplateAsync(eventEntity: eventEntity),
            OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
            IsReminderOn = true,
            Location = eventEntity.Type == (int)EventType.InPerson
                ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                : null,
            AllowNewTimeProposals = false,
            IsOnlineMeeting = eventEntity.Type == (int)EventType.Teams,
            OnlineMeetingProvider = eventEntity.Type == (int)EventType.Teams ? OnlineMeetingProviderType.TeamsForBusiness : OnlineMeetingProviderType.Unknown,
        };

        teamsEvent.Start = new DateTimeTimeZone
        {
            DateTime = eventEntity.StartDate?.ToString(format: "s", provider: CultureInfo.InvariantCulture),
            TimeZone = TimeZoneInfo.Utc.Id,
        };
        teamsEvent.End = new DateTimeTimeZone
        {
            DateTime = eventEntity.StartDate.Value.Date.Add(
                value: new TimeSpan(hours: eventEntity.EndTime.Hour, minutes: eventEntity.EndTime.Minute, seconds: eventEntity.EndTime.Second)).ToString(format: "s", provider: CultureInfo.InvariantCulture),
            TimeZone = TimeZoneInfo.Utc.Id,
        };

        if (eventEntity.NumberOfOccurrences > 1)
        {
            // Create recurring event.
            teamsEvent = this.GetRecurringEventTemplate(teamsEvent: teamsEvent, eventEntity: eventEntity);
        }

        return await this.applicationGraphClient.Users[id: eventEntity.CreatedBy].Events[id: eventEntity.GraphEventId].Request()
            .Header(name: "Prefer", value: $"outlook.timezone=\"{TimeZoneInfo.Utc.Id}\"").UpdateAsync(eventToUpdate: teamsEvent);
    }

    /// <summary>
    /// Modify event details for recurring event creation.
    /// </summary>
    /// <param name="teamsEvent">Event details which will be sent to Graph API.</param>
    /// <param name="eventEntity">Event details from user for which event needs to be created.</param>
    /// <returns>Event details to be sent to Graph API.</returns>
    private Event GetRecurringEventTemplate(
        Event teamsEvent,
        EventEntity eventEntity)
    {
        // Create recurring event.
        teamsEvent.Recurrence = new PatternedRecurrence
        {
            Pattern = new RecurrencePattern
            {
                Type = RecurrencePatternType.Daily,
                Interval = 1,
            },
            Range = new RecurrenceRange
            {
                Type = RecurrenceRangeType.EndDate,
                EndDate = new Date(year: (int)eventEntity.EndDate?.Year, month: (int)eventEntity.EndDate?.Month, day: (int)eventEntity.EndDate?.Day),
                StartDate = new Date(year: (int)eventEntity.StartDate?.Year, month: (int)eventEntity.StartDate?.Month, day: (int)eventEntity.StartDate?.Day),
            },
        };

        return teamsEvent;
    }

    /// <summary>
    /// Get list of event attendees for creating teams event.
    /// </summary>
    /// <param name="eventEntity">Event details containing registered attendees.</param>
    /// <returns>List of attendees.</returns>
    private async Task<List<Attendee>> GetEventAttendeesTemplateAsync(EventEntity eventEntity)
    {
        var attendees = new List<Attendee>();

        if (string.IsNullOrEmpty(value: eventEntity.RegisteredAttendees) && string.IsNullOrEmpty(value: eventEntity.AutoRegisteredAttendees))
        {
            return attendees;
        }

        if (!string.IsNullOrEmpty(value: eventEntity.RegisteredAttendees))
        {
            var registeredAttendeesList = eventEntity.RegisteredAttendees.Trim().Split(separator: ";");
            if (registeredAttendeesList.Any())
            {
                var userProfiles = await this.userGraphHelper.GetUsersAsync(userObjectIds: registeredAttendeesList);

                foreach (var userProfile in userProfiles)
                {
                    attendees.Add(item: new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = userProfile.UserPrincipalName,
                            Name = userProfile.DisplayName,
                        },
                        Type = AttendeeType.Required,
                    });
                }
            }
        }

        if (!string.IsNullOrEmpty(value: eventEntity.AutoRegisteredAttendees))
        {
            var autoRegisteredAttendeesList = eventEntity.AutoRegisteredAttendees.Trim().Split(separator: ";");
            if (autoRegisteredAttendeesList.Any())
            {
                var userProfiles = await this.userGraphHelper.GetUsersAsync(userObjectIds: autoRegisteredAttendeesList);

                foreach (var userProfile in userProfiles)
                {
                    attendees.Add(item: new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = userProfile.UserPrincipalName,
                            Name = userProfile.DisplayName,
                        },
                        Type = AttendeeType.Required,
                    });
                }
            }
        }

        return attendees;
    }

    /// <summary>
    /// Get the event body content based on event type
    /// </summary>
    /// <param name="eventEntity">The event details</param>
    /// <returns>Returns </returns>
    private string GetEventBodyContent(EventEntity eventEntity)
    {
        switch ((EventType)eventEntity.Type)
        {
            case EventType.InPerson:
                return $"{HttpUtility.HtmlEncode(s: eventEntity.Description)}<br/><br/>{HttpUtility.HtmlEncode(s: this.localizer.GetString(name: "CalendarEventLocationText", eventEntity.Venue))}";

            case EventType.LiveEvent:
                return $"{HttpUtility.HtmlEncode(s: eventEntity.Description)}<br/><br/>{this.localizer.GetString(name: "CalendarEventLiveEventURLText", $"<a href='{eventEntity.MeetingLink}'>{eventEntity.MeetingLink}</a>")}";

            default:
                return HttpUtility.HtmlEncode(s: eventEntity.Description);
        }
    }
}