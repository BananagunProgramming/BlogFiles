using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace EntityManager.Domain.Services
{
    public class AzureAudit : TableEntity
    {
        private const string MessagePartionKey = "Audit";
        private const string DateFormat = "yyyyMMdd ; HH:mm:ss:fffffff";
        private const string RowKeyFormat = "{0} - {1}";

        public string AuditMessage { get; set; }

        public AzureAudit() { }

        public AzureAudit(string auditMessage)
        {
            PartitionKey = MessagePartionKey;
            var date = DateTime.Now.ToUniversalTime().ToString(DateFormat);
            RowKey = string.Format(RowKeyFormat, date, Guid.NewGuid());
            AuditMessage = auditMessage;
        }
    }
}
