// <copyright file="EventRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories.Implementation;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// The event repository class which manages events' data in Azure Table Storage
/// </summary>
public class EventRepository : BaseRepository<EventEntity>, IEventRepository
{
    /// <summary>
    /// Represents the entity name which is used to store events.
    /// </summary>
    private const string EventEntityName = nameof(EventEntity);

    /// <summary>
    /// Initializes a new instance of the <see cref="EventRepository" /> class.
    /// </summary>
    /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
    /// <param name="logger">To send logs to the logger service.</param>
    public EventRepository(
        IOptions<StorageSetting> options,
        ILogger<EventRepository> logger)
        : base(connectionString: options?.Value.ConnectionString, tableName: EventEntityName, logger: logger)
    {
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
        if (string.IsNullOrEmpty(value: teamId))
        {
            throw new ArgumentException(message: "The team Id should have a valid value", paramName: nameof(teamId));
        }

        if (string.IsNullOrEmpty(value: eventId))
        {
            throw new ArgumentException(message: "The event Id should have a valid value", paramName: nameof(eventId));
        }

        await this.EnsureInitializedAsync();
        return await this.GetAsync(partitionKey: teamId, rowKey: eventId);
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

        await this.EnsureInitializedAsync();
        return await this.CreateOrUpdateAsync(entity: eventDetails);
    }

    /// <summary>
    /// This method update event in Azure Table Storage.
    /// </summary>
    /// <param name="eventDetails">The details of an event that needs to be created or updated</param>
    /// <returns>Returns true if event updated successfully. Else, returns false.</returns>
    public async Task<bool> UpdateEventAsync(EventEntity eventDetails)
    {
        if (eventDetails == null)
        {
            throw new ArgumentException(message: "The event details should be provided", paramName: nameof(eventDetails));
        }

        await this.EnsureInitializedAsync();
        return await this.UpdateAsync(entity: eventDetails);
    }
}