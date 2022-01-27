// <copyright file="MessagingExtensionCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Common;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;

/// <summary>
/// Holds the method which returns reminder card
/// </summary>
public static class MessagingExtensionCard
{
    /// <summary>
    /// Sets the maximum number of characters for project title.
    /// </summary>
    private const int TitleMaximumLength = 40;

    /// <summary>
    /// Sets the maximum number of characters for project title.
    /// </summary>
    private const int CategoryMaximumLength = 20;

    /// <summary>
    /// Sets the maximum number of characters for project title.
    /// </summary>
    private const int LocationMaximumLength = 30;

    /// <summary>
    /// Get projects result for Messaging Extension.
    /// </summary>
    /// <param name="events">List of user search result.</param>
    /// <param name="applicationBasePath">Application base URL.</param>
    /// <param name="localizer">The localizer for localizing content</param>
    /// <param name="localDateTime">Indicates local date and time of end user.</param>
    /// <returns>If event details provided, then returns reminder card. Else returns empty card.</returns>
    public static MessagingExtensionResult GetCard(
        IEnumerable<EventEntity> events,
        string applicationBasePath,
        IStringLocalizer<Strings> localizer,
        DateTimeOffset? localDateTime)
    {
        events = events ?? throw new ArgumentNullException(paramName: nameof(events), message: "Event list cannot be null");

        var composeExtensionResult = new MessagingExtensionResult
        {
            Type = "result",
            AttachmentLayout = AttachmentLayoutTypes.List,
            Attachments = new List<MessagingExtensionAttachment>(),
        };

        foreach (var eventDetails in events)
        {
            var card = GetEventDetailsAdaptiveCard(eventDetails: eventDetails, localizer: localizer, applicationBasePath: applicationBasePath);

            var previewCard = GetThumbnailCard(eventDetails: eventDetails, localDateTime: localDateTime, localizer: localizer);

            composeExtensionResult.Attachments.Add(item: new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            }.ToMessagingExtensionAttachment(previewAttachment: previewCard.ToAttachment()));
        }

