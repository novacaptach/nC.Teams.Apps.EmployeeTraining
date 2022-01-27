// <copyright file="EventCreationCardTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Cards;

using AdaptiveCards;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Cards;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class EventCreationCardTest
{
    private Mock<IStringLocalizer<Strings>> localizer;

    [TestInitialize]
    public void EventCreationCardTestSetup()
    {
        this.localizer = new Mock<IStringLocalizer<Strings>>();
    }

    [TestMethod]
    public void GetEventCreationCardForTeam()
    {
        var Results = EventDetailsCard.GetEventCreationCardForTeam(applicationBasePath: "https://www.example.com", localizer: this.localizer.Object, eventEntity: EventWorkflowHelperData.validEventEntity, createdByName: "random");

        Assert.AreEqual(expected: Results.ContentType, actual: AdaptiveCard.ContentType);
    }
}