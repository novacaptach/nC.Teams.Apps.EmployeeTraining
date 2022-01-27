// <copyright file="EventDetailsCard.cs" company="Microsoft">
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
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;

/// <summary>
/// Creates adaptive card attachment.
/// </summary>
public static class EventDetailsCard
{
    /// <summary>
    /// Create adaptive card attachment for a team which needs to be sent after creating new event.
    /// </summary>
    /// <param name="applicationBasePath">Base URL of application.</param>
    /// <param name="localizer">String localizer for localizing user facing text.</param>
    /// <param name="eventEntity">Event details of newly created event.</param>
    /// <param name="createdByName">Name of person who created event.</param>
    /// <returns>An adaptive card attachment.</returns>
    public static Attachment GetEventCreationCardForTeam(
        string applicationBasePath,
        IStringLocalizer<Strings> localizer,
        EventEntity eventEntity,
        string createdByName)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

        var lnDTeamCard = new AdaptiveCard(schemaVersion: new AdaptiveSchemaVersion(major: 1, minor: 2))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Height = AdaptiveHeight.Auto,
                            Width = AdaptiveColumnWidth.Auto,
                            Items = !string.IsNullOrEmpty(value: eventEntity.Photo)
                                ? new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(uriString: eventEntity.Photo),
                                        HorizontalAlignment = textAlignment,
                                        PixelHeight = 45,
                                        PixelWidth = 45,
                                    },
                                }
                                : new List<AdaptiveElement>(),
                        },
                        new ()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = eventEntity.Name,
                                    Size = AdaptiveTextSize.Large,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    HorizontalAlignment = textAlignment,
                                },
                                new AdaptiveTextBlock
                                {
                                    Text = eventEntity.CategoryName,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Small,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Color = AdaptiveTextColor.Warning,
                                    Spacing = AdaptiveSpacing.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.Medium,
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Width = "100px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"**{localizer.GetString(name: "DateAndTimeLabel")}:** ",
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                        new ()
                        {
                            Spacing = AdaptiveSpacing.None,
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = string.Format(provider: CultureInfo.CurrentCulture, format: "{0} {1}-{2}", arg0: "{{DATE(" + eventEntity.StartDate.Value.ToString(format: "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", provider: CultureInfo.InvariantCulture) + ", SHORT)}}", arg1: "{{TIME(" + eventEntity.StartTime.Value.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}", arg2: "{{TIME(" + eventEntity.EndTime.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}"),
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.Small,
                    Columns = eventEntity.Type != (int)EventType.InPerson
                        ? new List<AdaptiveColumn>()
                        : new List<AdaptiveColumn>
                        {
                            new ()
                            {
                                Width = "100px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"**{localizer.GetString(name: "Venue")}:** ",
                                        Wrap = true,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new ()
                            {
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventEntity.Venue,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                },
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.Small,
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Width = "100px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"**{localizer.GetString(name: "DescriptionLabelCard")}:** ",
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                        new ()
                        {
                            Spacing = AdaptiveSpacing.None,
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = eventEntity.Description,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.Small,
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Width = "100px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"**{localizer.GetString(name: "NumberOfRegistrations")}:** ",
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                        new ()
                        {
                            Spacing = AdaptiveSpacing.None,
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = eventEntity.RegisteredAttendeesCount.ToString(provider: CultureInfo.InvariantCulture),
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"{localizer.GetString(name: "CreatedByLabel")} **{createdByName}**",
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveImage
                {
                    IsVisible = eventEntity.Audience == (int)EventAudience.Private,
                    Url = new Uri(uriString: $"{applicationBasePath}/images/Private.png"),
                    PixelWidth = 84,
                    PixelHeight = 32,
                    Spacing = AdaptiveSpacing.Large,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                },
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = localizer.GetString(name: "EditEventCardButton"),
                    Data = new AdaptiveSubmitActionData
                    {
                        MsTeams = new CardAction
                        {
                            Type = "task/fetch",
                            Text = localizer.GetString(name: "EditEventCardButton"),
                        },
                        Command = BotCommands.EditEvent,
                        EventId = eventEntity.EventId,
                        TeamId = eventEntity.TeamId,
                    },
                },
                new AdaptiveSubmitAction
                {
                    Title = localizer.GetString(name: "CloseRegistrationCardButton"),
                    Data = new AdaptiveSubmitActionData
                    {
                        MsTeams = new CardAction
                        {
                            Type = "task/fetch",
                            Text = localizer.GetString(name: "CloseRegistrationCardButton"),
                        },
                        Command = BotCommands.CloseRegistration,
                        EventId = eventEntity.EventId,
                        TeamId = eventEntity.TeamId,
                    },
                },
            },
        };

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = lnDTeamCard,
        };
    }
}