        return composeExtensionResult;
    }

    /// <summary>
    /// Returns local date time for user by adding local timestamp (received from bot activity) offset to targeted date.
    /// </summary>
    /// <param name="dateTime">The date and time which needs to be converted to user local time.</param>
    /// <param name="userLocalTime">The sender's local time, as determined by the local timestamp of the activity.</param>
    /// <returns>User's local date and time.</returns>
    private static DateTime GetFormattedDateInUserTimeZone(
        DateTime dateTime,
        DateTimeOffset? userLocalTime)
    {
        // Adaptive card on mobile has a bug where it does not support DATE and TIME, so for now we convert the date and time manually.
        return dateTime.Add(value: userLocalTime?.Offset ?? TimeSpan.FromMinutes(value: 0));
    }

    /// <summary>
    /// Create event details adaptive to be shown in compose box.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    /// <param name="localizer">The localizer for localizing content</param>
    /// <param name="applicationBasePath">Application base URL.</param>
    /// <returns>An adaptive card with event details.</returns>
    private static AdaptiveCard GetEventDetailsAdaptiveCard(
        EventEntity eventDetails,
        IStringLocalizer<Strings> localizer,
        string applicationBasePath)
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
                            Height = AdaptiveHeight.Auto,
                            Width = AdaptiveColumnWidth.Auto,
                            Items = !string.IsNullOrEmpty(value: eventDetails.Photo)
                                ? new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(uriString: eventDetails.Photo),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        PixelHeight = 50,
                                        PixelWidth = 50,
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
                                    Size = AdaptiveTextSize.Large,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    HorizontalAlignment = textAlignment,
                                },
                                new AdaptiveTextBlock
                                {
                                    Text = eventDetails.CategoryName,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Default,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Color = AdaptiveTextColor.Attention,
                                    Spacing = AdaptiveSpacing.Small,
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
                            Width = "100px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"**{localizer.GetString(name: "DateAndTimeLabel")}:** ",
                                    Wrap = true,
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
                                    Text = string.Format(provider: CultureInfo.CurrentCulture, format: "{0} {1}-{2}", arg0: "{{DATE(" + eventDetails.StartDate.Value.ToString(format: "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", provider: CultureInfo.InvariantCulture) + ", SHORT)}}", arg1: "{{TIME(" + eventDetails.StartTime.Value.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}", arg2: "{{TIME(" + eventDetails.EndTime.ToString(format: "yyyy-MM-dd'T'HH:mm:ss'Z'", provider: CultureInfo.InvariantCulture) + ")}}"),
                                    Wrap = true,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
                new AdaptiveColumnSet
                {
                    Spacing = AdaptiveSpacing.None,
                    IsVisible = eventDetails.Type == (int)EventType.InPerson,
                    Columns = new List<AdaptiveColumn>
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
                                    Text = eventDetails.Venue,
                                    Wrap = true,
                                    HorizontalAlignment = textAlignment,
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
                            Width = "100px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"**{localizer.GetString(name: "DescriptionLabelCard")}:** ",
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Bolder,
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
                                    Text = eventDetails.Description,
                                    Wrap = true,
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
                                new AdaptiveImage
                                {
                                    IsVisible = eventDetails.Audience == (int)EventAudience.Private,
                                    Url = new Uri(uriString: $"{applicationBasePath}/images/Private.png"),
                                    PixelWidth = 84,
                                    PixelHeight = 32,
                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                },
                            },
                        },
                    },
                },
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = localizer.GetString(name: "RegisterButton"),
                    Data = new AdaptiveSubmitActionData
                    {
                        MsTeams = new CardAction
                        {
                            Type = "task/fetch",
                            Text = localizer.GetString(name: "RegisterButton"),
                        },
                        Command = BotCommands.RegisterForEvent,
                        EventId = eventDetails.EventId,
                        TeamId = eventDetails.TeamId,
                    },
                },
            },
        };

        return card;
    }

    /// <summary>
    /// Create thumbnail card for messaging extension.
    /// </summary>
    /// <param name="eventDetails">Event details.</param>
    /// <param name="localDateTime">The sender's local time, as determined by the local timestamp of the activity.</param>
    /// <param name="localizer">Localization of strings</param>
    /// <returns>Thumbnail card.</returns>
    private static ThumbnailCard GetThumbnailCard(
        EventEntity eventDetails,
        DateTimeOffset? localDateTime,
        IStringLocalizer<Strings> localizer)
    {
        var titleString = eventDetails.Name.Length < TitleMaximumLength ? HttpUtility.HtmlEncode(s: eventDetails.Name) : $"{HttpUtility.HtmlEncode(s: eventDetails.Name.Substring(startIndex: 0, length: TitleMaximumLength))}...";
        var categoryString = !string.IsNullOrEmpty(value: eventDetails.CategoryName)
            ? eventDetails.CategoryName.Length < CategoryMaximumLength ? HttpUtility.HtmlEncode(s: eventDetails.CategoryName) :
            $"{HttpUtility.HtmlEncode(s: eventDetails.CategoryName.Substring(startIndex: 0, length: CategoryMaximumLength))}..."
            : string.Empty;
        var locationString = string.Empty;

        if (!string.IsNullOrEmpty(value: eventDetails.Venue))
        {
            locationString = eventDetails.Venue.Length < LocationMaximumLength ? HttpUtility.HtmlEncode(s: eventDetails.Venue) : $"{HttpUtility.HtmlEncode(s: eventDetails.Venue.Substring(startIndex: 0, length: LocationMaximumLength))}...";
        }
        else
        {
            switch ((EventType)eventDetails.Type)
            {
                case EventType.InPerson:
                    locationString = $"{localizer.GetString(name: "TrainingTypeInPerson")}";
                    break;
                case EventType.Teams:
                    locationString = $"{localizer.GetString(name: "TeamsMeetingText")}";
                    break;
                case EventType.LiveEvent:
                    locationString = $"{localizer.GetString(name: "TrainingTypeLiveEvent")}";
                    break;
            }
        }

        var startDateInUserLocalTime = GetFormattedDateInUserTimeZone(dateTime: eventDetails.StartDate.Value, userLocalTime: localDateTime);
        var startTimeInUserLocalTime = GetFormattedDateInUserTimeZone(dateTime: eventDetails.StartTime.Value, userLocalTime: localDateTime);
        var endTimeInUserLocalTime = GetFormattedDateInUserTimeZone(dateTime: eventDetails.EndTime, userLocalTime: localDateTime);

        var trainingStartDateString = startDateInUserLocalTime.ToString(format: "d", provider: CultureInfo.CurrentCulture);
        var trainingStartTimeString = startTimeInUserLocalTime.ToString(format: "t", provider: CultureInfo.CurrentCulture);
        var trainingEndTimeString = endTimeInUserLocalTime.ToString(format: "t", provider: CultureInfo.CurrentCulture);

        var text = (EventAudience)eventDetails.Audience == EventAudience.Private
            ? $"<span style='color: #A72037; font-weight: 600;'>{HttpUtility.HtmlEncode(s: categoryString)} &nbsp;|</span>" +
              $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(s: locationString)}&nbsp;|</span>" +
              $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(s: localizer.GetString(name: "AudiencePrivate"))}</span><br/>" +
              $"<span style='font-size: 11px; line-height: 22px;'>{HttpUtility.HtmlEncode(s: trainingStartDateString)}, " +
              $"{HttpUtility.HtmlEncode(s: trainingStartTimeString)}-{HttpUtility.HtmlEncode(s: trainingEndTimeString)}</span>"
            : $"<span style='color: #A72037; font-weight: 600;'>{HttpUtility.HtmlEncode(s: categoryString)} &nbsp;|</span>" +
              $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(s: locationString)}</span><br/>" +
              $"<span style='font-size: 11px; line-height: 22px;'>{HttpUtility.HtmlEncode(s: trainingStartDateString)}, " +
              $"{HttpUtility.HtmlEncode(s: trainingStartTimeString)}-{HttpUtility.HtmlEncode(s: trainingEndTimeString)}</span>";

        return new ThumbnailCard
        {
            Title = $"<span style='font-weight: 600;'>{HttpUtility.HtmlEncode(s: titleString)}</span>",
            Text = text,
        };
    }
}