namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;

public class EventStorageProviderFake : IEventRepository
{
    public List<EventEntity> eventEntities;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStorageProvider" /> class.
    /// </summary>
    /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage</param>
    /// <param name="logger">To send logs to the logger service</param>
    public EventStorageProviderFake()
    {
        this.eventEntities = new List<EventEntity>
        {
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                Audience = 3,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-yyy",
                AutoRegisteredAttendees = "a789ca62-1236-4394-9712-523660226b02;a85c1ff9-7381-4721-bb7b-c8d9203d202c",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = DateTime.UtcNow,
                Description = "Teams Event",
                EndDate = DateTime.UtcNow.AddDays(value: 1),
                EndTime = DateTime.UtcNow.AddDays(value: 1),
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Status = 1,
            },
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2com",
                Audience = 3,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                AutoRegisteredAttendees = "",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = new DateTime(year: 2020, month: 09, day: 24),
                Description = "Teams Event",
                EndDate = new DateTime(year: 2020, month: 09, day: 25),
                EndTime = new DateTime(year: 2020, month: 09, day: 25),
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event 1",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Status = 3,
            },
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2pri",
                Audience = 2,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                AutoRegisteredAttendees = "a789ca62-1236-4394-9712-523660226b02;a85c1ff9-7381-4721-bb7b-c8d9203d202c",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-yyy",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = DateTime.UtcNow,
                Description = "Teams Event",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(value: 1),
                EndTime = DateTime.UtcNow.AddDays(value: 1),
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "a789ca62-1236-4394-9712-523660226b02",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Status = 2,
            },
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2act",
                Audience = 3,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                AutoRegisteredAttendees = "a789ca62-1236-4394-9712-523660226b02;a85c1ff9-7381-4721-bb7b-c8d9203d202c",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-yyy",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = DateTime.UtcNow,
                Description = "Teams Event",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(value: 1),
                EndTime = DateTime.UtcNow.AddDays(value: 1),
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "a789ca62-1236-4394-9712-523660226b02",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Status = 2,
                Photo = "https://www.google.com",
                Type = (int)EventType.Teams,
                RegisteredAttendeesCount = 2,
                Venue = "Teams meeting",
                TeamCardActivityId = "teanCardActicityId",
            },
        };
    }

    /// <summary>
    /// Get event details
    /// </summary>
    /// <param name="eventId">Event Id for a event.</param>
    /// <param name="teamId">The team Id of which events needs to be fetched</param>
    /// <returns>A collection of events</returns>
    public async Task<EventEntity> GetEventDetailsAsync(
        string eventId,
        string teamId)
    {
        if (eventId == "activeEventId")
        {
            eventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2act";
        }
        else if (eventId == "privateEventId")
        {
            eventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2pri";
        }
        else if (eventId == "completedEventId")
        {
            eventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2com";
        }
        else
        {
            eventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba";
        }


        teamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-yyy";

        if (string.IsNullOrEmpty(value: teamId))
        {
            throw new ArgumentException(message: "The team Id should have a valid value", paramName: nameof(teamId));
        }

        if (string.IsNullOrEmpty(value: eventId))
        {
            throw new ArgumentException(message: "The event Id should have a valid value", paramName: nameof(eventId));
        }

        var queryResult = this.eventEntities.Where(e => (e.EventId == eventId) && (e.TeamId == teamId));
        return await Task.Run(() => queryResult.FirstOrDefault());
    }

    /// <summary>
    /// This method inserts a new event in Azure Table Storage if it is not already exists. Else updates the existing one.
    /// </summary>
    /// <param name="eventDetails">The details of an event that needs to be created or updated</param>
    /// <returns>Returns true if event created or updated successfully. Else, returns false.</returns>
    public async Task<bool> UpsertEventAsync(EventEntity eventDetails)
    {
        if (eventDetails == null)
        {
            throw new ArgumentException(message: "The event details should be provided", paramName: nameof(eventDetails));
        }

        eventDetails = EventWorkflowHelperData.eventEntity;
        var result = this.eventEntities.FirstOrDefault(e => e.EventId == eventDetails.EventId);

        if (result != null)
        {
            result.Name = eventDetails.Name;
            result.TeamId = eventDetails.TeamId;
            result.MaximumNumberOfParticipants = eventDetails.MaximumNumberOfParticipants;
        }
        else
        {
            this.eventEntities.Add(item: eventDetails);
        }

        var value = true;
        var testValue = await Task.Run(() => value);
        return true;
    }

    public async Task<bool> UpdateEventAsync(EventEntity eventDetails)
    {
        if (eventDetails == null)
        {
            throw new ArgumentException(message: "The event details should be provided", paramName: nameof(eventDetails));
        }

        eventDetails = EventWorkflowHelperData.eventEntity;
        var result = this.eventEntities.FirstOrDefault(e => e.EventId == eventDetails.EventId);

        if (result != null)
        {
            result.Name = eventDetails.Name;
            result.TeamId = eventDetails.TeamId;
            result.MaximumNumberOfParticipants = eventDetails.MaximumNumberOfParticipants;
        }
        else
        {
            return false;
        }

        var value = true;
        var testValue = await Task.Run(() => value);
        return true;
    }
}