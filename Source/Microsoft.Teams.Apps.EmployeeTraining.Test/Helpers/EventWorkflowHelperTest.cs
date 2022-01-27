// <copyright file="EventWorkFlowHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService;
using Microsoft.Teams.Apps.EmployeeTraining.Test.Providers;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using User = Microsoft.Graph.User;

[TestClass]
public class EventWorkflowHelperTest
{
    private Mock<ICategoryHelper> categoryHelper;
    private Mock<IEventGraphHelper> eventGraphHelper;
    private Mock<IEventSearchService> eventSearchServiceProvider;
    private Mock<IEventRepository> eventStorageProvider;
    private EventStorageProviderFake eventStorageProviderFake;
    private EventWorkflowHelper eventWorkflowHelper;
    private Mock<IGroupGraphHelper> groupGraphHelper;
    private Mock<ILnDTeamConfigurationRepository> lnDTeamConfigurationStorageProvider;
    private Mock<IOptions<BotSettings>> mockBotSettings;
    private Mock<INotificationHelper> notificationHelper;
    private UserConfigurationStorageProviderFake userConfigurationStorageProviderFake;
    private Mock<IUserGraphHelper> userGraphHelper;
    private Mock<IUserConfigurationRepository> userStorageConfigurationProvider;

    [TestInitialize]
    public void EventWorkflowHelperTestSetup()
    {
        var localizer = new Mock<IStringLocalizer<Strings>>().Object;
        this.eventGraphHelper = new Mock<IEventGraphHelper>();
        this.userGraphHelper = new Mock<IUserGraphHelper>();
        this.groupGraphHelper = new Mock<IGroupGraphHelper>();
        this.notificationHelper = new Mock<INotificationHelper>();
        this.eventStorageProvider = new Mock<IEventRepository>();
        this.eventSearchServiceProvider = new Mock<IEventSearchService>();
        this.eventStorageProviderFake = new EventStorageProviderFake();
        this.userStorageConfigurationProvider = new Mock<IUserConfigurationRepository>();
        this.userConfigurationStorageProviderFake = new UserConfigurationStorageProviderFake();
        this.lnDTeamConfigurationStorageProvider = new Mock<ILnDTeamConfigurationRepository>();
        this.categoryHelper = new Mock<ICategoryHelper>();
        this.mockBotSettings = new Mock<IOptions<BotSettings>>();

        this.eventWorkflowHelper = new EventWorkflowHelper(eventRepository: this.eventStorageProvider.Object, eventSearchService: this.eventSearchServiceProvider.Object, eventGraphHelper: this.eventGraphHelper.Object, groupGraphHelper: this.groupGraphHelper.Object, userConfigurationRepository: this.userStorageConfigurationProvider.Object, teamConfigurationRepository: this.lnDTeamConfigurationStorageProvider.Object, categoryHelper: this.categoryHelper.Object,
            localizer: localizer, userGraphHelper: this.userGraphHelper.Object, notificationHelper: this.notificationHelper.Object,
            botOptions: EventWorkflowHelperData.botOptions);
    }

