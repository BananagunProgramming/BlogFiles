
using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EntityManager.Domain.Services
{
    public class AzureWriter
    {
        public void Audit(string message)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("AuditLog");
            table.CreateIfNotExists();

            var azureAppender = new AzureAudit(message);
            var insertOperation = TableOperation.Insert(azureAppender);

            table.Execute(insertOperation);
        }

        public void Error(Exception ex)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("ErrorLog");
            table.CreateIfNotExists();

            var azureAppender = new AzureError(ex);
            var insertOperation = TableOperation.Insert(azureAppender);

            table.Execute(insertOperation);
        }
    }
}
