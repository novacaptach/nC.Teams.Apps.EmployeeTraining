// <copyright file="ReminderCardTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Cards;

using System.Collections.Generic;
using AdaptiveCards;
using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.EmployeeTraining.Cards;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ReminderCardTest
{
    private Mock<IStringLocalizer<Strings>> localizer;

    [TestInitialize]
    public void ReminderCardTestSetup()
    {
        this.localizer = new Mock<IStringLocalizer<Strings>>();
    }

    [TestMethod]
    public void GetCard()
    {
        var Results = ReminderCard.GetCard(events: new List<EventEntity> { EventWorkflowHelperData.validEventEntity }, localizer: this.localizer.Object, applicationManifestId: "random");

        Assert.AreEqual(expected: Results.ContentType, actual: AdaptiveCard.ContentType);
    }
}