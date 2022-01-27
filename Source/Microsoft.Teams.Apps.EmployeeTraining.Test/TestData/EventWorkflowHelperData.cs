namespace Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;

extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using EventType = Microsoft.Teams.Apps.EmployeeTraining.Models.Enums.EventType;
using User = Microsoft.Graph.User;

public static class EventWorkflowHelperData
{
    public static readonly IOptions<BotSettings> botOptions = Options.Create(options: new BotSettings
    {
        MicrosoftAppId = "{Application id}",
        MicrosoftAppPassword = "{Application password or secret}",
        AppBaseUri = "https://2db43ef5248b.ngrok.io/",
        EventsPageSize = 50,
    });

    public static readonly IOptions<AzureSettings> azureSettings = Options.Create(options: new AzureSettings
    {
        ClientId = "{Application id}",
    });

    public static EventEntity eventEntity;
    public static EventEntity validEventEntity;
    public static List<EventEntity> eventEntities;
    public static Category category;
    public static List<Category> categoryList;
    public static List<User> graphUsers;
    public static User graphUser;
    public static List<DirectoryObject> graphGroupDirectoryObject;
    public static List<Group> graphGroups;
    public static FormFile fileInfo;
    public static List<TeamsChannelAccount> teamsChannelAccount;
    public static Event teamEvent;
    public static LnDTeam lndTeam;

