namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;

public class LnDTeamConfigurationStorageProviderFake : ILnDTeamConfigurationRepository
{
    public List<LnDTeam> lnDTeams;

    public LnDTeamConfigurationStorageProviderFake()
    {
        this.lnDTeams = new List<LnDTeam>
        {
            new ()
            {
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-3355",
            },
            new ()
            {
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee111",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-3355-eee",
            },
            new ()
            {
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee222",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-3355-erty",
            },
            new ()
            {
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baee333e",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-3355-poui",
            },
        };
    }

    /// <summary>
    /// Delete LnD team configuration details when LnD team uninstalls a Bot
    /// </summary>
    /// <param name="teamDetails">The LnD team details which needs to be deleted.</param>
    /// <returns>Returns true if configuration details deleted successfully. Else returns false.</returns>
    public async Task<bool> DeleteLnDTeamConfigurationsAsync(LnDTeam teamDetails)
    {
        if (teamDetails == null)
        {
            throw new ArgumentNullException(paramName: nameof(teamDetails), message: "The team Id should have a valid value");
        }

        this.lnDTeams.Remove(item: teamDetails);
        return await Task.Run(() => true);
    }

    /// <summary>
    /// Get all events of a team
    /// </summary>
    /// <param name="teamId">The team Id of which events needs to be fetched</param>
    /// <returns>A collection of events</returns>
    public async Task<LnDTeam> GetTeamDetailsAsync(string teamId)
    {
        if (string.IsNullOrEmpty(value: teamId))
        {
            throw new ArgumentException(message: "The team Id should have a valid value", paramName: nameof(teamId));
        }

        var queryResult = this.lnDTeams.FirstOrDefault(lnDTeams => lnDTeams.TeamId == teamId);
        return await Task.Run(() => queryResult);
    }

    /// <summary>
    /// Gets all LnD teams
    /// </summary>
    /// <returns>Returns list of LnD teams</returns>
    public async Task<IEnumerable<LnDTeam>> GetTeamsAsync()
    {
        var lnDTeams = this.lnDTeams;
        return await Task.Run(() => lnDTeams);
    }

    /// <summary>
    /// Insert a new LnD team configuration details when LnD team installs a Bot
    /// </summary>
    /// <param name="teamDetails">The LnD team configuration details</param>
    /// <returns>Returns true if configuration details inserted successfully. Else returns false.</returns>
    public async Task<bool> InsertLnDTeamConfigurationAsync(LnDTeam teamDetails)
    {
        if (teamDetails == null)
        {
            throw new ArgumentNullException(paramName: nameof(teamDetails), message: "The team details should be provided");
        }

        this.lnDTeams.Add(item: teamDetails);
        return await Task.Run(() => true);
    }
}