// <copyright file="UserEventsTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Helpers;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService;
using Microsoft.Teams.Apps.EmployeeTraining.Test.Providers;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class UserEventsHelperTest
{
    private Mock<IOptions<BotSettings>> botOptions;
    private Mock<ICategoryHelper> categoryHelper;
    private Mock<IEventGraphHelper> eventGraphHelper;
    private Mock<IEventSearchService> eventSearchServiceProvider;
    private Mock<IEventRepository> eventStorageProvider;
    private EventStorageProviderFake eventStorageProviderFake;
    private LnDTeamConfigurationStorageProviderFake lnDTeamConfigurationStorageProviderFake;
    private Mock<INotificationHelper> notificationHelper;
    private Mock<IUserEventSearchService> userEventSearchServiceHelper;
    private UserEventsHelper userEventsHelper;
    private Mock<IUserGraphHelper> userGraphHelper;

    [TestInitialize]
    public void UserEventsHelperTestSetup()
    {
        var mock = new Mock<IStringLocalizer<Strings>>();
        var key = "Hello my dear friend!";
        var localizedString = new LocalizedString(name: key, value: key);
        mock.Setup(_ => _[key]).Returns(value: localizedString);
        var localizer = mock.Object;

        this.eventGraphHelper = new Mock<IEventGraphHelper>();
        this.userGraphHelper = new Mock<IUserGraphHelper>();
        this.eventStorageProvider = new Mock<IEventRepository>();
        this.eventSearchServiceProvider = new Mock<IEventSearchService>();
        this.categoryHelper = new Mock<ICategoryHelper>();
        this.notificationHelper = new Mock<INotificationHelper>();
        this.eventStorageProviderFake = new EventStorageProviderFake();
        this.lnDTeamConfigurationStorageProviderFake = new LnDTeamConfigurationStorageProviderFake();
        this.userEventSearchServiceHelper = new Mock<IUserEventSearchService>();
        this.botOptions = new Mock<IOptions<BotSettings>>();

        this.userEventsHelper = new UserEventsHelper(eventRepository: this.eventStorageProvider.Object, eventSearchService: this.eventSearchServiceProvider.Object, userEventSearchService: this.userEventSearchServiceHelper.Object, userGraphHelper: this.userGraphHelper.Object, eventGraphHelper: this.eventGraphHelper.Object, notificationHelper: this.notificationHelper.Object, categoryHelper: this.categoryHelper.Object, lnDTeamConfigurationRepository: this.lnDTeamConfigurationStorageProviderFake, botOptions: this.botOptions.Object,
            localizer: localizer);
    }

    [TestMethod]
    public async Task GetEventAsync()
    {
        var eventToFetch = EventWorkflowHelperData.eventEntity;
        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(eventToFetch.EventId, eventToFetch.TeamId))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToFetch.EventId, teamId: eventToFetch.TeamId));

        // Getting event details for AutoRegistered user.
        var Result = await this.userEventsHelper.GetEventAsync(eventId: eventToFetch.EventId, teamId: eventToFetch.TeamId, userObjectId: "a85c1ff9-7381-4721-bb7b-c8d9203d202c");

        Assert.AreEqual(expected: Result.IsMandatoryForLoggedInUser && Result.IsLoggedInUserRegistered, actual: true);
    }

    [TestMethod]
    public async Task RemoveEventFailAsync()
    {
        var eventToFetch = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: "activeEventId", teamId: eventToFetch.TeamId));

        this.eventGraphHelper
            .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.teamEvent));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var userAADObjectIdToRegeister = new Guid().ToString();

        // Removing unregistered user.
        var Result = await this.userEventsHelper.UnregisterFromEventAsync(teamId: eventToFetch.EventId, eventId: eventToFetch.TeamId, userAADObjectId: userAADObjectIdToRegeister);

        Assert.AreEqual(expected: Result, actual: false);
    }
}