// <copyright file="TokenAcquisitionHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// Provides methods to fetch user and application access token for Graph scopes.
/// </summary>
public class TokenAcquisitionHelper : ITokenAcquisitionHelper
{
    /// <summary>
    /// Represents application access scopes.
    /// </summary>
    private readonly string[] applicationScopesList =
    {
        "https://graph.microsoft.com/.default",
    };

    /// <summary>
    /// Instance of IOptions to read data from application configuration.
    /// </summary>
    private readonly IOptions<AzureSettings> azureSettings;

    /// <summary>
    /// Instance of confidential client app to access web API.
    /// </summary>
    private readonly IConfidentialClientApplication confidentialClientApp;

    /// <summary>
    /// Represents scopes required by MsalNet for accessing token.
    /// </summary>
    private readonly string[] scopesRequestedByMsalNet =
    {
        "openid",
        "profile",
        "offline_access",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenAcquisitionHelper" /> class.
    /// </summary>
    /// <param name="confidentialClientApp">Instance of ConfidentialClientApplication class.</param>
    /// <param name="botOptions">Instance of IOptions for BotSettings to read data from application configuration.</param>
    public TokenAcquisitionHelper(
        IConfidentialClientApplication confidentialClientApp,
        IOptions<AzureSettings> botOptions)
    {
        this.confidentialClientApp = confidentialClientApp;
        this.azureSettings = botOptions;
    }

    /// <summary>
    /// Get token on behalf of user and add it to cache.
    /// </summary>
    /// <param name="userAadId">Azure AD object identifier for logged in user.</param>
    /// <param name="jwtToken">Id token of user.</param>
    /// <returns>Token with graph scopes.</returns>
    public async Task<string> GetUserAccessTokenAsync(
        string userAadId,
        string jwtToken)
    {
        try
        {
            var scopeList = this.azureSettings.Value.GraphScope.Split(separator: new[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries).ToList();

            // Gets user account from the accounts available in token cache.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientapplicationbase.getaccountasync?view=azure-dotnet
            // Concatenation of UserObjectId and TenantId separated by a dot is used as unique identifier for getting user account.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.accountid.identifier?view=azure-dotnet#Microsoft_Identity_Client_AccountId_Identifier
            var account = await this.confidentialClientApp.GetAccountAsync(identifier: $"{userAadId}.{this.azureSettings.Value.TenantId}");

            // Attempts to acquire an access token for the account from the user token cache.
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientapplicationbase.acquiretokensilent?view=azure-dotnet
            var result = await this.confidentialClientApp
                .AcquireTokenSilent(scopes: scopeList, account: account)
                .ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // If token does no exist in cache then get token on behalf of user.
            return await this.AquireTokenOnBehalfOfUserAsync(graphScopes: this.azureSettings.Value.GraphScope, jwtToken: jwtToken);
        }
    }

    /// <summary>
    /// Get user Azure AD access token.
    /// </summary>
    /// <returns>Access token with Graph scopes.</returns>
    public async Task<string> GetApplicationAccessTokenAsync()
    {
        var result = await this.confidentialClientApp
            .AcquireTokenForClient(scopes: this.applicationScopesList)
            .WithAuthority(authorityUri: $"https://login.microsoftonline.com/{this.azureSettings.Value.TenantId}")
            .ExecuteAsync();

        return result.AccessToken;
    }

    /// <summary>
    /// Get token on behalf of user.
    /// </summary>
    /// <param name="graphScopes">Graph scopes to be added to token.</param>
    /// <param name="jwtToken">JWT bearer token.</param>
    /// <returns>Token with graph scopes.</returns>
    private async Task<string> AquireTokenOnBehalfOfUserAsync(
        string graphScopes,
        string jwtToken)
    {
        graphScopes = graphScopes ?? throw new ArgumentNullException(paramName: nameof(graphScopes));
        jwtToken = jwtToken ?? throw new ArgumentNullException(paramName: nameof(jwtToken));
        var userAssertion = new UserAssertion(assertion: jwtToken, assertionType: "urn:ietf:params:oauth:grant-type:jwt-bearer");
        IEnumerable<string> requestedScopes = graphScopes.Split(separator: new[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries).ToList();

        // Result to make sure that the cache is filled-in before the controller tries to get access tokens
        var result = await this.confidentialClientApp.AcquireTokenOnBehalfOf(
                scopes: requestedScopes.Except(second: this.scopesRequestedByMsalNet),
                userAssertion: userAssertion)
            .ExecuteAsync();
        return result.AccessToken;
    }
}