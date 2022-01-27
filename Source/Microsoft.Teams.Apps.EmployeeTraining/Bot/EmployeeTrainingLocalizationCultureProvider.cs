// <copyright file="EmployeeTrainingLocalizationCultureProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Bot;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The BotLocalizationCultureProvider is responsible for implementing the <see cref="IRequestCultureProvider" /> for Bot
/// Activities
/// received from BotFramework.
/// </summary>
internal sealed class EmployeeTrainingLocalizationCultureProvider : IRequestCultureProvider
{
    private const string UserAgentHeaderKey = "User-Agent";
    private const string UserAgentMSBotHeaderValue = "Microsoft-BotFramework";
    private const string AcceptLanguageHeaderKey = "Accept-Language";
    private const int StreamReaderBufferSize = 1024;

    /// <summary>
    /// Get the culture of the current request.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    /// <returns>A Task resolving to the culture info if found, null otherwise.</returns>
#pragma warning disable UseAsyncSuffix // Interface method doesn't have Async suffix.
    public async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
#pragma warning restore UseAsyncSuffix
    {
        if (httpContext?.Request?.Body?.CanRead != true)
        {
            return null;
        }

        var isBotFrameworkUserAgent =
            httpContext.Request.Headers[key: UserAgentHeaderKey]
                .Any(userAgent => userAgent.Contains(value: UserAgentMSBotHeaderValue, comparisonType: StringComparison.OrdinalIgnoreCase));

        if (!isBotFrameworkUserAgent)
        {
            var locale = httpContext.Request.Headers[key: AcceptLanguageHeaderKey].FirstOrDefault();
            locale = locale?.Split(separator: ",")?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(value: locale))
            {
                return null;
            }

            return new ProviderCultureResult(culture: locale);
        }

        try
        {
            // Wrap the request stream so that we can rewind it back to the start for regular request processing.
            httpContext.Request.EnableBuffering();

            // Read the request body, parse out the activity object, and set the parsed culture information.
            using (var streamReader = new StreamReader(stream: httpContext.Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: StreamReaderBufferSize, leaveOpen: true))
            {
                using (var jsonReader = new JsonTextReader(reader: streamReader))
                {
                    var obj = await JObject.LoadAsync(reader: jsonReader);
                    var activity = obj.ToObject<Activity>();

                    var result = new ProviderCultureResult(culture: activity.Locale);
                    return result;
                }
            }
        }
#pragma warning disable CA1031 // part of the middleware pipeline, better to use default locale then fail the request.
        catch (Exception)
#pragma warning restore CA1031
        {
            return null;
        }
        finally
        {
            httpContext.Request.Body.Seek(offset: 0, origin: SeekOrigin.Begin);
        }
    }
}