    static EventWorkflowHelperData()
    {
        eventEntity = new EventEntity
        {
            EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234-2345",
            TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234",
            Audience = 3,
            CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
            AutoRegisteredAttendees = "",
            CategoryName = "Test_Category",
            CreatedBy = "Jack",
            CreatedOn = new DateTime(year: 2020, month: 09, day: 24),
            Description = "Teams Event",
            EndDate = new DateTime(year: 2020, month: 09, day: 25),
            EndTime = new DateTime(year: 2020, month: 09, day: 25),
            ETag = "",
            GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
            IsAutoRegister = false,
            IsRegistrationClosed = false,
            IsRemoved = false,
            MandatoryAttendees = "",
            MaximumNumberOfParticipants = 10,
            MeetingLink = "",
            Name = "Mandaotory Training Event",
            NumberOfOccurrences = 1,
            OptionalAttendees = "",
            Photo = "https://testurl/img.png",
            StartDate = new DateTime(year: 2020, month: 09, day: 25),
            StartTime = new DateTime(year: 2020, month: 09, day: 25),
            UpdatedBy = "Jack",
            Venue = "",
            SelectedUserOrGroupListJSON = "",
            RegisteredAttendeesCount = 0,
            Type = 0,
            RegisteredAttendees = "",
        };

        validEventEntity = new EventEntity
        {
            EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234-2345",
            TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234",
            Audience = 1,
            CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
            AutoRegisteredAttendees = "",
            CategoryName = "Test_Category",
            CreatedBy = "Jack",
            CreatedOn = DateTime.UtcNow,
            Description = "Teams Event",
            EndDate = DateTime.UtcNow.AddDays(value: 4).Date,
            EndTime = DateTime.UtcNow.AddDays(value: 4).Date,
            ETag = "",
            GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
            IsAutoRegister = false,
            IsRegistrationClosed = false,
            IsRemoved = false,
            MandatoryAttendees = "",
            MaximumNumberOfParticipants = 10,
            MeetingLink = "",
            Name = "Mandaotory Training Event",
            NumberOfOccurrences = 1,
            OptionalAttendees = "",
            Photo = "https://www.testurl.com/img.png",
            StartDate = DateTime.UtcNow.AddDays(value: 2).Date,
            StartTime = DateTime.UtcNow.AddDays(value: 2).Date,
            UpdatedBy = "Jack",
            Venue = "",
            SelectedUserOrGroupListJSON = "",
            RegisteredAttendeesCount = 0,
            Type = 2,
            RegisteredAttendees = "",
        };

        eventEntities = new List<EventEntity>
        {
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-888",
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                CategoryName = "",
            },
            new ()
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac3ba",
                CategoryName = "",
            },
        };

        category = new Category
        {
            CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
            Name = "Test_Category_1",
            Description = "Description",
            CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        categoryList = new List<Category>
        {
            new ()
            {
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba",
                Name = "Test_Category_1",
                Description = "Description",
                CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsInUse = false,
            },
            new ()
            {
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                Name = "Test_Category_1",
                Description = "Description",
                CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsInUse = false,
            },
            new ()
            {
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac3ba",
                Name = "Test_Category_1",
                Description = "Description",
                CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsInUse = false,
            },
        };

        teamEvent = new Event
        {
            Subject = "Teams Event",
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = eventEntity.Type == (int)EventType.LiveEvent ? $"{eventEntity.Description}<br/><br/><a href='{eventEntity.MeetingLink}'>{eventEntity.MeetingLink}</a>" : eventEntity.Description,
            },
            Attendees = new List<Attendee>(),
            OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
            IsReminderOn = true,
            Location = eventEntity.Type == (int)EventType.InPerson
                ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                : null,
            AllowNewTimeProposals = false,
            IsOnlineMeeting = true,
            OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
            Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-rrtyy",
        };

        lndTeam = new LnDTeam
        {
            ETag = "",
            PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
            TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
            RowKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
        };

        graphUser = new User
        {
            DisplayName = "Jack",
            UserPrincipalName = "Jack",
            Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
            Mail = "a@user.com",
        };

        graphUsers = new List<User>
        {
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@user.com",
            },
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                Mail = "b@user.com",
            },
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                Mail = "c@user.com",
            },
        };

        graphGroups = new List<Group>
        {
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@group.com",
            },
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                Mail = "b@group.com",
            },
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                Mail = "c@group.com",
            },
        };

        graphGroupDirectoryObject = new List<DirectoryObject>
        {
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
            },
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
            },
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
            },
        };

        fileInfo = new FormFile(baseStream: new MemoryStream(), baseStreamOffset: 1, length: 1, name: "sample.jpeg", fileName: "sample.jpeg");

        teamsChannelAccount = new List<TeamsChannelAccount>
        {
            new ()
            {
                GivenName = "sam",
                UserPrincipalName = "s",
            },
            new ()
            {
                GivenName = "jack",
                UserPrincipalName = "j",
            },
        };

        teamEvent = new Event
        {
            Subject = "Teams Event",
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = eventEntity.Type == (int)EventType.LiveEvent ? $"{eventEntity.Description}<br/><br/><a href='{eventEntity.MeetingLink}'>{eventEntity.MeetingLink}</a>" : eventEntity.Description,
            },
            Attendees = new List<Attendee>(),
            OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
            IsReminderOn = true,
            Location = eventEntity.Type == (int)EventType.InPerson
                ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                : null,
            AllowNewTimeProposals = false,
            IsOnlineMeeting = true,
            OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
            Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-rrtyy",
        };

        lndTeam = new LnDTeam
        {
            ETag = "",
            PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
            TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
            RowKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
        };

        graphUser = new User
        {
            DisplayName = "Jack",
            UserPrincipalName = "Jack",
            Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
            Mail = "a@user.com",
        };

        graphUsers = new List<User>
        {
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@user.com",
            },
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                Mail = "b@user.com",
            },
            new ()
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                Mail = "c@user.com",
            },
        };

        graphGroups = new List<Group>
        {
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@group.com",
            },
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                Mail = "b@group.com",
            },
            new ()
            {
                DisplayName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                Mail = "c@group.com",
            },
        };

        graphGroupDirectoryObject = new List<DirectoryObject>
        {
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
            },
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
            },
            new ()
            {
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
            },
        };

        fileInfo = new FormFile(baseStream: new MemoryStream(), baseStreamOffset: 1, length: 1, name: "sample.jpeg", fileName: "sample.jpeg");

        teamsChannelAccount = new List<TeamsChannelAccount>
        {
            new ()
            {
                GivenName = "sam",
                UserPrincipalName = "s",
            },
            new ()
            {
                GivenName = "jack",
                UserPrincipalName = "j",
            },
        };
    }
}