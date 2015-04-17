using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace EntityManager.Domain.Services
{
    public class AzureError : TableEntity
    {
        private const string MessagePartionKey = "Error";
        private const string DateFormat = "yyyyMMdd ; HH:mm:ss:fffffff";
        private const string RowKeyFormat = "{0} - {1}";
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string ErrorMessage { get; set; }

        public AzureError() { }

        public AzureError(Exception ex)
        {
            PartitionKey = MessagePartionKey;
            var date = DateTime.Now.ToUniversalTime().ToString(DateFormat);
            RowKey = string.Format(RowKeyFormat, date, Guid.NewGuid());
            ErrorMessage = ex.Message;
            Source = ex.Source;
            StackTrace = ex.StackTrace;
            //InnerExceptionMessage = ex.InnerException.Message;
        }
    }
}
