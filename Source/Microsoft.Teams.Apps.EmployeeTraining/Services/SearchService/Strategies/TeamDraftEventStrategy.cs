// <copyright file="TeamDraftEventStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

using System;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;

/// <summary>
/// Generates filter query to fetch draft events for team.
/// </summary>
public class TeamDraftEventStrategy : IFilterGeneratingStrategy
{
    /// <inheritdoc />
    public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
    {
        searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(paramName: nameof(searchParametersDto), message: "Search parameter is null");

        return $"{nameof(EventEntity.TeamId)} eq '{searchParametersDto.TeamId}' " +
               $"and {nameof(EventEntity.Status)} eq {(int)EventStatus.Draft}";
    }
}