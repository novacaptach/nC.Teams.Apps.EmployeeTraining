// <copyright file="EventWorkflowHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Teams.Apps.EmployeeTraining.Cards;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

/// <summary>
/// Helper for event CRUD operations.
/// </summary>
public class EventWorkflowHelper : IEventWorkflowHelper
{
    /// <summary>
    /// A set of key/value application configuration properties for activity settings.
    /// </summary>
    private readonly IOptions<BotSettings> botOptions;

    /// <summary>
    /// Category helper for fetching based on Ids, binding category names to events
    /// </summary>
    private readonly ICategoryHelper categoryHelper;

    /// <summary>
    /// Graph API helper for events.
    /// </summary>
    private readonly IEventGraphHelper eventGraphHelper;

    /// <summary>
    /// Provides the methods for event related operations on storage.
    /// </summary>
    private readonly IEventRepository eventRepository;

    /// <summary>
    /// The event search service provider for event table.
    /// </summary>
    private readonly IEventSearchService eventSearchService;

    /// <summary>
    /// Graph API helper for groups.
    /// </summary>
    private readonly IGroupGraphHelper groupGraphHelper;

    /// <summary>
    /// The current culture's string localizer
    /// </summary>
    private readonly IStringLocalizer<Strings> localizer;

    /// <summary>
    /// The notification helper for notification activities.
    /// </summary>
    private readonly INotificationHelper notificationHelper;

    /// <summary>
    /// Retry policy with linear backoff, retry twice with a jitter delay of up to 1 sec. Retry for HTTP 412(precondition
    /// failed).
    /// </summary>
    /// <remarks>
    /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
    /// </remarks>
    private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<StorageException>(ex => ex.RequestInformation.HttpStatusCode == StatusCodes.Status412PreconditionFailed)
        .WaitAndRetryAsync(sleepDurations: Backoff.LinearBackoff(initialDelay: TimeSpan.FromMilliseconds(value: 250), retryCount: 25));

    /// <summary>
    /// The repository for LnD team configuration related operations on storage
    /// </summary>
    private readonly ILnDTeamConfigurationRepository teamConfigurationRepository;

    /// <summary>
    /// The repository for user configuration related operations on storage
    /// </summary>
    private readonly IUserConfigurationRepository userConfigurationRepository;

    /// <summary>
    /// Graph API helper for users API.
    /// </summary>
    private readonly IUserGraphHelper userGraphHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventWorkflowHelper" /> class.
    /// </summary>
    /// <param name="eventRepository">Provides the methods for event related operations on storage.</param>
    /// <param name="eventSearchService">The event search service for event table.</param>
    /// <param name="eventGraphHelper">Graph API helper for events.</param>
    /// <param name="groupGraphHelper">Graph API helper for groups.</param>
    /// <param name="userConfigurationRepository">Provides the methods for user configuration operations on storage.</param>
    /// <param name="teamConfigurationRepository">Provides the methods for LnD team configuration operations on storage.</param>
    /// <param name="categoryHelper">Category helper for fetching based on Ids, binding category names to events.</param>
    /// <param name="localizer">The current culture's string localizer.</param>
    /// <param name="userGraphHelper">Graph API helper for users API.</param>
    /// <param name="notificationHelper">The notification helper for notification activities.</param>
    /// <param name="botOptions">A set of key/value application configuration properties for activity settings.</param>
    public EventWorkflowHelper(
        IEventRepository eventRepository,
        IEventSearchService eventSearchService,
        IEventGraphHelper eventGraphHelper,
        IGroupGraphHelper groupGraphHelper,
        IUserConfigurationRepository userConfigurationRepository,
        ILnDTeamConfigurationRepository teamConfigurationRepository,
        ICategoryHelper categoryHelper,
        IStringLocalizer<Strings> localizer,
        IUserGraphHelper userGraphHelper,
        INotificationHelper notificationHelper,
        IOptions<BotSettings> botOptions)
    {
        this.eventRepository = eventRepository;
        this.eventSearchService = eventSearchService;
        this.eventGraphHelper = eventGraphHelper;
        this.groupGraphHelper = groupGraphHelper;
        this.userConfigurationRepository = userConfigurationRepository;
        this.teamConfigurationRepository = teamConfigurationRepository;
        this.categoryHelper = categoryHelper;
        this.localizer = localizer;
        this.userGraphHelper = userGraphHelper;
        this.notificationHelper = notificationHelper;
        this.botOptions = botOptions;
    }

    /// <summary>
    /// Create new event as draft.
    /// </summary>
    /// <param name="eventEntity">Event details to be saved as draft.</param>
    /// <returns>Boolean indicating save operation result.</returns>
    public async Task<bool> CreateDraftEventAsync(EventEntity eventEntity)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");

