using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Entities
{
    public class ErrorLogEntity : TableEntity
    {
        public string Filename { get; set; }
        public string Error { get; set; }

        public ErrorLogEntity(string Filename, string Error)
        {
            this.PartitionKey = DateTime.UtcNow.ToString("MM-dd-yyyy");
            this.RowKey = Guid.NewGuid().ToString();
            this.Filename = Filename;
            this.Error = Error;
        }
    }
}