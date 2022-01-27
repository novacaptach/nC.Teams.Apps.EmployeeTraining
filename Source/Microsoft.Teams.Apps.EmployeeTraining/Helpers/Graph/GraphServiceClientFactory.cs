// <copyright file="GraphServiceClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers.Graph;

extern alias BetaLib;
using System;
using System.Threading.Tasks;
using BetaLib::Microsoft.Graph;
#pragma warning disable SA1135 // Referring BETA package of MS Graph SDK.
using Beta = Microsoft.Graph;

#pragma warning restore SA1135 // Referring BETA package of MS Graph SDK.

/// <summary>
/// Provides Microsoft Graph client for API calls.
/// </summary>
public static class GraphServiceClientFactory
{
    /// <summary>
    /// Get Microsoft Graph service client.
    /// </summary>
    /// <param name="acquireAccessToken">Callback method to get access token.</param>
    /// <returns>Microsoft Graph service client instance.</returns>
    public static Beta.GraphServiceClient GetAuthenticatedGraphClient(
        Func<Task<string>> acquireAccessToken)
    {
        return new Beta.GraphServiceClient(authenticationProvider: new CustomAuthenticationProvider(acquireAccessToken: acquireAccessToken));
    }

    /// <summary>
    /// Get Microsoft Graph Beta service client.
    /// </summary>
    /// <param name="acquireAccessToken">Callback method to get access token.</param>
    /// <returns>Microsoft Graph service client instance.</returns>
    public static GraphServiceClient GetAuthenticatedBetaGraphClient(
        Func<Task<string>> acquireAccessToken)
    {
        return new GraphServiceClient(authenticationProvider: new CustomAuthenticationProvider(acquireAccessToken: acquireAccessToken));
    }
}