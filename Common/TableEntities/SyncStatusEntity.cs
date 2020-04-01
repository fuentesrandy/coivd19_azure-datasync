using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Entities
{
    public class SyncStatusEntity : TableEntity
    {

        public DateTime SyncDate { get; set; }
        public int ItemCount { get; set; }


        public SyncStatusEntity()
        {

        }

        public SyncStatusEntity(string rowKey, int fileCount)
        {
            this.PartitionKey = "SyncStatus";
            this.RowKey = rowKey;
            SyncDate = DateTime.UtcNow;
            this.ItemCount = fileCount;
        }
    }
}