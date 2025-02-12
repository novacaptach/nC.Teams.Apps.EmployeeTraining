﻿// <copyright file="BlobRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories.Implementation;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

/// <summary>
/// Interface for handling Azure Blob Storage operations like uploading/downloading/deleting files from blob.
/// </summary>
public class BlobRepository : IBlobRepository
{
    /// <summary>
    /// Container to hold events photos.
    /// </summary>
    private readonly string eventsPhotosContainerName = "events-photos";

    /// <summary>
    /// Instance to send logs to the Application Insights service.
    /// </summary>
    private readonly ILogger<BlobRepository> logger;

    /// <summary>
    /// Instance to hold Microsoft Azure Storage data.
    /// </summary>
    private readonly IOptionsMonitor<StorageSetting> storageOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobRepository" /> class.
    /// </summary>
    /// <param name="storageOptions">A set of key/value application storage configuration properties.</param>
    /// <param name="logger">Instance to send logs to the Application Insights service.</param>
    public BlobRepository(
        IOptionsMonitor<StorageSetting> storageOptions,
        ILogger<BlobRepository> logger)
    {
        this.logger = logger;
        this.storageOptions = storageOptions ?? throw new ArgumentNullException(paramName: nameof(storageOptions));
    }

    /// <summary>
    /// Initialize a blob client for interacting with the blob service.
    /// </summary>
    /// <returns>Returns blob client for blob operations.</returns>
    public CloudBlobClient InitializeBlobClient()
    {
        try
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString: this.storageOptions.CurrentValue.ConnectionString);

            // Create a blob client for interacting with the blob service.
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }
        catch (FormatException ex)
        {
            this.logger.LogError(exception: ex, message: "Blob client is not created. Please confirm the AccountName and AccountKey are valid.");
            throw;
        }
        catch (ArgumentException ex)
        {
            this.logger.LogError(exception: ex, message: "Invalid argument. Blob client is not created.");
            throw;
        }
    }

    /// <summary>
    /// Upload event image to blob container.
    /// </summary>
    /// <param name="fileStream">File stream of file to be uploaded on blob storage.</param>
    /// <param name="contentType">Content type to be set on blob.</param>
    /// <returns>Returns uploaded file blob URL.</returns>
    public async Task<string> UploadAsync(
        Stream fileStream,
        string contentType)
    {
        try
        {
            var fileName = Guid.NewGuid().ToString();
            var blockBlob = await this.GetBlockBlobAsync(containerName: fileName);

            // Set the blob's content type so that the browser knows how to treat file.
            blockBlob.Properties.ContentType = contentType;
            await blockBlob.UploadFromStreamAsync(source: fileStream);
            return blockBlob.Uri.ToString();
        }
        catch (StorageException ex)
        {
            this.logger.LogError(exception: ex, message: "Error while uploading file to Azure Blob Storage.");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(exception: ex, message: "Error while uploading file to Azure Blob Storage.");
            throw;
        }
    }

    /// <summary>
    /// Delete file from Azure Storage Blob container.
    /// </summary>
    /// <param name="blobFilePath">Blob URL file path on which file is uploaded.</param>
    /// <returns>Returns success if file deletion from blob is successful.</returns>
    public async Task<bool> DeleteAsync(string blobFilePath)
    {
        try
        {
            // Create a blob client for interacting with the blob service.
            var blobClient = this.InitializeBlobClient();
            var blob = await blobClient.GetBlobReferenceFromServerAsync(blobUri: new Uri(uriString: blobFilePath));
            await blob.DeleteIfExistsAsync(deleteSnapshotsOption: DeleteSnapshotsOption.IncludeSnapshots, accessCondition: null, options: null, operationContext: null);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(exception: ex, message: $"Error while deleting file from a blob url {blobFilePath} blob.");
            throw;
        }
    }

    /// <summary>
    /// Get block blob instance for blob service operations.
    /// </summary>
    /// <param name="containerName">Name of the container on Azure Table Storage.</param>
    /// <returns>Returns block blob instance for blob service operations</returns>
    private async Task<CloudBlockBlob> GetBlockBlobAsync(string containerName)
    {
        // Set the permissions so the blobs are public.
        var permissions = new BlobContainerPermissions
        {
            PublicAccess = BlobContainerPublicAccessType.Blob,
        };

        // Create a blob client for interacting with the blob service.
        var blobClient = this.InitializeBlobClient();

        // Create a container for organizing blobs within the storage account.
        var container = blobClient.GetContainerReference(containerName: this.eventsPhotosContainerName);

        var requestOptions = new BlobRequestOptions();
        await container.CreateIfNotExistsAsync(options: requestOptions, operationContext: null);
        await container.SetPermissionsAsync(permissions: permissions);

        var blockBlob = container.GetBlockBlobReference(blobName: containerName);
        return blockBlob;
    }
}