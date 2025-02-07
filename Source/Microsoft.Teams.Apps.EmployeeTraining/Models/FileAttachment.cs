﻿// <copyright file="FileAttachment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models;

using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Class contains Azure Blob Storage file stream details.
/// </summary>
public class FileAttachment
{
    /// <summary>
    /// Gets or sets content type of blob content.
    /// </summary>
    [JsonProperty(propertyName: "contentType")]
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets name of file on blob.
    /// </summary>
    [JsonProperty(propertyName: "fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets URL of file on blob.
    /// </summary>
    [JsonProperty(propertyName: "blobUrl")]
    public string BlobUrl { get; set; }

    /// <summary>
    /// Gets or sets file content length.
    /// </summary>
    [JsonProperty(propertyName: "contentLength")]
    public long ContentLength { get; set; }

    /// <summary>
    /// Gets or sets contents of file in memory stream.
    /// </summary>
    [JsonProperty(propertyName: "memoryStream")]
    public MemoryStream MemoryStream { get; set; }
}