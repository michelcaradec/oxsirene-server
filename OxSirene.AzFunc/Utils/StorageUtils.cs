using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OxSirene.AzFunc
{
    internal static class StorageUtils
    {
        public const string CacheContainerName = "cache";
        /// <remarks>
        /// Minimum lease time is 15s.
        /// </remarks>
        private static readonly TimeSpan BlobLeaseTime = TimeSpan.FromSeconds(15);

        private static CloudBlobClient BlobClient
        {
            get
            {
                var account = CloudStorageAccount.Parse(API.Configuration.Instance["AzureWebJobsStorage"]);
                return account.CreateCloudBlobClient();
            }
        }

        private static (string containerName, string blobPath) ParseFileName(string fileName, string delimiter = "/")
        {
            var parts = fileName.Split(delimiter, 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException(fileName, nameof(fileName));
            }

            return (parts[0], parts[1]);
        }

        public static async Task WriteContentAsync(string fileName, string content)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var client = BlobClient;
            var (containerName, blobPath) = ParseFileName(fileName, client.DefaultDelimiter);
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobPath);

            if (await blob.ExistsAsync())
            {
                string leaseID = await blob.AcquireLeaseAsync(BlobLeaseTime);
                var accessCondition = AccessCondition.GenerateLeaseCondition(leaseID);

                try
                {
                    await blob.UploadTextAsync(
                        content,
                        accessCondition,
                        null,
                        null
                    );
                }
                catch (SystemException e)
                {
                    API.Configuration.Instance.LogError(e.ToString());
                    throw;
                }
                finally
                {
                    await blob.ReleaseLeaseAsync(accessCondition);
                }
            }
            else
            {
                await blob.UploadTextAsync(content);
            }
        }

        public static async Task<string> ReadContentAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var client = BlobClient;

            var (containerName, blobPath) = ParseFileName(fileName, client.DefaultDelimiter);
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobPath);

            if (await blob.ExistsAsync())
            {
                return await blob.DownloadTextAsync();
            }
            else
            {
                return null;
            }
        }
    }
}