// <copyright file="EventCancellationCardTest.cs" company="Microsoft">
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
public class EventCancellationCardTest
{
    private Mock<IStringLocalizer<Strings>> localizer;

    [TestInitialize]
    public void EventCancellationCardTestSetup()
    {
        this.localizer = new Mock<IStringLocalizer<Strings>>();
    }

    [TestMethod]
    public void GetCard()
    {
        var Results = EventCancellationCard.GetCancellationCard(localizer: this.localizer.Object, eventEntity: EventWorkflowHelperData.validEventEntity, applicationManifestId: "random");

        Assert.AreEqual(expected: Results.ContentType, actual: AdaptiveCard.ContentType);
    }
}