using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Serilog;

namespace HakeHR.Application.Infrastructure
{
    /// <summary>
    /// Blob Storage Handler is used as connector with blob storage for main actions 
    /// </summary>
    internal class BlobStorageHandler
    {
        private readonly BlobContainerClient blobContainer;

        /// <summary>
        /// Constructor creates client with blob storage container
        /// </summary>
        /// <param name="storageAccountConn">Connection string to storage account</param>
        /// <param name="containerName">Storage account container name</param>
        public BlobStorageHandler(string storageAccountConn, string containerName)
        {
            blobContainer = new BlobContainerClient(storageAccountConn, containerName);
            blobContainer.CreateIfNotExists();
        }

        /// <summary>
        /// Checks if blob with specified filename exists in container
        /// </summary>
        /// <param name="filename">File name string, how it is named in blob storage container</param>
        /// <returns>Boolean if file exists in blob storage container</returns>
        public async Task<bool> FileExists(string filename)
        {
            Log.Information($"Check if file exists in blob storage initialized with filename: {filename}");

            BlobClient blob = blobContainer.GetBlobClient(filename);
            bool exists = await blob.ExistsAsync();

            Log.Information(exists
                ? $"Filename: {filename} exists in container"
                : $"Filename: {filename} is not found in container");

            return exists;
        }

        /// <summary>
        /// Uplad file to blob with specified filename and content type
        /// </summary>
        /// <param name="filename">File name string, how it is named in blob storage container</param>
        /// <returns>Boolean if after upload, blob exists in container</returns>
        public async Task<bool> UploadFileAsync(Stream photoStream, string filename, string contentType)
        {
            Log.Information($"Upload to blob storage action initialized with filename: {filename}");

            BlobClient blob = blobContainer.GetBlobClient(filename);
            await blob.UploadAsync(photoStream, new BlobHttpHeaders { ContentType = contentType });
            bool exists = await blob.ExistsAsync();

            if (exists)
            {
                Log.Information($"New file: {filename} was inserted into blob storage.", filename);
            }
            else
            {
                Log.Warning($"Upload action completed, but file: {filename} does not exists.", filename);
            }

            return exists;
        }

        /// <summary>
        /// Deletes blob if it is found with specified filename
        /// </summary>
        /// <param name="filename">File name string, how it is named in blob storage container</param>
        /// <returns>Boolean if blob was deleted</returns>
        public async Task<bool> DeleteIfExists(string filename)
        {
            Log.Information($"Delete from blob storage action initialized with filename: {filename}");

            BlobClient blob = blobContainer.GetBlobClient(filename);
            bool isDeleted = await blob.DeleteIfExistsAsync();

            Log.Information(isDeleted 
                ? $"Filename: {filename} was deleted from blob storage"
                : $"Filename: { filename} was not found in blob storage");

            return isDeleted;
        }

        /// <summary>
        /// Delete stored blob if its content type is different than specified as in new file
        /// </summary>
        /// <param name="newFilePath">File name string that should replace stored blob</param>
        /// <param name="oldFilePath">File name of blob that is currently stored in container</param>
        /// <returns>Boolean if blob was deleted</returns>
        public async Task<bool> DeleteStoredBlobIfExtensionWillChangeAsync(string newFilePath, string oldFilePath)
        {
            Log.Information($"Delete if extension will change blob storage action initialized" +
                $" with new filename: {newFilePath}, old: {oldFilePath}");
            bool isDeleted = false;

            if(oldFilePath != null && newFilePath != null
              && Path.GetExtension(oldFilePath) != Path.GetExtension(newFilePath))
            {
                Log.Information($"Old blob with different extensions was found. " +
                    $"New file {newFilePath}, old file {oldFilePath}", newFilePath, oldFilePath);

                BlobClient blob = blobContainer.GetBlobClient(oldFilePath);
                isDeleted = await blob.DeleteIfExistsAsync();
            }

            Log.Information(isDeleted 
                    ? $"Blob file: {oldFilePath} was deleted"
                    : $"Blob file: {oldFilePath} was not deleted");

            return isDeleted;
        }
    }
}