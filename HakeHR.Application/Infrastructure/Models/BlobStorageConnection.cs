namespace HakeHR.Application.Infrastructure.Models
{
    /// <summary>
    /// This model contains information needed to connect to blob storage
    /// </summary>
    public class BlobStorageConnection
    {

        /// <summary>
        /// Constructor with property initialization, after initialization model is immutable
        /// </summary>
        /// <param name="storageAccountConn">Azure storage account connection string</param>
        /// <param name="containerName">Azure storage account blob container name</param>
        public BlobStorageConnection(string storageAccountConn, string containerName)
        {
            StorageAccountConn = storageAccountConn;
            ContainerName = containerName;
        }

        /// <summary>
        /// Azure storage account connection string
        /// </summary>
        public string StorageAccountConn { get; }

        /// <summary>
        /// Azure storage account blob container name
        /// </summary>
        public string ContainerName { get; }
    }
}
