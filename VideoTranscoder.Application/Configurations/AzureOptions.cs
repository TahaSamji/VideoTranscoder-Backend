// Models/AzureBlobOptions.cs
namespace VideoTranscoder.VideoTranscoder.Application.Configurations
{
    public class AzureOptions
    {
        // The full Azure Blob Storage connection string
        public required string ConnectionString { get; set; }

        // The name of the Blob container where files are stored
        public required string ContainerName { get; set; }

        // The Azure Storage account name
        public required string AccountName { get; set; }

        // The Azure Storage account key used for authentication
        public required string AccountKey { get; set; }
    }
}
