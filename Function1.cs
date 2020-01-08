using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ImageUpdater
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run([TimerTrigger("0 */60 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            //search the database for items to be updated (last modified within the 24 hours time window)

            //get the last run time 
            DateTime lastruntime = DateTime.Now;

            //try to get the last run time from Azure Storage (or if it doesn’t exist use DateTime.Now)
            string connectionString = $"<AZURE_STORAGE_CONNECTION_STRING>";

            // Setup the connection to the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Connect to the blob storage
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();

            // Connect to the blob container
            CloudBlobContainer container = serviceClient.GetContainerReference("timestamp");
            // Connect to the blob file
            CloudBlockBlob blob = container.GetBlockBlobReference("lastruntime.txt");

            if (await blob.ExistsAsync())
            {
                // Get the blob file as text
                string lastruntimeString = await blob.DownloadTextAsync();
                DateTime.TryParse(lastruntimeString, out lastruntime);
            }

            log.LogInformation("Last Run Time: " + lastruntime.ToString());

            //record a new run time, but do not update until the application completes 
            //  because if it errors out, we will not know how far we made it and will have to start from the last known good run time.
            DateTime currentRunTime = DateTime.Now;

            //do work.... go ahead and query the api with the lastruntime datetime (to see what changed)
            // do more work.... go ahead and download the images that changed.
            //  do even more work.... resize those images and upload somewhere for the public


            // now that we have completed, update the new last run time (currentRunTime) into Azure Storage 
            log.LogInformation("New Run Time: " + currentRunTime.ToString());
            await blob.UploadTextAsync(currentRunTime.ToString());
            log.LogInformation("Updloaded New Run Time to Azure Stoage");

        }
    }
}
