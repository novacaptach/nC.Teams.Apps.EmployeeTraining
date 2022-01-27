// <copyright file="CategoryHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService;
using Microsoft.Teams.Apps.EmployeeTraining.Test.Providers;
using Microsoft.Teams.Apps.EmployeeTraining.Test.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class CategoryHelperTest
{
    private CategoryHelper categoryHelper;
    private Mock<ICategoryRepository> categoryStorageProvider;
    private CategoryStorageProviderFake categoryStorageProviderFake;
    private Mock<ITeamEventSearchService> teamEventSearchService;

    [TestInitialize]
    public void CategoryHelperTestSetup()
    {
        this.categoryStorageProvider = new Mock<ICategoryRepository>();
        this.teamEventSearchService = new Mock<ITeamEventSearchService>();
        this.categoryStorageProviderFake = new CategoryStorageProviderFake();

        this.categoryHelper = new CategoryHelper(teamEventSearchService: this.teamEventSearchService.Object, categoryRepository: this.categoryStorageProvider.Object);
    }

    [TestMethod]
    public async Task ValidateCategoryInUse_NotInUse()
    {
        const string categoryToTest = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba";
        var searchParamsDto = new SearchParametersDto();

        this.teamEventSearchService
            .Setup(t => t.GetEventsAsync(searchParamsDto))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.eventEntities.Where(e => e.CategoryId == categoryToTest)));

        await this.categoryHelper.CheckIfCategoryIsInUseAsync(categories: EventWorkflowHelperData.categoryList);

        Assert.AreEqual(expected: false, actual: EventWorkflowHelperData.categoryList.Where(e => e.CategoryId == categoryToTest).FirstOrDefault().IsInUse);
    }

    [TestMethod]
    public async Task DeleteCategoriesAsync()
    {
        const string categoryToTest = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba,ad4b2b43-1cb5-408d-ab8a-17e28edac2ba";
        var searchParamsDto = new SearchParametersDto();

        this.teamEventSearchService
            .Setup(t => t.GetEventsAsync(searchParamsDto))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.eventEntities.Where(e => e.CategoryId == categoryToTest)));

        this.categoryStorageProvider
            .Setup(t => t.GetCategoriesByIdsAsync(It.IsAny<string[]>()))
            .Returns(value: Task.FromResult(result: EventWorkflowHelperData.categoryList as IEnumerable<Category>));
        this.categoryStorageProvider
            .Setup(t => t.DeleteCategoriesInBatchAsync(It.IsAny<IEnumerable<Category>>()))
            .Returns(value: Task.FromResult(result: true));

        var Result = await this.categoryHelper.DeleteCategoriesAsync(categoryIds: categoryToTest);

        Assert.AreEqual(expected: true, actual: Result);
    }

    [TestMethod]
    public async Task BindCategoryDetailsAsync()
    {
        var events = EventWorkflowHelperData.eventEntities;
        var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();

        this.categoryStorageProvider
            .Setup(x => x.GetCategoriesByIdsAsync(eventCategoryIds))
            .Returns(value: this.categoryStorageProviderFake.GetCategoriesByIdsAsync(categoryIds: eventCategoryIds));

        await this.categoryHelper.BindCategoryNameAsync(events: events);

        Assert.AreEqual(expected: true, actual: events[index: 0].CategoryName != "");
    }
}