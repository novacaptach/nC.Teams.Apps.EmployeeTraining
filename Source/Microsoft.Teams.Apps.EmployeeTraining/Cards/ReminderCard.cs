// <copyright file="ReminderCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;

/// <summary>
/// Holds the method which returns reminder card
/// </summary>
public static class ReminderCard
{
    /// <summary>
    /// Gets the reminder card with event details
    /// </summary>
    /// <param name="events">The list of events</param>
    /// <param name="localizer">The localizer for localizing content</param>
    /// <param name="applicationManifestId">Unique manifest Id used for side-loading app</param>
    /// <param name="notificationType">The type of notification being sent</param>
    /// <returns>If event details provided, then returns reminder card. Else returns empty card.</returns>
    public static Attachment GetCard(
        IEnumerable<EventEntity> events,
        IStringLocalizer<Strings> localizer,
        string applicationManifestId,
        NotificationType notificationType = NotificationType.Manual)
    {
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
        if ((events == null) || !events.Any())
        {
            return new Attachment();
        }

        var cardTitle = string.Empty;

        switch (notificationType)
        {
            case NotificationType.Daily:
                cardTitle = localizer.GetString(name: "DailyReminderCardTitle");
                break;

            case NotificationType.Weekly:
                cardTitle = localizer.GetString(name: "WeeklyReminderCardTitle");
                break;

            default:
                cardTitle = localizer.GetString(name: "ReminderCardTitle");
                break;
        }

        var cardBody = new List<AdaptiveElement>
        {
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
                                Text = cardTitle,
                                Wrap = true,
                                Size = AdaptiveTextSize.Large,
                                Weight = AdaptiveTextWeight.Bolder,
                                HorizontalAlignment = textAlignment,
                            },
                        },
                    },
                },
            },
        };

        cardBody.AddRange(collection: GetReminderCardElements(events: events, localizer: localizer).Select(cardElement => cardElement));

        var reminderCard = new AdaptiveCard(schemaVersion: new AdaptiveSchemaVersion(major: 1, minor: 2))
        {
            Body = cardBody,
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveOpenUrlAction
                {
                    Title = $"{localizer.GetString(name: "ReminderCardRegisteredEventButton")}",
                    Url = new Uri(uriString: $"https://teams.microsoft.com/l/entity/{applicationManifestId}/my-events"), // Open My events tab (deep link).
                },
            },
        };

        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = reminderCard,
        };
    }

    /// <summary>
    /// Gets reminder card elements
    /// </summary>
    /// <param name="events">The list of events</param>
    /// <param name="localizer">The localizer for localizing content</param>
    /// <returns>Returns reminder card elements</returns>
    private static List<AdaptiveElement> GetReminderCardElements(
        IEnumerable<EventEntity> events,
        IStringLocalizer<Strings> localizer)
    {
        var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
        var cardElements = new List<AdaptiveElement>();

        foreach (var eventDetails in events)
        {
            var adaptiveColumnSet = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                {
                    new ()
                    {
                        Width = "45px",
                        PixelMinHeight = 45,
                        Items = !string.IsNullOrEmpty(value: eventDetails.Photo)
                            ? new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri(uriString: eventDetails.Photo),
                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
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
                                Text = eventDetails.Name,
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Small,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveColumnSet
                            {
                                Spacing = AdaptiveSpacing.None,
                                Columns = new List<AdaptiveColumn>
                                {
                                    new ()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = eventDetails.CategoryName,
                                                Wrap = true,
                                                Color = AdaptiveTextColor.Warning,
                                                Size = AdaptiveTextSize.Small,
                                                HorizontalAlignment = textAlignment,
                                            },
                                        },
                                    },
                                    new ()
                                    {
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = "| " + (eventDetails.Type == (int)EventType.InPerson ? eventDetails.Venue : localizer.GetString(name: "TeamsMeetingText")),
                                                Wrap = true,
                                                HorizontalAlignment = textAlignment,
                                                Size = AdaptiveTextSize.Small,
                                            },
                                        },
                                    },
                                },
                            },
                            new AdaptiveColumnSet
                            {
                                Spacing = AdaptiveSpacing.None,
                                Columns = new List<AdaptiveColumn>
                                {
                                    new ()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = string.Format(provider: CultureInfo.CurrentCulture, format: "{0} {1}-{2}", arg0: "{{DATE(" + eventDetails.StartDate.Value.ToString(format: "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", provider: CultureInfo.InvariantCulture) + ", SHORT)}}", arg1: "{{TIME(" + eventDetails.StartTime.Value.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}", arg2: "{{TIME(" + eventDetails.EndTime.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}"),
                                                Wrap = true,
                                                Size = AdaptiveTextSize.Small,
                                                HorizontalAlignment = textAlignment,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            cardElements.Add(item: adaptiveColumnSet);
        }

        return cardElements;
    }
}