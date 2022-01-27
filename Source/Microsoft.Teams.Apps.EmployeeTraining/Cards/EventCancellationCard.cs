// <copyright file="EventCancellationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards;

using System;
using System.Collections.Generic;
using System.Globalization;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;

/// <summary>
/// Creates cancellation card attachment to be sent after event cancellation.
/// </summary>
public static class EventCancellationCard
{
    /// <summary>
    /// Get adaptive card attachment for event cancellation.
    /// </summary>
    /// <param name="localizer">String localizer for localizing user facing text.</param>
    /// <param name="eventEntity">Event details which is cancelled.</param>
    /// <param name="applicationManifestId">The unique manifest ID for application</param>
    /// <returns>An adaptive card attachment.</returns>
    public static Attachment GetCancellationCard(
        IStringLocalizer<Strings> localizer,
        EventEntity eventEntity,
        string applicationManifestId)
    {
        eventEntity = eventEntity ?? throw new ArgumentNullException(paramName: nameof(eventEntity), message: "Event details cannot be null");
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

        var cancellationCard = new AdaptiveCard(schemaVersion: new AdaptiveSchemaVersion(major: 1, minor: 2))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = $"{localizer.GetString(name: "CancellationHeader")}",
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = $"{localizer.GetString(name: "CancellationCardSubTitle")}",
                    Wrap = true,
                    Size = AdaptiveTextSize.Small,
                    Spacing = AdaptiveSpacing.None,
                    HorizontalAlignment = textAlignment,
                },
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
                                        Size = AdaptiveImageSize.Small,
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
                                    Size = AdaptiveTextSize.Medium,
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
                    Spacing = AdaptiveSpacing.ExtraLarge,
                    Columns = new List<AdaptiveColumn>
                    {
                        new ()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveActionSet
                                {
                                    Actions = new List<AdaptiveAction>
                                    {
                                        new AdaptiveOpenUrlAction
                                        {
                                            Url = new Uri(uriString: $"https://teams.microsoft.com/l/entity/{applicationManifestId}/discover-events"),
                                            Title = $"{localizer.GetString(name: "ViewAllEventsCardButton")}",
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = cancellationCard,
        };
    }
}