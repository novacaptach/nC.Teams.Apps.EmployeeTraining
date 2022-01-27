// <copyright file="TeamCategoryEventsStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

using System;
using Microsoft.Teams.Apps.EmployeeTraining.Models;

/// <summary>
/// Generates filter query to fetch events related to category for team.
/// </summary>
public class TeamCategoryEventsStrategy : IFilterGeneratingStrategy
{
    /// <inheritdoc />
    public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
    {
        searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(paramName: nameof(searchParametersDto), message: "Search parameter is null");

        return $"{nameof(EventEntity.CategoryId)} eq '{searchParametersDto.CategoryId}'";
    }
}