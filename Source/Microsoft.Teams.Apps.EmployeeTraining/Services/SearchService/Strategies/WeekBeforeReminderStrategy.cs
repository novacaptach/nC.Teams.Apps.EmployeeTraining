// <copyright file="WeekBeforeReminderStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

using System;
using System.Globalization;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;

/// <summary>
/// Generates filter query for fetching events to send week before notifications.
/// </summary>
public class WeekBeforeReminderStrategy : IFilterGeneratingStrategy
{
    /// <inheritdoc />
    public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
    {
        var startDateForNextWeek = DateTime.UtcNow.Date.AddDays(value: 7).Date;
        var endDateForNextWeek = startDateForNextWeek.AddDays(value: 7).Date;

        return $"{nameof(EventEntity.Status)} eq {(int)EventStatus.Active} and " +
               $"{nameof(EventEntity.StartDate)} ge {startDateForNextWeek.ToString(format: "O", provider: CultureInfo.InvariantCulture)} and " +
               $"{nameof(EventEntity.StartDate)} le {endDateForNextWeek.ToString(format: "O", provider: CultureInfo.InvariantCulture)} and " +
               $"{nameof(EventEntity.RegisteredAttendeesCount)} gt 0";
    }
}