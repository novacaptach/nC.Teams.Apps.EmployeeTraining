// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards;

using System;
using System.Collections.Generic;
using System.Globalization;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Common;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;

/// <summary>
/// Class that helps to return welcome card as attachment.
/// </summary>
public static class WelcomeCard
{
    /// <summary>
    /// Get welcome card attachment to show on Microsoft Teams channel scope.
    /// </summary>
    /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
    /// <param name="localizer">The current cultures' string localizer.</param>
    /// <returns>Team's welcome card as attachment.</returns>
    public static Attachment GetWelcomeCardAttachmentForTeam(
        string applicationBasePath,
        IStringLocalizer<Strings> localizer)
    {
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
        var card = new AdaptiveCard(schemaVersion: new AdaptiveSchemaVersion(major: 1, minor: 2))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Width = AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri(uriString: $"{applicationBasePath}/images/logo.png"),
                                    Size = AdaptiveImageSize.Medium,
                                },
                            },
                        },
                        new ()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Spacing = AdaptiveSpacing.None,
                                    Text = localizer.GetString(name: "WelcomeCardTitle"),
                                    Wrap = true,
                                    HorizontalAlignment = textAlignment,
                                },
                                new AdaptiveTextBlock
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = localizer.GetString(name: "WelcomeCardTeamIntro"),
                                    Wrap = true,
                                    IsSubtle = true,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                            Width = AdaptiveColumnWidth.Stretch,
                        },
                    },
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardTeamHeading"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Text = localizer.GetString(name: "WelcomeCardTeamPoint1"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardTeamPoint2"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardTeamPoint3"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardTeamPoint4"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Text = string.Format(provider: CultureInfo.CurrentCulture, format: localizer.GetString(name: "WelcomeCardTeamContentFooter"), arg0: localizer.GetString(name: "CreateEventButtonWelcomeCard")),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = localizer.GetString(name: "CreateEventButtonWelcomeCard"),
                    Data = new AdaptiveSubmitActionData
                    {
                        MsTeams = new CardAction
                        {
                            Type = "task/fetch",
                            Text = localizer.GetString(name: "CreateEventButtonWelcomeCard"),
                        },
                        Command = BotCommands.CreateEvent,
                    },
                },
            },
        };

        var adaptiveCardAttachment = new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = card,
        };

        return adaptiveCardAttachment;
    }

    /// <summary>
    /// Get welcome card attachment to show on Microsoft Teams personal scope.
    /// </summary>
    /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
    /// <param name="localizer">The current cultures' string localizer.</param>
    /// <param name="applicationManifestId">Application manifest id.</param>
    /// <returns>User welcome card attachment.</returns>
    public static Attachment GetWelcomeCardAttachmentForPersonal(
        string applicationBasePath,
        IStringLocalizer<Strings> localizer,
        string applicationManifestId)
    {
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
        var card = new AdaptiveCard(schemaVersion: new AdaptiveSchemaVersion(major: 1, minor: 2))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Width = AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri(uriString: $"{applicationBasePath}/images/logo.png"),
                                    Size = AdaptiveImageSize.Medium,
                                },
                            },
                        },
                        new ()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Spacing = AdaptiveSpacing.None,
                                    Text = localizer.GetString(name: "WelcomeCardTitle"),
                                    Wrap = true,
                                    HorizontalAlignment = textAlignment,
                                },
                                new AdaptiveTextBlock
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = localizer.GetString(name: "WelcomeCardPersonalIntro"),
                                    Wrap = true,
                                    IsSubtle = true,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                            Width = AdaptiveColumnWidth.Stretch,
                        },
                    },
                },
                new AdaptiveTextBlock
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Text = localizer.GetString(name: "WelcomeCardPersonalPoint1"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardPersonalPoint2"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardPersonalPoint3"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = localizer.GetString(name: "WelcomeCardPersonalPoint4"),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Text = string.Format(provider: CultureInfo.CurrentCulture, format: localizer.GetString(name: "WelcomeCardPersonalContentFooter"), arg0: localizer.GetString(name: "WelcomeCardPersonalDiscoverButtonText")),
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveOpenUrlAction
                {
                    Url = new Uri(uriString: $"https://teams.microsoft.com/l/entity/{applicationManifestId}/discover-events"),
                    Title = $"{localizer.GetString(name: "WelcomeCardPersonalDiscoverButtonText")}",
                },
            },
        };
        var adaptiveCardAttachment = new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = card,
        };

        return adaptiveCardAttachment;
    }
}