        var draftEvent = new EventEntity();
        this.MapEventModel(sourceEventEntity: eventEntity, destinationEventEntity: draftEvent, isEdit: false, isDraft: true);

        var result = await this.eventRepository.UpsertEventAsync(eventDetails: draftEvent);
        if (result)
        {
            await this.eventSearchService.RunIndexerOnDemandAsync();
        }

        return result;
    }

    /// <summary>
    /// Update draft event.
    /// </summary>
    /// <param name="eventEntity">Draft event details to be updated.</param>
    /// <returns>Boolean indicating save operation result.</returns>
    public async Task<bool?> UpdateDraftEventAsync(EventEntity eventEntity)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");

        var eventToUpdate = await this.eventRepository.GetEventDetailsAsync(eventId: eventEntity.EventId, teamId: eventEntity.TeamId);

        if ((eventToUpdate == null) || eventToUpdate.IsRemoved)
        {
            return null;
        }

        this.MapEventModel(sourceEventEntity: eventEntity, destinationEventEntity: eventToUpdate, isEdit: true, isDraft: false);

        var result = await this.eventRepository.UpdateEventAsync(eventDetails: eventToUpdate);
        if (result)
        {
            await this.eventSearchService.RunIndexerOnDemandAsync();
        }

        return result;
    }

    /// <summary>
    /// Create new event as draft.
    /// </summary>
    /// <param name="eventEntity">Event details to be saved as draft.</param>
    /// <param name="createdByName">Name of person who created event.</param>
    /// <returns>Boolean indicating save operation result.</returns>
    public async Task<bool?> CreateNewEventAsync(
        EventEntity eventEntity,
        string createdByName)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");

        var eventToUpsert = eventEntity;
        if (!string.IsNullOrEmpty(value: eventEntity.EventId))
        {
            // If event is saved as draft, change status.
            eventToUpsert = await this.eventRepository.GetEventDetailsAsync(eventId: eventEntity.EventId, teamId: eventEntity.TeamId);

            if ((eventToUpsert == null) || eventToUpsert.IsRemoved || (eventToUpsert.Status != (int)EventStatus.Draft))
            {
                return null;
            }

            // Update existing event record in storage.
            this.MapEventModel(sourceEventEntity: eventEntity, destinationEventEntity: eventToUpsert, isEdit: true, isDraft: false);
        }
        else
        {
            eventToUpsert = new EventEntity();

            // Create new event record in storage.
            this.MapEventModel(sourceEventEntity: eventEntity, destinationEventEntity: eventToUpsert, isEdit: false, isDraft: false);
        }

        // Calculate occurrences as per start and end date.
        eventToUpsert.NumberOfOccurrences = Convert.ToInt32(value: eventToUpsert.EndDate.Value.Subtract(value: eventToUpsert.StartDate.Value).TotalDays) + 1;

        // Set time of selected start date to start time.
        eventToUpsert.StartDate = eventEntity.StartDate?.Date.Add(
            value: new TimeSpan(hours: (int)eventEntity.StartTime?.Hour, minutes: (int)eventEntity.StartTime?.Minute, seconds: (int)eventEntity.StartTime?.Second));

        // Set time of selected end date to end time.
        eventToUpsert.EndDate = eventEntity.EndDate?.Date.Add(
            value: new TimeSpan(hours: eventEntity.EndTime.Hour, minutes: eventEntity.EndTime.Minute, seconds: eventEntity.EndTime.Second));

        // Validate selected users for event in case of private audience type.
        if (!string.IsNullOrEmpty(value: eventToUpsert.SelectedUserOrGroupListJSON) && (eventToUpsert.Audience == (int)EventAudience.Private))
        {
            eventToUpsert = await this.ValidateMandatoryAndOptionalUsers(eventEntity: eventToUpsert);
        }

        // Create event using MS Graph.
        var graphEventResult = await this.eventGraphHelper.CreateEventAsync(eventEntity: eventToUpsert);

        if (graphEventResult == null)
        {
            return false;
        }

        var activityId = await this.SendEventCreationNotificationInTeam(eventDetails: eventToUpsert, createdByName: createdByName);

        eventToUpsert.Status = (int)EventStatus.Active;
        eventToUpsert.GraphEventId = graphEventResult.Id;
        eventToUpsert.TeamCardActivityId = activityId;

        // Insert or update event record in storage.
        var result = await this.eventRepository.UpsertEventAsync(eventDetails: eventToUpsert);
        if (result)
        {
            if (eventToUpsert.IsAutoRegister && !string.IsNullOrEmpty(value: eventToUpsert.AutoRegisteredAttendees))
            {
                await this.SendAutoregisteredNotificationAsync(eventDetails: eventToUpsert, userIds: eventToUpsert.AutoRegisteredAttendees.Split(separator: ";").ToList());
            }

            // Run indexer on demand so that record will be available for search service.
            await this.eventSearchService.RunIndexerOnDemandAsync();
        }

        return result;
    }

    /// <summary>
    /// Update an event.
    /// </summary>
    /// <param name="eventEntity">Event details to be updated.</param>
    /// <returns>Boolean indicating save operation result.</returns>
    public async Task<bool?> UpdateEventAsync(EventEntity eventEntity)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");
        if (string.IsNullOrEmpty(value: eventEntity.EventId))
        {
            return null;
        }

        // Retry policy to handle consistency while updating event.
        return await this.retryPolicy.ExecuteAsync<bool?>(async () =>
        {
            var eventToUpsert = await this.eventRepository.GetEventDetailsAsync(eventId: eventEntity.EventId, teamId: eventEntity.TeamId);
            var existingAutoRegisteredAttendees = string.IsNullOrEmpty(value: eventToUpsert.AutoRegisteredAttendees) ? new List<string>() : eventToUpsert.AutoRegisteredAttendees.Split(separator: ";").ToList();

            if ((eventToUpsert == null) || string.IsNullOrEmpty(value: eventToUpsert.GraphEventId) || eventToUpsert.IsRemoved || (eventToUpsert.Status != (int)EventStatus.Active))
            {
                return null;
            }

            // Map properties which are allowed to be edited by user.
            this.MapEventModel(sourceEventEntity: eventEntity, destinationEventEntity: eventToUpsert, isEdit: true, isDraft: false);

            // Calculate occurrences as per start and end date.
            eventToUpsert.NumberOfOccurrences = Convert.ToInt32(value: eventToUpsert.EndDate.Value.Subtract(value: eventToUpsert.StartDate.Value).TotalDays) + 1;

            // Set time of selected start date to start time.
            eventToUpsert.StartDate = eventEntity.StartDate?.Date.Add(
                value: new TimeSpan(hours: (int)eventEntity.StartTime?.Hour, minutes: (int)eventEntity.StartTime?.Minute, seconds: (int)eventEntity.StartTime?.Second));

            // Set time of selected end date to end time.
            eventToUpsert.EndDate = eventEntity.EndDate?.Date.Add(
                value: new TimeSpan(hours: eventEntity.EndTime.Hour, minutes: eventEntity.EndTime.Minute, seconds: eventEntity.EndTime.Second));

            // Validate selected users for event in case of private audience type.
            if (!string.IsNullOrEmpty(value: eventToUpsert.SelectedUserOrGroupListJSON) && (eventToUpsert.Audience == (int)EventAudience.Private))
            {
                eventToUpsert = await this.ValidateMandatoryAndOptionalUsers(eventEntity: eventToUpsert);
            }

            // Create event using MS Graph.
            var graphEventResult = await this.eventGraphHelper.UpdateEventAsync(eventEntity: eventToUpsert);

            if (graphEventResult == null)
            {
                return false;
            }

            eventToUpsert.Status = (int)EventStatus.Active;

            // Insert or update event record in storage.
            var result = await this.eventRepository.UpdateEventAsync(eventDetails: eventToUpsert);
            if (result)
            {
                var createdByName = await this.userGraphHelper.GetUserAsync(userObjectId: eventToUpsert.CreatedBy);
                await this.SendEventCreationNotificationInTeam(eventDetails: eventToUpsert, createdByName: createdByName?.DisplayName);
                await this.SendEventUpdateNotificationAsync(eventDetails: eventToUpsert);

                if (eventToUpsert.IsAutoRegister && !string.IsNullOrEmpty(value: eventToUpsert.AutoRegisteredAttendees))
                {
                    var currentMandatoryUsers = string.IsNullOrEmpty(value: eventToUpsert.AutoRegisteredAttendees) ? new List<string>() : eventToUpsert.AutoRegisteredAttendees.Split(separator: ";").ToList();
                    var usersToNotify = currentMandatoryUsers.Except(second: existingAutoRegisteredAttendees).ToList();
                    await this.SendAutoregisteredNotificationAsync(eventDetails: eventToUpsert, userIds: usersToNotify);
                }

                // Run indexer on demand so that record will be available for search service.
                await this.eventSearchService.RunIndexerOnDemandAsync();
            }

            return result;
        });
    }

    /// <summary>
    /// Delete draft event.
    /// </summary>
    /// <param name="teamId">Team Id by which event was created.</param>
    /// <param name="eventId">Event Id of event which needs to be deleted.</param>
    /// <returns>Boolean indicating delete operation result.</returns>
    public async Task<bool?> DeleteDraftEventAsync(
        string teamId,
        string eventId)
    {
        teamId = teamId ?? throw new ArgumentNullException(paramName: nameof(teamId), message: "Team Id cannot be null or empty");
        eventId = eventId ?? throw new ArgumentNullException(paramName: nameof(eventId), message: "Event Id cannot be null or empty");

        // Retry policy to handle consistency while updating event.
        return await this.retryPolicy.ExecuteAsync<bool?>(async () =>
        {
            var eventToUpdate = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

            if ((eventToUpdate == null) || eventToUpdate.IsRemoved)
            {
                return null;
            }

            eventToUpdate.IsRemoved = true;

            var result = await this.eventRepository.UpdateEventAsync(eventDetails: eventToUpdate);

            if (result)
            {
                await this.eventSearchService.RunIndexerOnDemandAsync();
            }

            return result;
        });
    }

    /// <summary>
    /// Closes event registrations
    /// </summary>
    /// <param name="teamId">The LnD team Id</param>
    /// <param name="eventId">The event Id of which registrations to be closed</param>
    /// <param name="userAadId">The logged-in user's AAD Id</param>
    /// <returns>Returns true if event registrations closed successfully. Else returns false.</returns>
    public async Task<bool> CloseEventRegistrationsAsync(
        string teamId,
        string eventId,
        string userAadId)
    {
        // Retry policy to handle consistency while updating event.
        return await this.retryPolicy.ExecuteAsync(async () =>
        {
            var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

            if (eventDetails?.Status != (int)EventStatus.Active)
            {
                return false;
            }

            eventDetails.IsRegistrationClosed = true;
            eventDetails.UpdatedOn = DateTime.UtcNow;
            eventDetails.UpdatedBy = userAadId;

            var result = await this.eventRepository.UpdateEventAsync(eventDetails: eventDetails);

            if (result)
            {
                await this.eventSearchService.RunIndexerOnDemandAsync();
            }

            return result;
        });
    }

    /// <summary>
    /// Updates the event status
    /// </summary>
    /// <param name="teamId">The LnD team Id</param>
    /// <param name="eventId">The event Id of which status to change</param>
    /// <param name="eventStatus">The event status to change</param>
    /// <param name="userAadId">The logged-in user's AAD Id</param>
    /// <returns>Returns true if event status updated successfully. Else returns false.</returns>
    public async Task<bool> UpdateEventStatusAsync(
        string teamId,
        string eventId,
        EventStatus eventStatus,
        string userAadId)
    {
        // Retry policy to handle consistency while updating event.
        return await this.retryPolicy.ExecuteAsync(async () =>
        {
            var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

            if ((eventDetails == null) || !this.IsValidEventStatusToChange(currentStatus: (EventStatus)eventDetails.Status, statusToChange: eventStatus))
            {
                return false;
            }

            if (eventStatus == EventStatus.Cancelled)
            {
                await this.eventGraphHelper.CancelEventAsync(eventGraphId: eventDetails.GraphEventId, createdByUserId: eventDetails.CreatedBy, comment: this.localizer.GetString(name: "CancelEventComment"));
                await this.SendCancellationNotificationAsync(teamId: teamId, eventId: eventId);
            }

            eventDetails.Status = (int)eventStatus;
            eventDetails.UpdatedOn = DateTime.UtcNow;
            eventDetails.UpdatedBy = userAadId;

            var result = await this.eventRepository.UpdateEventAsync(eventDetails: eventDetails);

            if (result)
            {
                await this.eventSearchService.RunIndexerOnDemandAsync();
            }

            return result;
        });
    }

    /// <summary>
    /// Export event details to CSV
    /// </summary>
    /// <param name="teamId">The LnD team Id</param>
    /// <param name="eventId">The event Id of which details needs to be exported</param>
    /// <returns>Returns CSV data in stream</returns>
    public async Task<Stream> ExportEventDetailsToCSVAsync(
        string teamId,
        string eventId)
    {
        var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

        if (eventDetails == null)
        {
            return null;
        }

        await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });

        var csvColumns = new List<string>
        {
            this.localizer.GetString(name: "EventName"),
            this.localizer.GetString(name: "EventDescription"),
            this.localizer.GetString(name: "Category"),
            this.localizer.GetString(name: "TrainingType"),
            this.localizer.GetString(name: "Venue"),
            this.localizer.GetString(name: "NumberOfRegistrations"),
            this.localizer.GetString(name: "StartDate"),
            this.localizer.GetString(name: "EndDate"),
            this.localizer.GetString(name: "Audience"),
            this.localizer.GetString(name: "RegisteredUsers"),
        };

        var stream = new MemoryStream();

        await using var writer = new StreamWriter(stream: stream, encoding: Encoding.UTF8);
        await writer.WriteAsync(value: string.Join(separator: ",", value: csvColumns.Select(column => $"\"{column}\"").ToArray()));
        await writer.WriteLineAsync();

        var csvRows = new List<List<object>>();

        if (eventDetails.RegisteredAttendeesCount > 0)
        {
            var attendees = new List<string>();
            var eventAttendees = await this.userGraphHelper.GetUsersAsync(userObjectIds: eventDetails.GetAttendees());

            if (!eventAttendees.IsNullOrEmpty())
            {
                attendees.AddRange(collection: eventAttendees.Select(user => $"{user.DisplayName} ({user.UserPrincipalName})"));
            }

            attendees = attendees.OrderBy(user => user).ToList();

            csvRows.Add(item: new List<object>
            {
                eventDetails.Name,
                eventDetails.Description,
                eventDetails.CategoryName,
                this.GetTrainingTypeLocalizedString(eventType: eventDetails.Type),
                eventDetails.Type == (int)EventType.InPerson ? eventDetails.Venue : this.localizer.GetString(name: "TeamsMeetingText"),
                eventDetails.RegisteredAttendeesCount,
                eventDetails.StartDate,
                eventDetails.EndDate,
                this.GetEventAudienceLocalizedString(audience: eventDetails.Audience),
                attendees.First(),
            });

            await writer.WriteAsync(value: string.Join(separator: ",", values: csvRows.First().Select(cellValue => $"\"{cellValue}\"")));
            await writer.WriteLineAsync();

            for (var i = 1; i < attendees.Count; i++)
            {
                csvRows.Add(item: new List<object>
                {
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    attendees[index: i],
                });

                await writer.WriteAsync(value: string.Join(separator: ",", values: csvRows[index: i].Select(cellValue => $"\"{cellValue}\"")));
                await writer.WriteLineAsync();
            }
        }
        else
        {
            csvRows.Add(item: new List<object>
            {
                eventDetails.Name,
                eventDetails.Description,
                eventDetails.CategoryName,
                this.GetTrainingTypeLocalizedString(eventType: eventDetails.Type),
                eventDetails.Type == (int)EventType.InPerson ? eventDetails.Venue : this.localizer.GetString(name: "TeamsMeetingText"),
                eventDetails.RegisteredAttendeesCount,
                eventDetails.StartDate,
                eventDetails.EndDate,
                this.GetEventAudienceLocalizedString(audience: eventDetails.Audience),
                string.Empty,
            });

            await writer.WriteAsync(value: string.Join(separator: ",", values: csvRows.First().Select(cellValue => $"\"{cellValue}\"")));
        }

        await writer.FlushAsync();
        stream.Position = 0;

        return stream;
    }

    /// <summary>
    /// Sends reminder to the registered users for an event
    /// </summary>
    /// <param name="teamId">The LnD team Id</param>
    /// <param name="eventId">The event Id for which notification to send</param>
    /// <returns>Returns the list of user Ids to whom notification send was failed</returns>
    public async Task SendReminderAsync(
        string teamId,
        string eventId)
    {
        var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

        if ((eventDetails == null) || (eventDetails.RegisteredAttendeesCount == 0))
        {
            return;
        }

        var registeredAttendees = await this.userConfigurationRepository.GetUserConfigurationsAsync(userAADObjectIds: eventDetails.GetAttendees());

        await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });

        var notificationCard = ReminderCard.GetCard(events: new List<EventEntity> { eventDetails }, localizer: this.localizer, applicationManifestId: this.botOptions.Value.ManifestId);

        await this.notificationHelper.SendNotificationToUsersAsync(users: registeredAttendees, card: notificationCard);
    }

    /// <summary>
    /// Decides whether event's current status can be changed to the provided one
    /// </summary>
    /// <param name="currentStatus">The current status of an event</param>
    /// <param name="statusToChange">The event status that to be changed to</param>
    /// <returns>Returns true if status change request is valid. Else returns false.</returns>
    private bool IsValidEventStatusToChange(
        EventStatus currentStatus,
        EventStatus statusToChange)
    {
        switch (currentStatus)
        {
            case EventStatus.Draft:
                return statusToChange == EventStatus.Active;

            case EventStatus.Active:
                return statusToChange != EventStatus.Draft;

            case EventStatus.Cancelled:
            case EventStatus.Completed:
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Sends cancellation notification to the registered users for an event
    /// </summary>
    /// <param name="teamId">The LnD team Id</param>
    /// <param name="eventId">The event Id for which notification to send</param>
    /// <returns>Returns true if notification sent successfully. Else returns false.</returns>
    private async Task<bool> SendCancellationNotificationAsync(
        string teamId,
        string eventId)
    {
        var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId: eventId, teamId: teamId);

        if ((eventDetails == null) || (eventDetails.RegisteredAttendeesCount == 0))
        {
            return false;
        }

        var users = new List<string>();

        if (!string.IsNullOrEmpty(value: eventDetails.RegisteredAttendees))
        {
            users.AddRange(collection: eventDetails.RegisteredAttendees.Split(separator: ";"));
        }

        if (!string.IsNullOrEmpty(value: eventDetails.AutoRegisteredAttendees))
        {
            users.AddRange(collection: eventDetails.AutoRegisteredAttendees.Split(separator: ";"));
        }

        await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });

        var registeredAttendees = await this.userConfigurationRepository.GetUserConfigurationsAsync(userAADObjectIds: users);

        var notificationCard = EventCancellationCard.GetCancellationCard(localizer: this.localizer, eventEntity: eventDetails, applicationManifestId: this.botOptions.Value.ManifestId);

        await this.notificationHelper.SendNotificationToUsersAsync(users: registeredAttendees, card: notificationCard);

        return true;
    }

    /// <summary>
    /// Map event properties for updating and creating event. Pickicking properties which are allowed to modify by user
    /// eliminates risk of unnecessary modifications.
    /// </summary>
    /// <param name="sourceEventEntity">Event details received from user.</param>
    /// <param name="destinationEventEntity">Event details to be saved in database.</param>
    /// <param name="isEdit">Boolean indicating whether mapping has to be done for event edit operation.</param>
    /// <param name="isDraft">Boolean indicating whether mapping has to be done for draft event.</param>
    private void MapEventModel(
        EventEntity sourceEventEntity,
        EventEntity destinationEventEntity,
        bool isEdit,
        bool isDraft)
    {
        // If DB contains 'Private' audience type and user modified it to 'Public' then clear registered, mandatory and optional user information.
        if ((sourceEventEntity.Audience == (int)EventAudience.Public) && (destinationEventEntity.Audience != sourceEventEntity.Audience))
        {
            destinationEventEntity.MandatoryAttendees = string.Empty;
            destinationEventEntity.OptionalAttendees = string.Empty;
            destinationEventEntity.IsAutoRegister = false;
            destinationEventEntity.AutoRegisteredAttendees = string.Empty;
            destinationEventEntity.RegisteredAttendees = string.Empty;
            destinationEventEntity.RegisteredAttendeesCount = 0;
            destinationEventEntity.SelectedUserOrGroupListJSON = string.Empty;
        }
        else
        {
            // Copy registration details sent by user as it is.
            destinationEventEntity.IsAutoRegister = sourceEventEntity.IsAutoRegister;
            destinationEventEntity.MandatoryAttendees = sourceEventEntity.MandatoryAttendees;
            destinationEventEntity.OptionalAttendees = sourceEventEntity.OptionalAttendees;
            destinationEventEntity.SelectedUserOrGroupListJSON = sourceEventEntity.SelectedUserOrGroupListJSON;
        }

        // Copy remaining properties which are allowed to be modified by user.
        destinationEventEntity.Audience = sourceEventEntity.Audience;
        destinationEventEntity.CategoryId = sourceEventEntity.CategoryId;
        destinationEventEntity.Description = sourceEventEntity.Description;
        destinationEventEntity.EndTime = sourceEventEntity.EndTime;
        destinationEventEntity.EndDate = sourceEventEntity.EndDate;
        destinationEventEntity.MaximumNumberOfParticipants = sourceEventEntity.MaximumNumberOfParticipants;
        destinationEventEntity.MeetingLink = sourceEventEntity.MeetingLink;
        destinationEventEntity.Name = sourceEventEntity.Name.Trim();
        destinationEventEntity.NumberOfOccurrences = sourceEventEntity.NumberOfOccurrences;
        destinationEventEntity.Photo = sourceEventEntity.Photo;
        destinationEventEntity.SelectedColor = sourceEventEntity.SelectedColor;
        destinationEventEntity.StartDate = sourceEventEntity.StartDate;
        destinationEventEntity.StartTime = sourceEventEntity.StartTime;
        destinationEventEntity.Type = sourceEventEntity.Type;
        destinationEventEntity.UpdatedBy = sourceEventEntity.UpdatedBy;
        destinationEventEntity.UpdatedOn = DateTime.UtcNow;
        destinationEventEntity.Venue = sourceEventEntity.Venue;

        // If user is creating new event, initialize properties with default value.
        if (!isEdit)
        {
            destinationEventEntity.IsRegistrationClosed = false;
            destinationEventEntity.CreatedOn = DateTime.UtcNow;
            destinationEventEntity.EventId = string.IsNullOrEmpty(value: sourceEventEntity.EventId) ? Guid.NewGuid().ToString() : sourceEventEntity.EventId;
            destinationEventEntity.GraphEventId = null;
            destinationEventEntity.RegisteredAttendees = null;
            destinationEventEntity.RegisteredAttendeesCount = 0;
            destinationEventEntity.Status = isDraft ? (int)EventStatus.Draft : (int)EventStatus.Active;
            destinationEventEntity.IsRemoved = false;
            destinationEventEntity.CreatedBy = sourceEventEntity.CreatedBy;
            destinationEventEntity.TeamId = sourceEventEntity.TeamId;
        }
    }

    /// <summary>
    /// Sends registration notification to the auto registered users for an event.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    /// <param name="userIds">Users eligible for notification.</param>
    /// <returns>Returns true if notification sent successfully. Else returns false.</returns>
    private async Task<bool> SendAutoregisteredNotificationAsync(
        EventEntity eventDetails,
        List<string> userIds)
    {
        if ((eventDetails == null) || (eventDetails.RegisteredAttendeesCount == 0))
        {
            return false;
        }

        if (userIds.Any())
        {
            await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });
            var registeredAttendees = await this.userConfigurationRepository.GetUserConfigurationsAsync(userAADObjectIds: userIds);

            if (!registeredAttendees.IsNullOrEmpty())
            {
                var notificationCard = AutoRegisteredCard.GetAutoRegisteredCard(applicationBasePath: this.botOptions.Value.AppBaseUri, localizer: this.localizer, eventEntity: eventDetails, applicationManifestId: this.botOptions.Value.ManifestId);

                await this.notificationHelper.SendNotificationToUsersAsync(users: registeredAttendees, card: notificationCard);
            }
        }

        return true;
    }

    /// <summary>
    /// Sends notification in team after event in created
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    /// <param name="createdByName">Name of person who created event.</param>
    /// <returns>Returns true if notification sent successfully. Else returns false.</returns>
    private async Task<string> SendEventCreationNotificationInTeam(
        EventEntity eventDetails,
        string createdByName)
    {
        if (eventDetails == null)
        {
            return null;
        }

        await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });
        var teamDetails = await this.teamConfigurationRepository.GetTeamDetailsAsync(teamId: eventDetails.TeamId);

        var notificationCard = EventDetailsCard.GetEventCreationCardForTeam(applicationBasePath: this.botOptions.Value.AppBaseUri, localizer: this.localizer, eventEntity: eventDetails, createdByName: createdByName);

        if (string.IsNullOrEmpty(value: eventDetails.TeamCardActivityId))
        {
            return await this.notificationHelper.SendNotificationInTeamAsync(team: teamDetails, card: notificationCard);
        }

        return await this.notificationHelper.SendNotificationInTeamAsync(team: teamDetails, card: notificationCard, updateCard: true, activityId: eventDetails.TeamCardActivityId);
    }

    /// <summary>
    /// Validates entries in mandatory and optional user columns.
    /// </summary>
    /// <param name="eventEntity">Event entity to be saved.</param>
    /// <returns>Modified event details.</returns>
    private async Task<EventEntity> ValidateMandatoryAndOptionalUsers(EventEntity eventEntity)
    {
        var userMandatory = new List<string>();
        var userOptional = new List<string>();
        var groupMandatory = new List<string>();
        var groupOptional = new List<string>();

        var finalMandtoryUserIds = new List<string>();
        var finalOptionalUserIds = new List<string>();

        // Selected groups and users are stored as JSON to preserve selection state which is used in case of edit event.
        var selectedUsersAndGroups = JsonConvert.DeserializeObject<List<UserGroupSearchResult>>(value: eventEntity.SelectedUserOrGroupListJSON);

        if (!selectedUsersAndGroups.IsNullOrEmpty())
        {
            foreach (var selectedUserOrGroup in selectedUsersAndGroups)
            {
                if (selectedUserOrGroup != null)
                {
                    if (selectedUserOrGroup.IsGroup)
                    {
                        if (!string.IsNullOrEmpty(value: selectedUserOrGroup.Id))
                        {
                            // If selected entity is group then get members of groups.
                            var members = await this.groupGraphHelper.GetGroupMembersAsync(groupId: selectedUserOrGroup.Id);
                            if (members != null)
                            {
                                var memberIds = members.Select(member => member.Id).ToList();

                                // If group is marked mandatory then add all members of group in mandatory list.
                                if (!selectedUserOrGroup.IsMandatory)
                                {
                                    groupOptional.AddRange(collection: memberIds);
                                }
                                else
                                {
                                    groupMandatory.AddRange(collection: memberIds);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If user is marked mandatory then add in mandatory list.
                        if (!selectedUserOrGroup.IsMandatory)
                        {
                            userOptional.Add(item: selectedUserOrGroup.Id);
                        }
                        else
                        {
                            userMandatory.Add(item: selectedUserOrGroup.Id);
                        }
                    }
                }
            }

            // Remove duplicates
            userMandatory = userMandatory.Distinct().ToList();
            userOptional = userOptional.Distinct().ToList();
            groupMandatory = groupMandatory.Distinct().ToList();
            groupOptional = groupOptional.Distinct().ToList();

            // Remove users from optional array if present in both mandatory and optional array (for users). Entity marked as Mandatory has higher precedence.
            userOptional = userOptional.Where(userId => !userMandatory.Contains(item: userId)).ToList();

            // Remove users from optional array if present in both mandatory and optional array (for users in group). Entity marked as Mandatory has higher precedence.
            groupOptional = groupOptional.Where(userId => !groupMandatory.Contains(item: userId)).ToList();

            // Check if any user from group is added again as single user.
            // If true then single user entity's mandatory/optional status will be considered and group's status will be neglected for that user.
            groupOptional = groupOptional.Where(userId => !userMandatory.Contains(item: userId)).ToList();
            groupMandatory = groupMandatory.Where(userId => !userOptional.Contains(item: userId)).ToList();

            // Add group and user mandatory/optional Ids in respective array.
            finalMandtoryUserIds.AddRange(collection: groupMandatory);
            finalMandtoryUserIds.AddRange(collection: userMandatory);
            finalOptionalUserIds.AddRange(collection: groupOptional);
            finalOptionalUserIds.AddRange(collection: userOptional);

            // Remove duplicates after merging group's users and single user Ids
            finalMandtoryUserIds = finalMandtoryUserIds.Distinct().ToList();
            finalOptionalUserIds = finalOptionalUserIds.Distinct().ToList();

            // Generate semi colon separated string of user Ids.
            eventEntity.MandatoryAttendees = string.Join(separator: ';', values: finalMandtoryUserIds);
            eventEntity.OptionalAttendees = string.Join(separator: ';', values: finalOptionalUserIds);

            // If IsAutoRegister is true then selected mandatory users will be added as default registered users.
            if (eventEntity.IsAutoRegister)
            {
                var registeredAttendees = string.IsNullOrEmpty(value: eventEntity.RegisteredAttendees) ? new List<string>() : eventEntity.RegisteredAttendees.Split(separator: ';').ToList();
                var filteredAutoRegisterAttendees = finalMandtoryUserIds.Where(attendee => !registeredAttendees.Contains(item: attendee)).ToList();
                eventEntity.AutoRegisteredAttendees = string.Join(separator: ';', values: filteredAutoRegisterAttendees);
                eventEntity.RegisteredAttendeesCount = filteredAutoRegisterAttendees.Count + registeredAttendees.Count;
            }
        }
        else
        {
            eventEntity.MandatoryAttendees = string.Empty;
            eventEntity.OptionalAttendees = string.Empty;
        }

        return eventEntity;
    }

    /// <summary>
    /// Gets the localized string for event training type
    /// </summary>
    /// <param name="eventType">The event training type</param>
    /// <returns>Returns the localized string for event training type</returns>
    private string GetTrainingTypeLocalizedString(int eventType)
    {
        switch ((EventType)eventType)
        {
            case EventType.InPerson:
                return this.localizer.GetString(name: "TrainingTypeInPerson");

            case EventType.Teams:
                return this.localizer.GetString(name: "TeamsMeetingText");

            case EventType.LiveEvent:
                return this.localizer.GetString(name: "TrainingTypeLiveEvent");

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for event audience
    /// </summary>
    /// <param name="audience">The event audience</param>
    /// <returns>Returns the localized string for event audience</returns>
    private string GetEventAudienceLocalizedString(int audience)
    {
        switch ((EventAudience)audience)
        {
            case EventAudience.Private:
                return this.localizer.GetString(name: "AudiencePrivate");

            case EventAudience.Public:
                return this.localizer.GetString(name: "AudiencePublic");

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Sends event updation notification to the registered users for an event
    /// </summary>
    /// <param name="eventDetails">The event details for which update notification needs to be sent to user</param>
    /// <returns>Returns true if notification sent successfully. Else returns false.</returns>
    private async Task<bool> SendEventUpdateNotificationAsync(EventEntity eventDetails)
    {
        if ((eventDetails == null) || (eventDetails.RegisteredAttendeesCount == 0))
        {
            return false;
        }

        var users = eventDetails.GetAttendees();

        await this.categoryHelper.BindCategoryNameAsync(events: new List<EventEntity> { eventDetails });

        // Get user details from DB using user Ids.
        var registeredAttendees = await this.userConfigurationRepository.GetUserConfigurationsAsync(userAADObjectIds: users);

        var notificationCard = EventUpdateCard.GetEventUpdateCard(localizer: this.localizer, eventEntity: eventDetails, applicationManifestId: this.botOptions.Value.ManifestId);

        await this.notificationHelper.SendNotificationToUsersAsync(users: registeredAttendees, card: notificationCard);

        return true;
    }
}