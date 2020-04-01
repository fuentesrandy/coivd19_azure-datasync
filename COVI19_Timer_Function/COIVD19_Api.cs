using System;
using System.Threading.Tasks;
using Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Common.Entities;
using Newtonsoft.Json;
using Common.Model;

namespace COVI19_Function
{
    public class COIVD19_Api
    {
        private readonly ICOVIDService _COVIDService;

        public COIVD19_Api(ICOVIDService COVIDService)
        {
            _COVIDService = COVIDService;
        }


        [FunctionName("CheckGithub")]
        public async Task CheckGithubAsync([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer
            , [Table("SyncStatus", Connection = "AzureWebJobsStorage")] CloudTable SyncStatusTable
            , [Queue("COIVD19FilesToProcess"), StorageAccount("AzureWebJobsStorage")] ICollector<string> queue
            , ILogger log)
        {
            int fileCount = 0;
            DateTime? lastSyncDate = null;
           
            await SyncStatusTable.CreateIfNotExistsAsync();
            string filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "CheckGithub");
            var query = new TableQuery<SyncStatusEntity>()
                .Where(filter);
            var segment = await SyncStatusTable.ExecuteQuerySegmentedAsync(query, null);
            var lastSync = segment.Select(x => x).FirstOrDefault();
            if (lastSync != null)
            {
                lastSyncDate = lastSync.SyncDate;
            }

            var commits = await _COVIDService.GetCommitsAsync();

            if (lastSyncDate.HasValue)
            {
                commits = commits.Where(x => x.commit.author.date < lastSyncDate.Value);
            }


            foreach (var commit in commits)
            {
                var files = await _COVIDService.GetCommitFileListAsync(commit);
                foreach (var file in files)
                {
                    var queueMsg = JsonConvert.SerializeObject(file);
                    queue.Add(queueMsg);
                    fileCount++;
                    log.LogInformation($"processing {queueMsg}");
                }
            }



            var operation = TableOperation.InsertOrMerge(new SyncStatusEntity("CheckGithub", fileCount));
            var result = await SyncStatusTable.ExecuteAsync(operation);

        }


        [FunctionName("Process_File")]
        public async Task ProcessFileAsync(
          [QueueTrigger("COIVD19FilesToProcess", Connection = "AzureWebJobsStorage")]string queueItem
          , [Table("SyncStatus", Connection = "AzureWebJobsStorage")] CloudTable SyncStatusTable
          , [Table("ErrorLogs", Connection = "AzureWebJobsStorage")] CloudTable ErrorLogsTable
          , [Table("Covid19", Connection = "AzureWebJobsStorage")] CloudTable Covid19Table
          , ILogger log)
        {

            await Covid19Table.CreateIfNotExistsAsync();
            await ErrorLogsTable.CreateIfNotExistsAsync();
            int count = 0;
            List<ErrorLogEntity> Errors = new List<ErrorLogEntity>();
            GitCommitFile file = JsonConvert.DeserializeObject<GitCommitFile>(queueItem);
            var covidEntities = await _COVIDService.GetFileContentAsEntityListAsync(file);

            log.LogInformation($"Start: Processing {file.raw_url}");

            foreach (var entity in covidEntities)
            {
                try
                {
                    var coividInserOperation = TableOperation.InsertOrMerge(entity);
                    var covidResult = await Covid19Table.ExecuteAsync(coividInserOperation);
                    count++;
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Source:{file.raw_url} -- Error inserting into Azure Table Storage {ex.Message}, Data: {JsonConvert.SerializeObject(entity)}";
                    log.LogError(errorMsg);
                    Errors.Add(new ErrorLogEntity(file.raw_url, errorMsg));
                }
            }


            if (Errors.Any())
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                Errors.ForEach(batchOperation.Insert);
                await ErrorLogsTable.ExecuteBatchAsync(batchOperation);
            }


            log.LogInformation($"Done: Processing {file.raw_url}");
            var operation = TableOperation.InsertOrMerge(new SyncStatusEntity("Process_File", count));
            var result = await SyncStatusTable.ExecuteAsync(operation);

        }
    }
}