    [TestMethod]
    public async Task UpdateDraftEventAsync()
    {
        var eventToUpdate = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(eventToUpdate.EventId, eventToUpdate.TeamId))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToUpdate.EventId, teamId: eventToUpdate.TeamId));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.UpdateDraftEventAsync(eventEntity: EventWorkflowHelperData.eventEntity);
        Assert.AreEqual(expected: result, actual: true);
    }

    [TestMethod]
    public async Task DeleteDraftEventAsync()
    {
        var eventToDelete = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(eventToDelete.EventId, eventToDelete.TeamId))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToDelete.EventId, teamId: eventToDelete.TeamId));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.DeleteDraftEventAsync(teamId: eventToDelete.TeamId, eventId: eventToDelete.EventId);
        Assert.AreEqual(expected: result, actual: true);
    }

    [TestMethod]
    public async Task UpdateEventAsync()
    {
        var eventToUpdate = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: "activeEventId", teamId: eventToUpdate.TeamId));

        this.eventGraphHelper
            .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.teamEvent));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.UpdateEventAsync(eventEntity: EventWorkflowHelperData.eventEntity);
        Assert.AreEqual(expected: result, actual: true);
    }

    [TestMethod]
    public async Task CloseEventRegistrations()
    {
        var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToCloseRegistration.EventId, teamId: eventToCloseRegistration.TeamId));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.CloseEventRegistrationsAsync(teamId: eventToCloseRegistration.TeamId, eventId: eventToCloseRegistration.EventId, userAadId: "8781d219-3920-4b4a-b280-48a17d2f23a6");
        Assert.AreEqual(expected: result, actual: false);
    }

    [TestMethod]
    public async Task CloseEventRegistrationsFail()
    {
        var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: "activeEventId", teamId: eventToCloseRegistration.TeamId));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.CloseEventRegistrationsAsync(teamId: eventToCloseRegistration.TeamId, eventId: eventToCloseRegistration.EventId, userAadId: "8781d219-3920-4b4a-b280-48a17d2f23a6");
        Assert.AreEqual(expected: result, actual: true);
    }

    [TestMethod]
    public async Task UpdateEventStatus()
    {
        var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToCloseRegistration.EventId, teamId: eventToCloseRegistration.TeamId));

        this.eventGraphHelper
            .Setup(x => x.CancelEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: Task.FromResult(result: true));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.UpdateEventStatusAsync(teamId: eventToCloseRegistration.TeamId, eventId: eventToCloseRegistration.EventId, eventStatus: (EventStatus)2, userAadId: "8781d219-3920-4b4a-b280-48a17d2f23a6");
        Assert.AreEqual(expected: result, actual: true);
    }

    [TestMethod]
    public async Task UpdateEventStatusFail()
    {
        var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: "completedEventId", teamId: eventToCloseRegistration.TeamId));

        this.eventGraphHelper
            .Setup(x => x.CancelEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: Task.FromResult(result: true));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        this.eventSearchServiceProvider
            .Setup(x => x.RunIndexerOnDemandAsync())
            .Returns(value: Task.FromResult(result: true));

        var result = await this.eventWorkflowHelper.UpdateEventStatusAsync(teamId: eventToCloseRegistration.TeamId, eventId: eventToCloseRegistration.EventId, eventStatus: (EventStatus)2, userAadId: "8781d219-3920-4b4a-b280-48a17d2f23a6");
        Assert.AreEqual(expected: result, actual: false);
    }

    [TestMethod]
    public async Task ExportEventDetailsToCSV()
    {
        var eventToExport = EventWorkflowHelperData.eventEntity;

        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: eventToExport.EventId, teamId: eventToExport.TeamId));

        this.userGraphHelper
            .Setup(x => x.GetUsersAsync(It.IsAny<List<string>>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.graphUsers as IEnumerable<User>));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        var result = await this.eventWorkflowHelper.ExportEventDetailsToCSVAsync(teamId: eventToExport.TeamId, eventId: eventToExport.EventId);
        Assert.AreEqual(expected: result.Length > 0, actual: true);
    }

    [TestMethod]
    public async Task CreateDraftEventAsync()
    {
        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        var Result = await this.eventWorkflowHelper.CreateDraftEventAsync(eventEntity: EventWorkflowHelperData.eventEntity);

        Assert.AreEqual(expected: Result, actual: true);
    }

    [TestMethod]
    public async Task CreateNewEventAsync()
    {
        this.eventStorageProvider
            .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(value: this.eventStorageProviderFake.GetEventDetailsAsync(eventId: It.IsAny<string>(), teamId: It.IsAny<string>()));

        this.eventGraphHelper
            .Setup(x => x.CreateEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.teamEvent));

        this.eventStorageProvider
            .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
            .Returns(value: this.eventStorageProviderFake.UpsertEventAsync(eventDetails: EventWorkflowHelperData.eventEntity));

        var events = EventWorkflowHelperData.eventEntities;
        var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();

        this.lnDTeamConfigurationStorageProvider
            .Setup(x => x.GetTeamDetailsAsync(It.IsAny<string>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.lndTeam));

        this.mockBotSettings
            .Setup(x => x.Value)
            .Returns(value: EventWorkflowHelperData.botOptions.Value);

        var result = await this.eventWorkflowHelper.CreateNewEventAsync(eventEntity: EventWorkflowHelperData.eventEntity, createdByName: "");

        Assert.AreEqual(expected: result, actual: true);
    }
}