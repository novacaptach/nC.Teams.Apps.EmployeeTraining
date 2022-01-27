// <copyright file="LnDTeamConfigurationRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories.Implementation;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// This class manages LnD team's configuration operations in storage.
/// </summary>
public class LnDTeamConfigurationRepository : BaseRepository<LnDTeam>, ILnDTeamConfigurationRepository
{
    /// <summary>
    /// Represents the entity name which is used to store LnD team configurations.
    /// </summary>
    private const string LnDTeamConfiguration = "LnDTeamConfiguration";

    /// <summary>
    /// Initializes a new instance of the <see cref="LnDTeamConfigurationRepository" /> class.
    /// </summary>
    /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
    /// <param name="logger">To send logs to the logger service.</param>
    public LnDTeamConfigurationRepository(
        IOptions<StorageSetting> options,
        ILogger<LnDTeamConfigurationRepository> logger)
        : base(connectionString: options?.Value.ConnectionString, tableName: LnDTeamConfiguration, logger: logger)
    {
    }

    /// <summary>
    /// Delete LnD team configuration details when LnD team uninstalls a Bot.
    /// </summary>
    /// <param name="teamDetails">The LnD team details which needs to be deleted.</param>
    /// <returns>Returns true if configuration details deleted successfully. Else returns false.</returns>
    public async Task<bool> DeleteLnDTeamConfigurationsAsync(LnDTeam teamDetails)
    {
        if (teamDetails == null)
        {
            throw new ArgumentNullException(paramName: nameof(teamDetails), message: "The team Id should have a valid value");
        }

        await this.EnsureInitializedAsync();
        return await this.DeleteAsync(entity: teamDetails);
    }

    /// <summary>
    /// Get team details by Id.
    /// </summary>
    /// <param name="teamId">The team Id for which details needs to be fetched.</param>
    /// <returns>Team details.</returns>
    public async Task<LnDTeam> GetTeamDetailsAsync(string teamId)
    {
        if (string.IsNullOrEmpty(value: teamId))
        {
            throw new ArgumentException(message: "The team Id should have a valid value", paramName: nameof(teamId));
        }

        await this.EnsureInitializedAsync();
        return await this.GetAsync(partitionKey: teamId, rowKey: teamId);
    }

    /// <summary>
    /// Insert a new LnD team configuration details when LnD team installs a Bot.
    /// </summary>
    /// <param name="teamDetails">The LnD team configuration details.</param>
    /// <returns>Returns true if configuration details inserted successfully. Else returns false.</returns>
    public async Task<bool> InsertLnDTeamConfigurationAsync(LnDTeam teamDetails)
    {
        if (teamDetails == null)
        {
            throw new ArgumentNullException(paramName: nameof(teamDetails), message: "The team details should be provided");
        }

        await this.EnsureInitializedAsync();
        return await this.CreateOrUpdateAsync(entity: teamDetails);
    }

    /// <summary>
    /// Gets all LnD teams
    /// </summary>
    /// <returns>Returns list of LnD teams</returns>
    public async Task<IEnumerable<LnDTeam>> GetTeamsAsync()
    {
        await this.EnsureInitializedAsync();
        return await this.ExecuteQueryAsync(query: new TableQuery<LnDTeam>());
    }
}