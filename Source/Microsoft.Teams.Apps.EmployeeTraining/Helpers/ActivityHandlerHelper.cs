// <copyright file="ActivityHandlerHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Bot;
using Microsoft.Teams.Apps.EmployeeTraining.Cards;
using Microsoft.Teams.Apps.EmployeeTraining.Common;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using Microsoft.Teams.Apps.EmployeeTraining.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Helper for handling bot related activities.
/// </summary>
public class ActivityHandlerHelper : IActivityHandlerHelper
{
    /// <summary>
    /// A set of key/value application configuration properties for Activity settings.
    /// </summary>
    private readonly IOptions<BotSettings> botOptions;

    /// <summary>
    /// The current cultures' string localizer.
    /// </summary>
    private readonly IStringLocalizer<Strings> localizer;

    /// <summary>
    /// Instance to send logs to the Application Insights service.
    /// </summary>
    private readonly ILogger<EmployeeTrainingActivityHandler> logger;

    /// <summary>
    /// Provides insert and delete operations for team configuration entity.
    /// </summary>
    private readonly ILnDTeamConfigurationRepository teamConfigurationRepository;

    /// <summary>
    /// Provides insert and delete operations for user details entity.
    /// </summary>
    private readonly IUserConfigurationRepository userConfigurationRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityHandlerHelper" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="localizer">The current cultures' string localizer.</param>
    /// <param name="options">The options.</param>
    /// <param name="teamConfigurationRepository">Provides insert and delete operations for team configuration entity.</param>
    /// <param name="userConfigurationRepository">Provides insert and delete operations for user details entity.</param>
    public ActivityHandlerHelper(
        ILogger<EmployeeTrainingActivityHandler> logger,
        IStringLocalizer<Strings> localizer,
        IOptions<BotSettings> options,
        ILnDTeamConfigurationRepository teamConfigurationRepository,
        IUserConfigurationRepository userConfigurationRepository)
    {
        this.logger = logger;
        this.localizer = localizer;
        this.botOptions = options;
        this.teamConfigurationRepository = teamConfigurationRepository;
        this.userConfigurationRepository = userConfigurationRepository;
    }

