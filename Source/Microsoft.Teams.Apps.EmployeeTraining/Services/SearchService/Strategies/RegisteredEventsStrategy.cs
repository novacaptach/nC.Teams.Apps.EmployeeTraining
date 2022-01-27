// <copyright file="RegisteredEventsStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

using System;
using System.Globalization;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;

/// <summary>
/// Generates filter query for fetching registered events for user.
/// </summary>
public class RegisteredEventsStrategy : IFilterGeneratingStrategy
{
    /// <inheritdoc />
    public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
    {
        searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(paramName: nameof(searchParametersDto), message: "Search parameter is null");

        return $"(search.ismatch('{searchParametersDto.UserObjectId}', '{nameof(EventEntity.RegisteredAttendees)}')" +
               $" or search.ismatch('{searchParametersDto.UserObjectId}', '{nameof(EventEntity.AutoRegisteredAttendees)}'))" +
               $" and {nameof(EventEntity.Status)} eq {(int)EventStatus.Active}" +
               $" and {nameof(EventEntity.EndDate)} ge {DateTime.UtcNow.ToString(format: "O", provider: CultureInfo.InvariantCulture)}";
    }
}