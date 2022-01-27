// <copyright file="AutoRegisteredCardTest.cs" company="Microsoft">
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
public class AutoRegisteredCardTest
{
    private Mock<IStringLocalizer<Strings>> localizer;

    [TestInitialize]
    public void AutoRegisteredCardTestSetup()
    {
        this.localizer = new Mock<IStringLocalizer<Strings>>();
    }

    [TestMethod]
    public void GetAutoRegisteredCard()
    {
        var Results = AutoRegisteredCard.GetAutoRegisteredCard(applicationBasePath: "https://www.random.com", localizer: this.localizer.Object, eventEntity: EventWorkflowHelperData.validEventEntity, applicationManifestId: "random");

        Assert.AreEqual(expected: Results.ContentType, actual: AdaptiveCard.ContentType);
    }
}