    /// <summary>
    /// Sent welcome card to personal chat.
    /// </summary>
    /// <param name="turnContext">Provides context for a turn in a bot.</param>
    /// <returns>A task that represents a response.</returns>
    public async Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext)
    {
        turnContext = turnContext ?? throw new ArgumentNullException(paramName: nameof(turnContext), message: "Turncontext cannot be null");

        this.logger.LogInformation(message: $"Bot added in personal scope for user {turnContext.Activity.From.AadObjectId}");
        var userWelcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForPersonal(
            applicationBasePath: this.botOptions.Value.AppBaseUri,
            localizer: this.localizer,
            applicationManifestId: this.botOptions.Value.ManifestId);
        await turnContext.SendActivityAsync(activity: MessageFactory.Attachment(attachment: userWelcomeCardAttachment));

        var activity = turnContext.Activity;
        var userEntity = new User
        {
            AADObjectId = activity.From.AadObjectId,
            ConversationId = activity.Conversation.Id,
            BotInstalledOn = DateTime.UtcNow,
            ServiceUrl = turnContext.Activity.ServiceUrl,
        };

        var operationStatus = await this.userConfigurationRepository.UpsertUserConfigurationsAsync(userConfigurationDetails: userEntity);
        if (operationStatus)
        {
            this.logger.LogInformation(message: $"Successfully stored bot installation state for user {activity.From.AadObjectId} in storage.");
        }
        else
        {
            this.logger.LogInformation(message: $"Unable to store bot installation state for user {activity.From.AadObjectId} in storage.");
        }
    }

    /// <summary>
    /// Add user membership to storage if bot is installed in Team scope.
    /// </summary>
    /// <param name="turnContext">Provides context for a turn in a bot.</param>
    /// <returns>A task that represents a response.</returns>
    public async Task SendWelcomeCardInChannelAsync(ITurnContext<IConversationUpdateActivity> turnContext)
    {
        turnContext = turnContext ?? throw new ArgumentNullException(paramName: nameof(turnContext), message: "Turncontext cannot be null");

        var userWelcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForTeam(applicationBasePath: this.botOptions.Value.AppBaseUri, localizer: this.localizer);
        await turnContext.SendActivityAsync(activity: MessageFactory.Attachment(attachment: userWelcomeCardAttachment));
    }

    /// <summary>
    /// Send a welcome card if bot is installed in Team scope.
    /// </summary>
    /// <param name="turnContext">Provides context for a turn in a bot.</param>
    /// <returns>A task that represents a response.</returns>
    public async Task OnBotInstalledInTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext)
    {
        turnContext = turnContext ?? throw new ArgumentNullException(paramName: nameof(turnContext));

        // If bot added to team, add team tab configuration with service URL.
        await this.SendWelcomeCardInChannelAsync(turnContext: turnContext);

        var activity = turnContext.Activity;

        // Storing team information to storage.
        var teamsDetails = activity.TeamsGetTeamInfo();

        if (teamsDetails == null)
        {
            this.logger.LogInformation(message: $"Unable to store bot installation state for team {teamsDetails.Id} in storage. Team details is null.");
        }
        else
        {
            this.logger.LogInformation(message: $"Bot added in team {teamsDetails.Id}");
            var teamEntity = new LnDTeam
            {
                TeamId = teamsDetails.Id,
                BotInstalledOn = DateTime.UtcNow,
                ServiceUrl = activity.ServiceUrl,
            };

            var operationStatus = await this.teamConfigurationRepository.InsertLnDTeamConfigurationAsync(teamDetails: teamEntity);

            if (operationStatus)
            {
                this.logger.LogInformation(message: $"Successfully stored bot installation state for team {teamsDetails.Id} in storage.");
            }
            else
            {
                this.logger.LogInformation(message: $"Unable to store bot installation state for team {teamsDetails.Id} in storage.");
            }
        }
    }

    /// <summary>
    /// Remove user details from storage if bot is uninstalled from Team scope.
    /// </summary>
    /// <param name="turnContext">Provides context for a turn in a bot.</param>
    /// <returns>A task that represents a response.</returns>
    public async Task OnBotUninstalledFromTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext)
    {
        turnContext = turnContext ?? throw new ArgumentNullException(paramName: nameof(turnContext), message: "Turncontext cannot be null");

        var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
        var teamId = teamsChannelData.Team.Id;
        this.logger.LogInformation(message: $"Bot removed from team {teamId}");

        try
        {
            var teamEntity = await this.teamConfigurationRepository.GetTeamDetailsAsync(teamId: teamId);
            if (teamEntity == null)
            {
                this.logger.LogError(message: $"Could not find team with Id {teamId} for deletion.");
                return;
            }

            // Deleting team information from storage when bot is uninstalled from a team.
            var deletedTeamDetailsStatus = await this.teamConfigurationRepository.DeleteLnDTeamConfigurationsAsync(teamDetails: teamEntity);
            if (deletedTeamDetailsStatus)
            {
                this.logger.LogError(message: $"Deleted team details for team {teamId}");
            }
            else
            {
                this.logger.LogError(message: $"Unable to clear team details for team {teamId}");
            }
        }
#pragma warning disable CA1031 // Catching general exception to continue flow after logging it.
        catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception to continue flow after logging it.
        {
            this.logger.LogError(exception: ex, message: $"Failed to delete team details from storage for team {teamId} after bot is uninstalled");
        }
    }

    /// <summary>
    /// Process task module fetch request.
    /// </summary>
    /// <param name="turnContext">Provides context for a turn in a bot.</param>
    /// <returns>A task that represents a response.</returns>
    public async Task<TaskModuleResponse> OnTaskModuleFetchRequestAsync(ITurnContext<IInvokeActivity> turnContext)
    {
        turnContext = turnContext ?? throw new ArgumentNullException(paramName: nameof(turnContext), message: "Turn context cannot be null");
        var member = await TeamsInfo.GetMemberAsync(turnContext: turnContext, userId: turnContext.Activity.From.Id, cancellationToken: CancellationToken.None);

        if (member == null)
        {
            return this.GetTaskModuleResponse(url: new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/error"), title: this.localizer.GetString(name: "ErrorTitle"));
        }

        var activity = turnContext.Activity as Activity;

        var activityValue = JObject.Parse(json: activity.Value?.ToString())[propertyName: "data"].ToString();

        var adaptiveTaskModuleCardAction = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(value: activityValue);

        if (adaptiveTaskModuleCardAction == null)
        {
            this.logger.LogInformation(message: "Value obtained from task module fetch action is null");
        }

        var command = adaptiveTaskModuleCardAction.Command;
        Uri taskModuleRequestUrl;

        switch (command)
        {
            case BotCommands.EditEvent:
                taskModuleRequestUrl = new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/create-event?teamId={adaptiveTaskModuleCardAction.EventId}&eventId={adaptiveTaskModuleCardAction.EventId}");
                return this.GetTaskModuleResponse(url: taskModuleRequestUrl, title: this.localizer.GetString(name: "EditEventCardButton"));

            case BotCommands.CreateEvent:
                taskModuleRequestUrl = new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/create-event");
                return this.GetTaskModuleResponse(url: taskModuleRequestUrl, title: this.localizer.GetString(name: "CreateEventButtonWelcomeCard"));

            case BotCommands.CloseRegistration:
                taskModuleRequestUrl = new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/close-or-cancel-event?operationType={(int)EventOperationType.CloseRegistration}&eventId={adaptiveTaskModuleCardAction.EventId}&teamId={adaptiveTaskModuleCardAction.TeamId}");
                return this.GetTaskModuleResponse(url: taskModuleRequestUrl, title: this.localizer.GetString(name: "CloseRegistrationCardButton"));

            case BotCommands.RegisterForEvent:
                taskModuleRequestUrl = new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/register-remove?eventId={adaptiveTaskModuleCardAction.EventId}&teamId={adaptiveTaskModuleCardAction.TeamId}");
                return this.GetTaskModuleResponse(url: taskModuleRequestUrl, title: this.localizer.GetString(name: "RegisterButton"));

            default:
                return this.GetTaskModuleResponse(url: new Uri(uriString: $"{this.botOptions.Value.AppBaseUri}/error"), title: this.localizer.GetString(name: "ErrorTitle"));
        }
    }

    /// <summary>
    /// Gets a task module response
    /// </summary>
    /// <param name="url">The task module request URL</param>
    /// <param name="title">The title of the task module</param>
    /// <returns>Task module response object</returns>
    public TaskModuleResponse GetTaskModuleResponse(
        Uri url,
        string title)
    {
        return new TaskModuleResponse
        {
            Task = new TaskModuleContinueResponse
            {
                Value = new TaskModuleTaskInfo
                {
                    Url = url?.ToString(),
                    Height = 746,
                    Width = 600,
                    Title = title,
                },
            },
        };
    }
}