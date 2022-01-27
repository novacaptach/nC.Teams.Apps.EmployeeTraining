// <copyright file="AuthenticationMetadataController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

/// <summary>
/// Controller for sign in authentication data.
/// </summary>
[Route(template: "api/authenticationMetadata")]
public class AuthenticationMetadataController : ControllerBase
{
    /// <summary>
    /// Represents a set of key/value application configuration properties for Azure.
    /// </summary>
    private readonly IOptions<AzureSettings> azureOptions;

    /// <summary>
    /// Represents a set of key/value application configuration properties for bot.
    /// </summary>
    private readonly IOptions<BotSettings> botOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationMetadataController" /> class.
    /// </summary>
    /// <param name="azureOptions">A set of key/value application configuration properties for Azure.</param>
    /// <param name="botOptions">A set of key/value application configuration properties for bot.</param>
    public AuthenticationMetadataController(
        IOptions<AzureSettings> azureOptions,
        IOptions<BotSettings> botOptions)
    {
        this.azureOptions = azureOptions ?? throw new ArgumentNullException(paramName: nameof(azureOptions));
        this.botOptions = botOptions ?? throw new ArgumentNullException(paramName: nameof(botOptions));
    }

    /// <summary>
    /// Get authentication consent Url.
    /// </summary>
    /// <param name="windowLocationOriginDomain">Window location origin domain.</param>
    /// <param name="loginHint">User Principal Name value.</param>
    /// <returns>Conset Url.</returns>
    [HttpGet(template: "consentUrl")]
    public string GetConsentUrl(
        [FromQuery] string windowLocationOriginDomain,
        [FromQuery] string loginHint)
    {
        var consentUrlComponentDictionary = new Dictionary<string, string>
        {
            [key: "redirect_uri"] = $"https://{HttpUtility.UrlDecode(str: windowLocationOriginDomain)}/signin-simple-end",
            [key: "client_id"] = this.botOptions.Value.MicrosoftAppId,
            [key: "response_type"] = "id_token",
            [key: "response_mode"] = "fragment",
            [key: "scope"] = "https://graph.microsoft.com/User.Read openid profile",
            [key: "nonce"] = Guid.NewGuid().ToString(),
            [key: "state"] = Guid.NewGuid().ToString(),
            [key: "login_hint"] = loginHint,
        };

        var consentUrlComponentList = consentUrlComponentDictionary
            .Select(p => $"{p.Key}={HttpUtility.UrlEncode(str: p.Value)}")
            .ToList();

        var consentUrlPrefix = $"https://login.microsoftonline.com/{this.azureOptions.Value.TenantId}/oauth2/v2.0/authorize?";
        var consentUrlString = consentUrlPrefix + string.Join(separator: '&', values: consentUrlComponentList);

        return consentUrlString;
    }
}